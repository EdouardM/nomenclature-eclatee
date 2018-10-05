#r "../packages/Deedle/lib/net40/Deedle.dll"
#r "../packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#load "./Bom.fsx"
#load "./DFClassif.fsx"

open System
open FSharp.Data
open Deedle

open DFClassif
open Classification

open Bom

let [<Literal>] basePath = __SOURCE_DIRECTORY__ + @"../../data/"

module BomData =
    open Deedle.Frame
    
    let [<Literal>] codeVentePath = basePath + "nomenclatures_codes_ventes_actifs.csv"
    let [<Literal>] sf1Path = basePath + "nomenclatures_SF1_actifs.csv"
    let [<Literal>] sf2Path = basePath + "nomenclatures_SF2_actifs.csv"

    type BomData = CsvProvider<sf1Path, Schema= Bom.CsvFile.schema,HasHeaders=true,Separators=";",Culture="fr-FR">
    type BomRow = BomData.Row

    let csvBomCV = BomData.Load(codeVentePath)
    let csvBomSF1 = BomData.Load(sf1Path)
    let csvBomSF2 = BomData.Load(sf2Path)


    let toObs (row: BomRow): Observation = 
        {
            CodeProduit = row.CodeProduit
            Designation = row.Designation
            Variante = row.Variante
            Evolution = row.Evolution
            CodeFamilleLog = row.CodeFamilleLog
            Nature = row.Nature
            Quantite = row.Quantite
            CodeComposant = row.CodeComposant
            DesignationComposant = row.DesignationComposant
            VarianteComposant = row.VarianteComposant
            QuantiteComposant = row.QuantiteComposant
            SousEnsembleComposant = row.SousEnsembleComposant
        }

    let obs = 
        let cvs = csvBomCV.Rows |> Seq.map toObs
        let sf1 = csvBomSF1.Rows |> Seq.map toObs
        let sf2 = csvBomSF2.Rows |> Seq.map toObs

        Seq.concat [ cvs; sf1; sf2 ]

    let df =  Deedle.Frame.ofRecords obs
        

    module Transforms = 
        let dfClassif = dfClassif

        let addSousEnsmbleProduit (df: Frame<int, string>) = 
            let ssE = dfClassif.GetColumn File.sousEnsemble
            let ssEnsembleCol = 
                df.GetColumn<string> InfoProduit.codeProduit
                |> Series.mapValues(fun c -> 
                    let code = Code c
                    Series.lookup code Lookup.Exact ssE
                )
            
            Frame.addCol File.sousEnsemble ssEnsembleCol df
                

        let addNatureCompo (df: Frame<int, string>) = 
            let nature = dfClassif.GetColumn File.nature
            let natureCol: Series<int, string option> = 
                df.GetColumn<string> InfoComposants.codeComposant
                |> Series.mapValues(fun c ->
                    let code = Code c
                    Series.tryLookup code Lookup.Exact nature
                )
            
            Frame.addCol InfoComposants.natureComposant natureCol df
                
        let dfComplete : Frame<int,string>  =
            df
            |> addSousEnsmbleProduit
            |> addNatureCompo
        
    open Transforms

    let cleanDF : Frame<int,string> = dfComplete


let dfBom = 
    let cleanDF = BomData.cleanDF
    let pathBomClean = basePath + "dfbom.csv"

    cleanDF.SaveCsv(path=pathBomClean, separator=';')
    cleanDF
