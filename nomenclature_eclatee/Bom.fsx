namespace Bom
open System

module InfoProduit = 

    module ColTypes = 
        let [<Literal>] codeProduitT = "string"
        let [<Literal>] versionVarianteT = "string"
        let [<Literal>] evolutionT = "string" 
        let [<Literal>] libelleT = "string"
        let [<Literal>] codeFamilleLogT = "string"
        let [<Literal>] natureT = "string"
        let [<Literal>] quantiteT = "int"
        
    let [<Literal>] codeProduit = "CodeProduit"
    let [<Literal>] versionVariante = "VersionVariante"
    let [<Literal>] evolution = "Evolution"
    let [<Literal>] libelle = "Libelle"
    let [<Literal>] codeFamilleLog = "CodeFamilleLog"
    let [<Literal>] nature = "Nature"
    let [<Literal>] quantite = "Quantite"

    open ColTypes

    let [<Literal>] schema = 
        codeProduit         + " (" + codeProduitT       + "), "
        + versionVariante   + " (" + versionVarianteT   + "), "
        + evolution         + " (" + evolutionT         + "), "
        + libelle           + " (" + libelleT           + "), "
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
    Quantite : int
    CodeComposant: string option
    VersionComposant : string
    QuantiteComposant: float option
    SousEnsemble : string option
}


module CsvFile = 

    let [<Literal>] schema = 
        InfoProduit.schema + "," + InfoComposants.schema