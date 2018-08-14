#load "..\packages/FsLab/FsLab.fsx"
#load "./DFBom.fsx"

open DFBom
open Bom
open BomData
open Deedle.Frame
open Deedle.Series

let df = dfBom

type BomId = 
    {
        CodeProduit : string
        Variante : string
        Evolution : string
    }

//Regroupement des lignes par code produit:
let byBomId =
    df
    |> groupRowsUsing (fun _ c -> 
        { 
            CodeProduit = c.GetAs<string>(InfoProduit.codeProduit) 
            Variante = c.GetAs<string>(InfoProduit.versionVariante)
            Evolution = c.GetAs<string>(InfoProduit.evolution)
        }        
    )
    |> nest
    |> observations
    |> Map.ofSeq

let getComponents (id: BomId): seq<string * string * float> = 
    byBomId.[id]
    |> sliceCols InfoComposants.list
    |> mapRowValues(fun c -> 
        let code = c.GetAs<string> InfoComposants.codeComposant
        let version = c.GetAs<string> InfoComposants.versionComposant
        let quantity = c.GetAs<float> InfoComposants.quantiteComposant
        
        code, version , quantity
    )
    |> values

getComponents {CodeProduit = "10003"; Variante ="1"; Evolution ="1"} |> Seq.head
