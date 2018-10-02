#r "../packages/Deedle/lib/net40/Deedle.dll"
#load "./DFClassif.fsx"
#load "./nomenclature_eclatee.fsx"

open Nomenclature_eclatee


byBomId.Get {CodeProduit = "25708"; Variante = "1"; Evolution = "1"; SousEnsemble = ""; Nature = "V" }


(****************)
//TEST
getComponents {CodeProduit = "25708"; Variante = "1"; Evolution = "1"; SousEnsemble = ""; Nature = "V"} [] 1
|> Option.map collectComponents
|> Option.map Seq.ofList
(****************)
