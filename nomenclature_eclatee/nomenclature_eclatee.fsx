#load "..\packages/FsLab/FsLab.fsx"
open XPlot.GoogleCharts

open Deedle
open FSharp.Data

let [<Literal>] Sourcedir = """c:\Users\FREDOMOU\Documents\04. Dev\nomenclature_eclatee"""

let [<Literal>] NomPath = Sourcedir + "/data/nomenclatures.csv"

let [<Literal>] NomSchema =
    "Code produit(string), Version de la variante(string), Evolution(string), \
    Libellé (string), Code Famille Logistique(int option), Nature du produit(string), \
    Quantité(int), Code Composant(string option),Version(string option), \
    Quantité composant(int option), Sous ensemble(string option)"

type Nomenclature = CsvProvider<NomPath, ";", Schema = NomSchema>
type NomRow = Nomenclature.Row

let nom = Nomenclature.Load(NomPath)
