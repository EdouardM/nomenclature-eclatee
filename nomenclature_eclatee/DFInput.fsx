#r "../packages/Deedle/lib/net40/Deedle.dll"
#r "../packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#load "./Input.fsx"
#load "./Types.fs"

open System
open FSharp.Data
open Deedle

open Input


let [<Literal>] basePath = __SOURCE_DIRECTORY__ + @"../../data/"

module InputData =
    open Deedle.Frame
    
    let [<Literal>] inputPath = basePath + "input.csv"

    type InputData = CsvProvider<inputPath, Schema= Input.CsvFile.schema,HasHeaders=true,Separators=";",Culture="fr-FR">
    type InputRow = InputData.Row

    let csvInput = InputData.Load(inputPath)
    
    let toObs (row: InputRow): Observation = 
        {
            CodeProduit = row.CodeProduit
            Quantite = row.Quantite
        }

    let df = 
        csvInput.Rows 
        |> Seq.map toObs
        |> Frame.ofRecords
    
    module Transforms = 
        let indexByCodeProduit (df: Frame<'R, string>) = 
           df 
           |> Frame.groupRowsByString Input.codeProduit
           //|> (fun df -> df.GetColumn<float> Input.quantite)
           //|> Series.applyLevel fst (Series.reduceValues (+))
           //|> Frame.ofRecords
           |> Frame.mapRowKeys (fun (c,i) -> Code c, i)
                          

    open Transforms
    
    let cleanDF : Frame<Code * int,string> = 
        df
        |> indexByCodeProduit

let dfInput = 
    let cleanDF = InputData.cleanDF
    let pathInputClean = basePath + "dfinput.csv"

    cleanDF.SaveCsv(path=pathInputClean, separator=';')
    cleanDF
