#r "../packages/Deedle/lib/net40/Deedle.dll"
#load "./DFClassif.fsx"
#load "./nomenclature_eclatee.fsx"

open Nomenclature_eclatee
open Deedle

byBomId.Get {CodeProduit = Code "13437"; Nature = Nature "V" }


(****************)
//TEST
getComponents {CodeProduit = Code "13437"; Nature = Nature "V"} [] (Level 1)
|> Option.map collectComponents
|> Option.map Seq.ofList
(****************)


byBomIdAllLevel
|> Series.tryLookup {CodeProduit = Code "13437"; Nature = Nature "V"} Lookup.Exact
|> Option.map (List.filter (fun compo -> compo.CodeComposant.Value = "120630"))