#r "../packages/Deedle/lib/net40/Deedle.dll"
#load "./DFClassif.fsx"
#load "./nomenclature_eclatee.fsx"

open Nomenclature_eclatee
open Deedle

byBomId.Get {CodeProduit = "13437"; Variante = "2"; Evolution = "1"; SousEnsemble = ""; Nature = "V" }


(****************)
//TEST
getComponents {CodeProduit = "13437"; Variante = "2"; Evolution = "1"; SousEnsemble = ""; Nature = "V"} [] 1
|> Option.map collectComponents
|> Option.map Seq.ofList
(****************)


byBomIdAllLevel
|> Series.tryLookup {CodeProduit = "13437"; Variante = "2"; Evolution = "1"; SousEnsemble = ""; Nature = "V"} Lookup.Exact
|> Option.map (List.filter (fun compo -> compo.CodeComposant = "120630"))