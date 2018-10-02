#r "../packages/Deedle/lib/net40/Deedle.dll"
#load "./DFClassif.fsx"
#load "./nomenclature_eclatee.fsx"

open Classification
open Deedle 
open System
open Nomenclature_eclatee
open DFClassif


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

type BomCompoOutput = {
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

let searchBomCompo (compos: string list) (fileName: string) = 
    filterComponents compos
    |> seriesAllLevelsToFrameOutput
    |> frameToCsv fileName

//82188 - 82289
searchBomCompo ["82188"; "82294"] "output.csv"

