#r "../packages/Deedle/lib/net40/Deedle.dll"
#load "./DFClassif.fsx"
#load "./nomenclature_eclatee.fsx"

open Deedle 
open System
open Nomenclature_eclatee
open DFClassif

let filterProduct (product: string) = 
    byBomIdAllLevel
    |> Series.filter(fun bomId _ -> 
        bomId.CodeProduit = product
    )

let filterProducts (products: string list) =
    products
    |> List.map filterProduct
    |> List.reduce(fun s1 s2 -> 
        Series.mergeUsing UnionBehavior.PreferLeft s1 s2
    ) 


type AggregatedBomCompo = {
    CodeComposant : string
    DesignationComposant : string option
    NatureComposant : string option
    Quantite: float
    SousEnsemble: string
}

let aggregateCompo (series: Series<BomId, BomCompo list>) = 
    series
    |> Series.mapValues(fun compos -> 
        compos 
        |> List.groupBy(fun compo -> (compo.CodeComposant, compo.Version, compo.DesignationComposant, compo.NatureComposant, compo.SousEnsemble))
        |> List.map(fun ((code, version, designation, nature, ssE), level) ->
            {
                CodeComposant = code
                DesignationComposant = designation
                NatureComposant = nature
                Quantite =          
                    level
                    |> List.map (fun c -> c.Quantite)
                    |> List.reduce (+) 
                SousEnsemble = ssE                
            }
        )
    )
    
type AggregatedBomCompoOutput = {
    CodeProduit : string
    Variante: string
    DesignationProduit : string
    CodeComposant : string
    DesignationComposant : string
    NatureComposant : string
    QuantiteCompo : float
    SousEnsembleComposant: string
}

let toAggregatedBomCompoOutputFrame (series: Series<BomId, AggregatedBomCompo list>) = 
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
                
                CodeComposant = compo.CodeComposant
                SousEnsembleComposant = compo.SousEnsemble
                NatureComposant = Option.defaultValue "" compo.NatureComposant
                DesignationComposant = Option.defaultValue "" compo.DesignationComposant
                QuantiteCompo = compo.Quantite
            }) )
    |> Frame.ofRecords        

let aggregateBomCompo (products: string list) (fileName: string) = 
    filterProducts products
    |> aggregateCompo
    |> toAggregatedBomCompoOutputFrame
    |> frameToCsv fileName

aggregateBomCompo  ["23905"; "24912"] "output2.csv"