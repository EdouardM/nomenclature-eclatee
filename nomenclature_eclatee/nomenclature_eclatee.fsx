#load "..\packages/FsLab/FsLab.fsx"
#load "./DFBom.fsx"

open DFBom
open Bom
open BomData
open Deedle.Frame
open Deedle.Series
open Deedle 

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
    |> groupRowsUsing (fun _ c -> 
        { 
            CodeProduit = c.GetAs<string>(InfoProduit.codeProduit) 
            Variante = c.GetAs<string>(InfoProduit.versionVariante)
            Evolution = c.GetAs<string>(InfoProduit.evolution)
        }        
    )
    |> nest
    |> observations
    |> Map.ofSeq

type BomCompo = {
    CodeComposant : string
    Version : string
    Quantite: float
}

let toBomCompo code version quantity = 
    { CodeComposant = code; Version =  version; Quantite = quantity }

let toBomCompoList (df: Deedle.Frame<'R, string>) = 
        df
        |> sliceCols InfoComposants.list
        |> mapRowValues(fun c -> 
            let code = c.GetAs<string option> InfoComposants.codeComposant
            let version = c.GetAs<string> InfoComposants.versionComposant
            let quantity = c.GetAs<float option> InfoComposants.quantiteComposant
            
            Option.map3 toBomCompo code (Some version) quantity
        )
        |> values   
        |> List.ofSeq
        |> List.choose id

let getComponents bomId = 
    Map.tryFind bomId byBomId
    |> Option.map toBomCompoList
    
let toBomId (compo: BomCompo) = 
    if String.isEmpty(compo.Version) 
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
                    |> List.map(fun child -> { child with Quantite = c.Quantite * child.Quantite} )
            
            let acc' = List.concat [ acc; newAcc ]
            loop cs acc'             
    loop l []  

getComponents {CodeProduit = "10054"; Variante = "1"; Evolution = "1" }
|> Option.map collectComponents
|> Option.map Seq.ofList


let byBomIdAllLevel = 
    byBomId
    |> Map.toSeq
    |> Seq.map fst
    |> Seq.choose(fun bomId -> 
        let compos = 
            getComponents bomId
            |> Option.map collectComponents
            |> Option.map (fun compos -> 
                compos 
                |> List.groupBy(fun compo -> compo.CodeComposant, compo.Version)
                |> List.map(fun (key, compos) -> key, compos |> List.sumBy (fun c -> c.Quantite) )
                |> List.map(fun ( (code, version), quantite ) -> {CodeComposant = code; Version = version; Quantite = quantite }
            ))
        match compos with
        | Some cs -> Some (bomId, cs)
        | None -> None
    )
    |> Map.ofSeq
   

byBomIdAllLevel.[ {CodeProduit = "10054"; Variante = "1"; Evolution = "1" } ]
|> List.length

