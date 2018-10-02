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



type BomId = 
    {
        CodeProduit : string
        Variante : string
        Nature : string
        Evolution : string
        SousEnsemble: string
    }



//Regroupement des lignes par code produit:
let byBomId =
    dfBom
    |> Frame.groupRowsUsing (fun _ c -> 
        { 
            CodeProduit = c.GetAs<string>(InfoProduit.codeProduit) 
            Variante = c.GetAs<string>(InfoProduit.versionVariante)
            Evolution = c.GetAs<string>(InfoProduit.evolution)
            Nature = c.GetAs<string>(InfoProduit.nature)
            SousEnsemble = c.GetAs<string>(File.sousEnsemble)
        }        
    )
    |> Frame.nest


type Parent = 
    {
        CodeParent : string
        SousEnsemble : string
    }

type BomCompo = {
    CodeComposant : string
    Version : string
    DesignationComposant : string option
    NatureComposant : string option
    Quantite: float
    Parents: Parent list
    SousEnsemble: string
    Level : int
    }

let toBomCompo code version designation nature quantity parents sousEnsemble level = 
    { 
        CodeComposant = code; 
        Version =  version;
        DesignationComposant = designation;
        NatureComposant = nature;
        Quantite = quantity;
        Parents = parents; 
        SousEnsemble = sousEnsemble 
        Level = level
    }

let toBomCompoList 
    (bomId: BomId) 
    (parents: Parent list) 
    (level: int)
    (df: Deedle.Frame<'R, string>) = 
        df        
        |> Frame.sliceCols InfoComposants.list
        |> Frame.mapRowValues(fun c -> 
            let code = c.GetAs<string> InfoComposants.codeComposant
            let version = c.GetAs<string> InfoComposants.versionComposant
            let nature = c.GetAs<string option> InfoComposants.natureComposant
            let designation = c.GetAs<string option> InfoComposants.designationComposant
            let q = c.GetAs<float> InfoComposants.quantiteComposant
            let ssE = c.GetAs<string> InfoComposants.sousEnsembleComposant
    
            //add the list of parents: actual bomId + its parents
            let ps = { CodeParent = bomId.CodeProduit; SousEnsemble = bomId.SousEnsemble } :: parents 
            
            toBomCompo code version designation nature q ps ssE level
        )
        |> Series.values   
        |> List.ofSeq

let getComponents bomId parents level = 
    Series.tryGet bomId byBomId
    |> Option.map ( toBomCompoList bomId parents level )

let toBomId (compo: BomCompo) = 
    let nature = Option.defaultValue "" compo.NatureComposant
    if String.IsNullOrEmpty(compo.Version) 
    then { CodeProduit = compo.CodeComposant; Variante = "1"; Evolution = "1"; SousEnsemble = compo.SousEnsemble; Nature = nature }
    else { CodeProduit = compo.CodeComposant; Variante = compo.Version ; Evolution = "1"; SousEnsemble = compo.SousEnsemble; Nature = nature }

let collectComponents (l : BomCompo list) =
    let rec loop (l: BomCompo list) (acc: BomCompo list) = 
        match l with
        | [] -> acc
        | c :: cs -> 
            let bomId = toBomId c
            let compos = getComponents bomId (c.Parents) (c.Level + 1)
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
            getComponents bomId [] 1
            |> Option.map collectComponents
            |> Option.map (fun cs -> bomId, cs ))
    |> series

let formatParents (parents: Parent list) = 
    parents
    |> List.map (fun p -> 
        match p.SousEnsemble with
        | "" -> p.CodeParent
        | ssE -> p.CodeParent + " (" + p.SousEnsemble + ")")

let formatCodeCompo code (level: int) = 
    let blank = String.init (level - 1) (fun _ -> "    " )
    blank + code
    

let frameToCsv (fileName: string) (frameOutput: Frame<int,string>)  = 
    let outputPath = basePath + fileName
    frameOutput.SaveCsv(outputPath, separator=';')