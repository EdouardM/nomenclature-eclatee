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
    |> Series.mapKeys(fun bomId -> bomId.CodeProduit )
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

let multiply (input: Series<Code, float>) (series: Series<Code, AggregatedBomCompo list>)=
    series 
    |> Series.map(fun bomId compos -> 
        let q = Series.lookup bomId Lookup.Exact input
        compos |> List.map(fun compo -> { compo with Quantite = compo.Quantite * q}) )

    
type AggregatedBomCompoOutput = {
    CodeProduit : string
    DesignationProduit : string
    CodeComposant : string
    DesignationComposant : string
    NatureComposant : string
    QuantiteCompo : float
    SousEnsembleComposant: string
}

let toAggregatedBomCompoOutputFrame (series: Series<Code, AggregatedBomCompo list>) = 
    let des  = dfClassif.GetColumn File.libelle
        
    series
    |> Series.observations
    |> Seq.collect(fun (code, compos) -> 
        compos
        |> List.map(fun compo -> 
            {
                CodeProduit = code.Value
                DesignationProduit = 
                                Series.tryLookup code Lookup.Exact des
                                |> Option.defaultValue ""
                
                CodeComposant = compo.CodeComposant
                SousEnsembleComposant = compo.SousEnsemble
                NatureComposant = compo.NatureComposant
                DesignationComposant = compo.DesignationComposant
                QuantiteCompo = compo.Quantite
            }) )
    |> Frame.ofRecords        

let aggregateBomCompo (input: Series<Code, float>) (fileName: string) = 
    let products = 
        input.Keys 
        |> Seq.map (fun c -> c.Value) 
        |> List.ofSeq

    filterProducts products
    |> aggregateCompo
    |> multiply input
    |> toAggregatedBomCompoOutputFrame
    |> frameToCsv fileName


let s = 
    [Code "155315" => 2. ]
    |> series 

#load "./DFInput.fsx"

open DFInput
open Input

let input =dfInput.GetColumn<float> Input.quantite

aggregateBomCompo input "output2.csv"
