#load "../packages/FsLab/FsLab.fsx"
#load "./Bom.fsx"


open System
open Deedle
open FSharp.Data

open Bom

let [<Literal>] basePath = __SOURCE_DIRECTORY__ + @"../../data/"

module BomData =
    let [<Literal>] path = basePath + "nomenclatures.csv"

    type BomData = CsvProvider<path, Schema= Bom.CsvFile.schema,HasHeaders=true,Separators=";",Culture="fr-FR">
    type BomRow = BomData.Row
    
    let csvBom = BomData.Load(path)

    let toObs (row: BomRow) = 
        {
            CodeProduit = row.``Code produit``
            VersionVariante = row.``Version de la variante``
            Evolution = row.Evolution
            Libelle = row.Libellé
            CodeFamilleLog = row.``Code Famille Logistique``
            Nature = row.``Nature du produit``
            Quantite = row.Quantité
            CodeComposant = row.``Code Composant``
            VersionComposant = row.Version
            QuantiteComposant = row.``Quantité composant``
            SousEnsemble = row.``Sous ensemble``
        }

    let obs = csvBom.Rows |> Seq.map toObs

    let df = Frame.ofRecords obs

    module Transforms = 
        ()

    open Transforms

    let cleanDF : Frame<int,string> = df

let dfBom = 
    let cleanDF = BomData.cleanDF
    let pathBomClean = basePath + "dfbom.csv"

    cleanDF.SaveCsv(path=pathBomClean, separator=';')
    cleanDF
    