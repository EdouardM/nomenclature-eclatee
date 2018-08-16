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
        let [<Literal>] codeComposantT = "string option"
        let [<Literal>] versionComposantT = "string"
        let [<Literal>] quantiteComposantT = "float option"
        let [<Literal>] sousEnsembleT = "string option"

    let [<Literal>] codeComposant = "CodeComposant"
    let [<Literal>] versionComposant = "VersionComposant"
    let [<Literal>] quantiteComposant = "QuantiteComposant"
    let [<Literal>] sousEnsemble = "SousEnsemble"

    open ColTypes

    let [<Literal>] schema = 
        codeComposant       + " (" + codeComposantT     + "), "
        + versionComposant  + " (" + versionComposantT  + "), "
        + quantiteComposant + " (" + quantiteComposantT + "), "
        + sousEnsemble      + " (" + sousEnsembleT      + ")"

    let list = [
        codeComposant; versionComposant;
        quantiteComposant; sousEnsemble ]
    

type Observation = {
    CodeProduit : string
    VersionVariante : string
    Evolution : string
    Libelle : string
    CodeFamilleLog : string
    Nature : string
    Quantite : float
    CodeComposant: string option
    VersionComposant : string
    QuantiteComposant: float option
    SousEnsemble : string option
}


module CsvFile = 

    let [<Literal>] schema = 
        InfoProduit.schema + "," + InfoComposants.schema