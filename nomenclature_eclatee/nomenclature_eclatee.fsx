#r "../packages/Deedle/lib/net40/Deedle.dll"
open DFClassif
#load "./DFBom.fsx"
#load "./DFClassif.fsx"
#load "./Classification.fsx"

open DFBom
open DFClassif
open Bom
open BomData
open Classification
open Deedle 
open System

type DF = Frame

let dfClassif = dfClassif

let df = 
    let ssEs = dfClassif.getCol(Classification.File.sousEnsemble)
    let newCol = dfBom.RowKeys
    newCol
    
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


byBomId.Get {CodeProduit = "10057"; Variante = "1"; Evolution = "1" }

byBomId
|> Series.observations
|> Seq.head

type Parent = 
    {
        CodeParent : string
        SousEnsemble : string
    }


type BomCompo = {
    CodeComposant : string
    Version : string
    Quantite: float
    Parents: Parent list
    SousEnsemble: string
    }

let toBomCompo code version quantity parents sousEnsemble = 
    { 
        CodeComposant = code; 
        Version =  version;
        Quantite = quantity;
        Parents = parents; 
        SousEnsemble = sousEnsemble 
    }

let toBomCompoList 
    (bomId: BomId) 
    (parents: Parent list) 
    (df: Deedle.Frame<'R, string>) = 
        df        
        |> Frame.sliceCols InfoComposants.list
        |> Frame.mapRowValues(fun c -> 
            let code = c.GetAs<string option> InfoComposants.codeComposant
            let version = c.GetAs<string> InfoComposants.versionComposant
            let q = c.GetAs<float option> InfoComposants.quantiteComposant
            let ssE = c.GetAs<string> InfoComposants.sousEnsemble

            //add the list of parents: actual bomId + its parents
            let ps = bomId.CodeProduit :: parents 
            
            Option.map3 toBomCompo code (Some version) q
            |> Option.map (fun f -> f ps)
            |> Option.map (fun f -> f ssE )
        )
        |> Series.values   
        |> List.ofSeq
        |> List.choose id

let getComponents bomId parents = 
    Series.tryGet bomId byBomId
    |> Option.map ( toBomCompoList bomId parents )
    
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
            let compos = getComponents bomId (c.Parents)
            let newAcc = 
                match compos with
                | None -> [c]
                | Some children -> 
                    loop children []
                    |> List.map(fun child -> 
                        { child with Quantite = c.Quantite * child.Quantite} )
                    |> fun l -> c :: l                    
            
            let acc' = List.concat [ acc; newAcc ]
            loop cs acc'             
    loop l []  

(****************)
//TEST
getComponents {CodeProduit = "10057"; Variante = "1"; Evolution = "1" } []
|> Option.map collectComponents
|> Option.map Seq.ofList
(****************)


let byBomIdAllLevel = 
    byBomId
    |> Series.keys
    |> Seq.choose(fun bomId -> 
        let compos = 
            getComponents bomId []
            |> Option.map collectComponents
            |> Option.map (fun compos -> 
                compos 
                |> List.groupBy(fun compo -> (compo.CodeComposant, compo.Version, compo.SousEnsemble))
                |> List.map(fun ((code, version, ssEnsemble), level) ->
                    {
                        CodeComposant = code
                        Version = version
                        Quantite =          
                            level
                            |> List.map (fun c -> c.Quantite)
                            |> List.reduce (+) 
                        Parents = 
                            level 
                            |> List.collect (fun c -> c.Parents)
                            |> List.distinct
                        SousEnsemble = ssEnsemble
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

dictBomAllLevel.[ {CodeProduit = "26184"; Variante = "1"; Evolution = "1" } ]
|> List.map(fun c -> int c.CodeComposant)
|> List.sort
(****************)

type BomAllLevels = {
    CodeProduit : string
    Variante: string
    CodeComposant : string
    QuantiteCompo : float
    ParentsCompo : string
}

let dfAllBomLevels = 
    byBomIdAllLevel
    |> Seq.collect(fun (bomId, compos) -> 
        compos
        |> List.map(fun compo -> 
            {
                CodeProduit = bomId.CodeProduit
                Variante = bomId.Variante
                CodeComposant = compo.CodeComposant
                QuantiteCompo = compo.Quantite
                ParentsCompo = compo.Parents |> String.concat ";"
            }) )
    |> Frame.ofRecords        


let saveBomAllLevels () = 
    let outputPath = basePath + "bommAllLevels.csv"

    dfAllBomLevels.SaveCsv(outputPath, separator=';')
    
saveBomAllLevels ()