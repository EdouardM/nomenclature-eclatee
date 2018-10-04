#r "../packages/Deedle/lib/net40/Deedle.dll"
#r "../packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#load "./Classification.fsx"
#load "./Types.fs"

open System
open FSharp.Data
open Deedle

open Classification

let [<Literal>] basePath = __SOURCE_DIRECTORY__ + @"../../data/"

module ClassifData =
    open Deedle.Frame
    let [<Literal>] path = basePath + "classification_produits.csv"    
    
    type ClassifData = CsvProvider<path, Schema= Classification.CsvFile.schema,HasHeaders=true,Separators=";",Culture="fr-FR" >
    type ClassifRow = ClassifData.Row
    
    let csvClassif = ClassifData.Load(path)
    
    let toObs (row: ClassifRow) = 
        {
            CodeProduit = row.CodeProduit
            Libelle = row.Libelle
            CodeFamilleLog = row.CodeFamilleLog
            Nature = row.Nature
            SousEnsemble = row.SousEnsemble
        }

    let obs = csvClassif.Rows |> Seq.map toObs
        
        
    let df =  Deedle.Frame.ofRecords obs
        
    module Transforms = 
        let indexByCodeProduit (df: Frame<'R, string>) = 
           df 
           |> Frame.groupRowsByString File.codeProduit
           |> Frame.mapRowKeys fst                        

    open Transforms
    
    let cleanDF : Frame<Code,string> = 
        df
        |> indexByCodeProduit
        |> Frame.mapRowKeys Code

let dfClassif = 
    let cleanDF = ClassifData.cleanDF
    let pathBomClean = basePath + "dfclassif.csv"

    cleanDF.SaveCsv(path=pathBomClean, separator=';')
    cleanDF
