#r "../packages/Deedle/lib/net40/Deedle.dll"
#load "./DFClassif.fsx"
#load "./nomenclature_eclatee.fsx"

open Deedle 
open System
open Nomenclature_eclatee
open DFClassif
open Classification

let filterProduct (product: string) = 
    byBomIdAllLevel
    |> Series.filter(fun bomId _ -> 
        bomId.CodeProduit.Value = product
    )

let filterProducts (products: string list) =
    products
    |> List.map filterProduct
    |> List.reduce(fun s1 s2 -> 
        Series.mergeUsing UnionBehavior.PreferLeft s1 s2
    ) 


type AggregatedBomCompo = {
    CodeComposant : string
    DesignationComposant : string
    NatureComposant : string
    Quantite: float
    SousEnsemble: string
}

let aggregateCompo (series: Series<BomId, BomCompo list>) = 
    series
    |> Series.mapValues(fun compos -> 
        compos 
        |> List.groupBy(fun compo -> (compo.CodeComposant,compo.DesignationComposant, compo.NatureComposant, compo.SousEnsemble))
        |> List.map(fun ((code,designation, nature, ssE), level) ->
            {
                CodeComposant = code.Value
                DesignationComposant = designation.Value
                NatureComposant = Option.defaultValue (Nature "") nature |> (fun n -> n.Value)
                Quantite =          
                    level
                    |> List.map (fun c -> c.Quantite)
                    |> List.reduce (+) 
                SousEnsemble = ssE.Value                
            }
        )
    )
    
type AggregatedBomCompoOutput = {
    CodeProduit : string
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
                CodeProduit = bomId.CodeProduit.Value
                DesignationProduit = 
                                Series.tryLookup bomId.CodeProduit Lookup.Exact des
                                |> Option.defaultValue ""
                
                CodeComposant = compo.CodeComposant
                SousEnsembleComposant = compo.SousEnsemble
                NatureComposant = compo.NatureComposant
                DesignationComposant = compo.DesignationComposant
                QuantiteCompo = compo.Quantite
            }) )
    |> Frame.ofRecords        

let aggregateBomCompo (products: string list) (fileName: string) = 
    filterProducts products
    |> aggregateCompo
    |> toAggregatedBomCompoOutputFrame
    |> frameToCsv fileName

aggregateBomCompo  ["155315";"155312";"155228";"155230";"155224";"155224";"155226";"155228"] "output2.csv"