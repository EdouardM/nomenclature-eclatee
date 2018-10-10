namespace Input
open System

module Input = 

    module ColTypes = 
        let [<Literal>] codeProduitT = "string"
        let [<Literal>] quantiteT = "float"

    let [<Literal>] codeProduit = "CodeProduit"
    let [<Literal>] quantite = "Quantite"

    open ColTypes

    let [<Literal>] schema = 
        codeProduit         + " (" + codeProduitT       + "), "
        + quantite       + " (" + quantiteT           + ")"

    let list = [ codeProduit;quantite ]

type Observation = {
    CodeProduit : string
    Quantite: float }


module CsvFile = 

    let [<Literal>] schema = Input.schema

