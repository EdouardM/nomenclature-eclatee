namespace Classification
open System

module File = 

    module ColTypes = 
        let [<Literal>] codeProduitT = "string"
        let [<Literal>] libelleT = "string"
        let [<Literal>] natureT = "string"
        let [<Literal>] codeFamilleLogT = "string"
        let [<Literal>] sousEnsembleT = "string"

    let [<Literal>] codeProduit = "CodeProduit"
    let [<Literal>] libelle = "Libelle"
    let [<Literal>] nature = "Nature"
    let [<Literal>] codeFamilleLog = "CodeFamilleLog"
    let [<Literal>] sousEnsemble = "SousEnsemble"

    open ColTypes

    let [<Literal>] schema = 
        codeProduit         + " (" + codeProduitT       + "), "
        + libelle           + " (" + libelleT           + "), "
        + codeFamilleLog    + " (" + codeFamilleLogT    + "), "
        + nature            + " (" + natureT            + "), "
        + sousEnsemble      + " (" + sousEnsembleT      + ")"
    
    let list = [
        codeProduit; libelle; 
        codeFamilleLog; 
        nature; sousEnsemble ] 
        

type Observation = {
    CodeProduit : string
    Libelle : string
    CodeFamilleLog : string
    Nature : string
    SousEnsemble : string
}

module CsvFile = 

    let [<Literal>] schema = File.schema