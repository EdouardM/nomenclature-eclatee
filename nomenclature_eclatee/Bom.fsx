namespace Bom
open System

module InfoProduit = 

    module ColTypes = 
        let [<Literal>] codeProduitT = "string"
        let [<Literal>] libelleT = "string"
        let [<Literal>] versionVarianteT = "string"
        let [<Literal>] evolutionT = "string" 
        let [<Literal>] codeFamilleLogT = "string"
        let [<Literal>] natureT = "string"
        let [<Literal>] quantiteT = "float"
        
    let [<Literal>] codeProduit = "CodeProduit"
    let [<Literal>] libelle = "Libelle"
    let [<Literal>] versionVariante = "VersionVariante"
    let [<Literal>] evolution = "Evolution"
    let [<Literal>] codeFamilleLog = "CodeFamilleLog"
    let [<Literal>] nature = "Nature"
    let [<Literal>] quantite = "Quantite"

    open ColTypes

    let [<Literal>] schema = 
        codeProduit         + " (" + codeProduitT       + "), "
        + libelle           + " (" + libelleT           + "), "
        + versionVariante   + " (" + versionVarianteT   + "), "
        + evolution         + " (" + evolutionT         + "), "
        + codeFamilleLog    + " (" + codeFamilleLogT    + "), "
        + nature            + " (" + natureT            + "), "
        + quantite          + " (" + quantiteT          + ")"
    
    let list = [
        codeProduit; versionVariante; 
        evolution; libelle; 
        codeFamilleLog; 
        nature; quantite ] 
        
    
    
module InfoComposants = 
    module ColTypes = 
        let [<Literal>] codeComposantT = "string"
        let [<Literal>] versionComposantT = "string"
        let [<Literal>] quantiteComposantT = "float"
        let [<Literal>] sousEnsembleComposantT = "string"
        let [<Literal>] natureComposantT = "string"
        

    let [<Literal>] codeComposant = "CodeComposant"
    let [<Literal>] versionComposant = "VersionComposant"
    let [<Literal>] quantiteComposant = "QuantiteComposant"
    let [<Literal>] sousEnsembleComposant = "SousEnsembleComposant"
    let [<Literal>] natureComposant = "NatureComposant"

    open ColTypes

    let [<Literal>] schema = 
        codeComposant       + " (" + codeComposantT     + "), "
        + versionComposant  + " (" + versionComposantT  + "), "
        + quantiteComposant + " (" + quantiteComposantT + "), "
        + sousEnsembleComposant + " (" + sousEnsembleComposantT      + ")"

    let list = [
        codeComposant; versionComposant;
        quantiteComposant; sousEnsembleComposant ]
    

type Observation = {
    CodeProduit : string
    VersionVariante : string
    Evolution : string
    Libelle : string
    CodeFamilleLog : string
    Nature : string
    Quantite : float
    CodeComposant: string
    VersionComposant : string
    QuantiteComposant: float
    SousEnsembleComposant : string
}


module CsvFile = 

    let [<Literal>] schema = 
        InfoProduit.schema + "," + InfoComposants.schema