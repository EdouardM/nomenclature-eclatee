#r "../packages/Deedle/lib/net40/Deedle.dll"
#load "./DFBom.fsx"

open DFBom
open Bom
open BomData
open Deedle 
open System

type DF = Frame

let df = dfBom


type BomId = 
    {
        CodeProduit : string
        Variante : string
        Evolution : string
    }

//Regroupement des lignes par code produit:
let byBomId =
    df
    |> Frame.groupRowsUsing (fun _ c -> 
        { 
            CodeProduit = c.GetAs<string>(InfoProduit.codeProduit) 
            Variante = c.GetAs<string>(InfoProduit.versionVariante)
            Evolution = c.GetAs<string>(InfoProduit.evolution)
        }        
    )
    |> Frame.nest


type BomCompo = {
    CodeComposant : string
    Version : string
    Quantite: float
}

let toBomCompo code version quantity = 
    { CodeComposant = code; Version =  version; Quantite = quantity }

let toBomCompoList (df: Deedle.Frame<'R, string>) = 
        df        
        |> Frame.sliceCols (InfoProduit.quantite ::  InfoComposants.list)
        |> Frame.mapRowValues(fun c -> 
            let code = c.GetAs<string option> InfoComposants.codeComposant
            let version = c.GetAs<string> InfoComposants.versionComposant
            
            let q = c.GetAs<float option> InfoComposants.quantiteComposant
            let qP = c.GetAs<float> InfoComposants.quantiteComposant

            let q2 = Option.map(fun q -> q / qP ) q

            Option.map3 toBomCompo code (Some version) q2
        )
        |> Series.values   
        |> List.ofSeq
        |> List.choose id

let getComponents bomId = 
    Series.tryGet bomId byBomId
    |> Option.map toBomCompoList
    
let toBomId (compo: BomCompo) = 
    if String.IsNullOrEmpty(compo.Version) 
    then { CodeProduit = compo.CodeComposant; Variante = "1"; Evolution = "1" }
    else { CodeProduit = compo.CodeComposant; Variante = compo.Version ; Evolution = "1" }

let collectComponents (l : BomCompo list) =
    let rec loop (l: BomCompo list) (acc: BomCompo list) = 
        match l with
        | [] -> acc
        | c :: cs -> 
            let bomId = toBomId c
            let compos = getComponents bomId
            let newAcc = 
                match compos with
                | None -> [c]
                | Some children -> 
                    loop children []
                    |> List.map(fun child -> 
                        { child with Quantite = c.Quantite * child.Quantite} )
            
            let acc' = List.concat [ acc; newAcc ]
            loop cs acc'             
    loop l []  

(****************)
//TEST
getComponents {CodeProduit = "10054"; Variante = "1"; Evolution = "1" }
|> Option.map collectComponents
|> Option.map Seq.ofList
(****************)


let byBomIdAllLevel = 
    byBomId
    |> Series.keys
    |> Seq.choose(fun bomId -> 
        let compos = 
            getComponents bomId
            |> Option.map collectComponents
            |> Option.map (fun compos -> 
                compos 
                |> List.groupBy(fun compo -> (compo.CodeComposant, compo.Version))
                |> List.map(fun ((code, version), level) ->
                    {
                        CodeComposant = code
                        Version = version
                        Quantite =          
                            level
                            |> List.map (fun c -> c.Quantite)
                            |> List.reduce (+) 
                    }
                )) 
                
               
        match compos with
        | Some cs -> Some (bomId, cs)
        | None -> None
    )

(****************)
//TEST   
let dictBomAllLevel = 
    byBomIdAllLevel
    |> dict  

dictBomAllLevel.[ {CodeProduit = "10054"; Variante = "1"; Evolution = "1" } ]
(****************)

let saveBomAllLevels () = 
    let bomAllLevels = 
        byBomIdAllLevel
        |> List.ofSeq
        |> List.collect(fun (bomId, compos) -> List.map(fun compo -> sprintf "%s;%s;%s;%f" bomId.CodeProduit bomId.Variante compo.CodeComposant compo.Quantite) compos)


    let outputPath = basePath + "bommAllLevels.csv"

    System.IO.File.WriteAllLines(outputPath, bomAllLevels)

    
saveBomAllLevels()