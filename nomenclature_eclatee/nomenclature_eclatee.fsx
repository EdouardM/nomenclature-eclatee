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


let dfClassif = dfClassif

let addSousEnsmbleCompo (df: Frame<int, string>) = 
    let ssE = dfClassif.GetColumn File.sousEnsemble
    let ssEnsembleCol = 
        df.GetColumn<string> InfoProduit.codeProduit
        |> Series.mapValues(fun code -> 
            Series.lookup code Lookup.Exact ssE
        )
    
    Frame.addCol File.sousEnsemble ssEnsembleCol df
        

let addNatureCompo (df: Frame<int, string>) = 
    let nature = dfClassif.GetColumn File.nature
    let natureCol: Series<int, string option> = 
        df.GetColumn<string> InfoComposants.codeComposant
        |> Series.mapValues(fun code -> 
            Series.tryLookup code Lookup.Exact nature
        )
    
    Frame.addCol InfoComposants.natureComposant natureCol df
        
let addDesignationCompo (df: Frame<int, string>) = 
    let des  = dfClassif.GetColumn File.libelle
    let desCol : Series<int, string option> = 
        df.GetColumn<string> InfoComposants.codeComposant
        |> Series.mapValues(fun code -> 
            Series.tryLookup code Lookup.Exact des
        )
    
    Frame.addCol InfoComposants.designationComposant desCol  df


let df : Frame<int,string>  =
    dfBom
    |> addSousEnsmbleCompo
    |> addNatureCompo
    |> addDesignationCompo


df.Columns.Keys
|> Seq.toList
|> List.iter (printfn "%s")

df.GetColumn<string option> InfoComposants.designationComposant
|> Series.values
|> Seq.take 10
|> Seq.toList
|> List.iter (Option.iter (printfn "%s"))


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
    df
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


byBomId.Get {CodeProduit = "25708"; Variante = "1"; Evolution = "1"; SousEnsemble = ""; Nature = "V" }

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

(****************)
//TEST
getComponents {CodeProduit = "25708"; Variante = "1"; Evolution = "1"; SousEnsemble = ""; Nature = "V"} [] 1
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
    |> Series.filter(fun bomId _ -> bomId.Nature = "V"  )

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
    DesignationProduit : string
    Level : string
    CodeComposant : string
    DesignationComposant : string
    NatureComposant :  string
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
    let des  = dfClassif.GetColumn File.libelle
        
    series
    |> Series.observations
    |> Seq.collect(fun (bomId, compos) -> 
        compos
        |> List.map(fun compo -> 
            {
                CodeProduit = bomId.CodeProduit
                Variante = bomId.Variante
                DesignationProduit = 
                                Series.tryLookup bomId.CodeProduit Lookup.Exact des
                                |> Option.defaultValue ""
                
                CodeComposant = formatCodeCompo compo.CodeComposant compo.Level
                SousEnsembleComposant = compo.SousEnsemble
                NatureComposant = Option.defaultValue "" compo.NatureComposant
                DesignationComposant = Option.defaultValue "" compo.DesignationComposant
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

//82188 - 82289
searchBomCompo ["82188"; "82294"] "output.csv"