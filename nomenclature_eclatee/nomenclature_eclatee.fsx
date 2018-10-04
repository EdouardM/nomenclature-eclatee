#r "../packages/Deedle/lib/net40/Deedle.dll"
#load "./DFBom.fsx"
#load "./DFClassif.fsx"
#load "./Classification.fsx"


open DFBom
open Bom
open BomData
open Classification
open Deedle 
open System
open DFClassif


//Regroupement des lignes par code produit:
let byBomId =
    dfBom
    |> Frame.groupRowsUsing (fun _ c -> 
        { 
            CodeProduit = c.GetAs<string>(InfoProduit.codeProduit) |> Code
            Nature = c.GetAs<string>(InfoProduit.nature) |> Nature
        }        
    )
    |> Frame.nest


let toBomCompoList 
    (bomId: BomId) 
    (parents: Parent list) 
    (level: Level)
    (df: Deedle.Frame<'R, string>) = 
        let ssEs = dfClassif.GetColumn<string> File.sousEnsemble
        df        
        |> Frame.sliceCols InfoComposants.list
        |> Frame.mapRowValues(fun c -> 
            let code = c.GetAs<string> InfoComposants.codeComposant |> Code
            let nature = c.GetAs<string option> InfoComposants.natureComposant |> Option.map Nature
            let designation = c.GetAs<string option> InfoComposants.designationComposant |> Option.map Designation
            let q = c.GetAs<float> InfoComposants.quantiteComposant
            let ssE = c.GetAs<string> InfoComposants.sousEnsembleComposant |> SousEnsemble
    
            let ssE = 
                Series.tryLookup bomId.CodeProduit Lookup.Exact ssEs 
                |> Option.defaultValue ""
                |> SousEnsemble

            //add the list of parents: actual bomId + its parents
            let ps = { CodeParent = bomId.CodeProduit ; SousEnsemble = ssE } :: parents 
            
            BomCompo.create code designation nature q ps ssE level
        )
        |> Series.values   
        |> List.ofSeq

let getComponents bomId parents level = 
    Series.tryGet bomId byBomId
    |> Option.map ( toBomCompoList bomId parents level )

let toBomId (compo: BomCompo) = 
    let nature = Option.defaultValue (Nature "") compo.NatureComposant
    { CodeProduit = compo.CodeComposant; Nature = nature }
   

let collectComponents (l : BomCompo list) =
    let rec loop (l: BomCompo list) (acc: BomCompo list) = 
        match l with
        | [] -> acc
        | c :: cs -> 
            let bomId =  toBomId c
            let compos = getComponents bomId (c.Parents) (c.Level.add 1)
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

let byBomIdAllLevel = 
    byBomId
    |> Series.keys
    |> Seq.choose(fun bomId -> 
            getComponents bomId [] (Level 1)
            |> Option.map collectComponents
            |> Option.map (fun cs -> bomId, cs ))
    |> series

let formatParents (parents: Parent list) = 
    parents
    |> List.map (fun p -> 
        let parent = p.CodeParent.Value
        match p.SousEnsemble.Value with
        | "" -> parent
        | ssE -> parent + " (" + ssE + ")")

let formatCodeCompo (code: string) (level: int) = 
    let blank = String.init (level - 1) (fun _ -> "    " )
    blank + code
    

let frameToCsv (fileName: string) (frameOutput: Frame<int,string>)  = 
    let outputPath = basePath + fileName
    frameOutput.SaveCsv(outputPath, separator=';')