#r "../packages/Deedle/lib/net40/Deedle.dll"
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

let df: Frame<int, string> = 
    let ssE = dfClassif.GetColumn File.sousEnsemble
    let ssEnsembleCol = 
        dfBom.GetColumn<string> InfoProduit.codeProduit
        |> Series.mapValues(fun code -> 
            Series.lookup code Lookup.Exact ssE
        )
    
    Frame.addCol File.sousEnsemble ssEnsembleCol dfBom
        
type BomId = 
    {
        CodeProduit : string
        Variante : string
        Evolution : string
        SousEnsemble: string
    }

//Regroupement des lignes par code produit:
let byBomId =
    df
    |> Frame.groupRowsUsing (fun _ c -> 
        { 
            CodeProduit = c.GetAs<string>(InfoProduit.codeProduit) 
            Variante = c.GetAs<string>(InfoProduit.versionVariante)
            Evolution = c.GetAs<string>(InfoProduit.evolution)
            SousEnsemble = c.GetAs<string>(File.sousEnsemble)
        }        
    )
    |> Frame.nest


byBomId.Get {CodeProduit = "25708"; Variante = "1"; Evolution = "1"; SousEnsemble = "" }


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
    Level : int
    }

let toBomCompo code version quantity parents sousEnsemble level = 
    { 
        CodeComposant = code; 
        Version =  version;
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
            let q = c.GetAs<float> InfoComposants.quantiteComposant
            let ssE = c.GetAs<string> InfoComposants.sousEnsembleComposant
    
            //add the list of parents: actual bomId + its parents
            let ps = { CodeParent = bomId.CodeProduit; SousEnsemble = bomId.SousEnsemble } :: parents 
            
            toBomCompo code version q ps ssE level
        )
        |> Series.values   
        |> List.ofSeq

let getComponents bomId parents level = 
    Series.tryGet bomId byBomId
    |> Option.map ( toBomCompoList bomId parents level )

let toBomId (compo: BomCompo) = 
    let ssE = compo.SousEnsemble
    if String.IsNullOrEmpty(compo.Version) 
    then { CodeProduit = compo.CodeComposant; Variante = "1"; Evolution = "1"; SousEnsemble = ssE}
    else { CodeProduit = compo.CodeComposant; Variante = compo.Version ; Evolution = "1"; SousEnsemble = ssE}

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

(****************)
//TEST
getComponents {CodeProduit = "25708"; Variante = "1"; Evolution = "1"; SousEnsemble = ""} [] 1
|> Option.map collectComponents
|> Option.map Seq.ofList
(****************)


let byBomIdAllLevel = 
    byBomId
    |> Series.keys
    |> Seq.choose(fun bomId -> 
            getComponents bomId [] 1
            |> Option.map collectComponents
            |> Option.map (fun cs -> bomId, cs ))
    |> series

//Filter to the bom containing the components: 
let filterComponent (component: string ) = 
    byBomIdAllLevel
    |> Series.filterValues(fun compos -> 
        compos 
        |> List.filter(fun compo -> compo.CodeComposant = component)
        |> List.isEmpty
        |> not    
    )

let filterComponents (components: string list) = 
    components
    |> List.map filterComponent
    |> List.reduce(fun s1 s2 -> 
        Series.mergeUsing UnionBehavior.PreferLeft s1 s2
    ) 


(****************)
//TEST   

filterComponents ["52506"; "57829"]
|> Series.keys
|> Seq.take 10
|> Seq.toList
|> List.map(fun bom -> bom.CodeProduit )

(****************)

type BomAllLevels = {
    CodeProduit : string
    Variante: string
    Level : string
    CodeComposant : string
    SousEnsembleComposant : string
    QuantiteCompo : float
    ParentsCompo : string
}

let formatParents (parents: Parent list) = 
    parents
    |> List.map (fun p -> 
        match p.SousEnsemble with
        | "" -> p.CodeParent
        | ssE -> p.CodeParent + " (" + p.SousEnsemble + ")")

let formatCodeCompo code (level: int) = 
    let blank = String.init (level - 1) (fun _ -> "    " )
    blank + code
    

let seriesAllLevelsToFrameOutput (series: Series<BomId, BomCompo list>) = 
    series
    |> Series.observations
    |> Seq.collect(fun (bomId, compos) -> 
        compos
        |> List.map(fun compo -> 
            {
                CodeProduit = bomId.CodeProduit
                Variante = bomId.Variante
                CodeComposant = formatCodeCompo compo.CodeComposant compo.Level
                SousEnsembleComposant = compo.SousEnsemble
                QuantiteCompo = compo.Quantite
                ParentsCompo = compo.Parents |> formatParents |> String.concat ";"
                Level = string compo.Level
            }) )
    |> Frame.ofRecords        


let saveBomAllLevels (fileName: string) (frameOutput: Frame<int,string>)  = 
    let outputPath = basePath + fileName
    frameOutput.SaveCsv(outputPath, separator=';')
    

let searchBomCompo (compos: string list) (fileName: string) = 
    filterComponents compos
    |> seriesAllLevelsToFrameOutput
    |> saveBomAllLevels fileName

//60768 - 58776 - 82795 - 62529 - 6259 - 62192 - 82337
searchBomCompo ["60768"] "output.csv"