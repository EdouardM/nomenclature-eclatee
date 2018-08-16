#r "../packages/Deedle/lib/net40/Deedle.dll"
#r "../packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#load "./Bom.fsx"


open System
open FSharp.Data
open Deedle

open Bom

let [<Literal>] basePath = __SOURCE_DIRECTORY__ + @"../../data/"

module BomData =
    open Deedle.Frame
    let [<Literal>] path = basePath + "nomenclatures.csv"    
    let [<Literal>] codeVentePath = basePath + "nomenclatures_code_ventes_actifs.csv"
    let [<Literal>] sf1Path = basePath + "nomenclatures_SF1_actifs.csv"
    let [<Literal>] sf2Path = basePath + "nomenclatures_SF2_actifs.csv"

    type BomData = CsvProvider<sf1Path, Schema= Bom.CsvFile.schema,HasHeaders=true,Separators=";",Culture="fr-FR">
    type BomRow = BomData.Row
    
    let csvBomCV = BomData.Load(codeVentePath)
    let csvBomSF1 = BomData.Load(sf1Path)
    let csvBomSF2 = BomData.Load(sf2Path)


    let toObs (row: BomRow) = 
        {
            CodeProduit = row.CodeProduit
            VersionVariante = row.VersionVariante
            Evolution = row.Evolution
            Libelle = row.Libelle
            CodeFamilleLog = row.CodeFamilleLog
            Nature = row.Nature
            Quantite = row.Quantite
            CodeComposant = row.CodeComposant
            VersionComposant = row.VersionComposant
            QuantiteComposant = row.QuantiteComposant
            SousEnsemble = row.SousEnsemble
        }

    let obs = 
        let cvs = csvBomCV.Rows |> Seq.map toObs
        let sf1 = csvBomSF1.Rows |> Seq.map toObs
        let sf2 = csvBomSF2.Rows |> Seq.map toObs

        Seq.concat [ cvs; sf1; sf2 ]

    let df =  Deedle.Frame.ofRecords obs
        

    module Transforms = 
        let fillMissingWithBlank (colname: string) (df: Frame<'R, string>) = 
            let newSeries = 
                df
                |> Frame.getCol colname
                |> Series.fillMissingWith ""
            df |> Frame.replaceCol colname newSeries                        

    open Transforms

    let cleanDF : Frame<int,string> = df


let dfBom = 
    let cleanDF = BomData.cleanDF
    let pathBomClean = basePath + "dfbom.csv"

    cleanDF.SaveCsv(path=pathBomClean, separator=';')
    cleanDF
