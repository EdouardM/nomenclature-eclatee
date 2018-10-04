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
        let code = Code component
        compos 
        |> List.filter(fun compo -> compo.CodeComposant = code)
        |> List.isEmpty
        |> not    
    )
    |> Series.filter(fun bomId _ -> bomId.Nature = Nature "V"  )

let filterComponents (components: string list) = 
    components
    |> List.map filterComponent
    |> List.reduce(fun s1 s2 -> 
        Series.mergeUsing UnionBehavior.PreferLeft s1 s2
    ) 

(****************)
//TEST   

filterComponents ["82294"]
|> Series.keys
|> Seq.take 10
|> Seq.toList
//|> List.map(fun bom -> bom.CodeProduit )

(****************)

type BomCompoOutput = {
    CodeProduit : string
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
                CodeProduit = bomId.CodeProduit.Value
                DesignationProduit = 
                                Series.tryLookup bomId.CodeProduit Lookup.Exact des
                                |> Option.defaultValue ""
                
                CodeComposant = compo.CodeComposant.Value
                SousEnsembleComposant = compo.SousEnsemble.Value
                NatureComposant = 
                    let n = Option.defaultValue (Nature "") compo.NatureComposant
                    n.Value
                DesignationComposant = 
                    let d = Option.defaultValue (Designation "") compo.DesignationComposant
                    d.Value
                QuantiteCompo = compo.Quantite
                ParentsCompo = compo.Parents |> formatParents |> String.concat ";"
                Level = string compo.Level.Value
            }) )
    |> Frame.ofRecords        

let searchBomCompo (compos: string list) (fileName: string) = 
    filterComponents compos
    |> seriesAllLevelsToFrameOutput
    |> frameToCsv fileName

//82188 - 82294
searchBomCompo ["150549";"150550";"120630";"120628";"120642";"120629";"121332";"121400";"121110";"121111";"152819";"152820";"152822";"152823";"152825";"152826";"152828";"152829";"153008";"153009";"153119";"153121";"153120";"123297";"123293";"123258";"123296";"123294";"153224";"153234";"155427";"153223";"153233";"155429";"155430";"155431";"155432";"155433";"155434";"155435";"155436";"155437";"155438";"155439";"155440";"155441";"155442";"155443";"152895";"152890";"152884";"152878";"153012";"155160";"155161"] "output.csv"

searchBomCompo ["82294"; "82188"] "output.csv"
