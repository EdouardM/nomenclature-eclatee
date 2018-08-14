namespace Bom
open System

module InfoProduit = 

    module ColTypes = 
        let [<Literal>] codeProduitT = "string"
        let [<Literal>] versionVarianteT = "string"
        let [<Literal>] evolutionT = "string" 
        let [<Literal>] libelleT = "string"
        let [<Literal>] codeFamilleLogT = "int option"
        let [<Literal>] natureT = "string"
        let [<Literal>] quantiteT = "int"
        
    let [<Literal>] codeProduit = "Code produit"
    let [<Literal>] versionVariante = "Version de la variante"
    let [<Literal>] evolution = "Evolution"
    let [<Literal>] libelle = "Libellé"
    let [<Literal>] codeFamilleLog = "Code Famille Logistique"
    let [<Literal>] nature = "Nature du produit"
    let [<Literal>] quantite = "Quantité"

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
        let [<Literal>] versionComposantT = "string option"
        let [<Literal>] quantiteComposantT = "int option"
        let [<Literal>] sousEnsembleT = "string option"

    let [<Literal>] codeComposant = "Code Composant"
    let [<Literal>] versionComposant = "Version"
    let [<Literal>] quantiteComposant = "Quantité composant"
    let [<Literal>] sousEnsemble = "Sous ensemble"

    open ColTypes

    let [<Literal>] schema = 
        codeComposant       + " (" + codeComposantT     + "), "
        + versionComposant  + " (" + versionComposantT  + "), "
        + quantiteComposant + " (" + quantiteComposantT + "), "
        + sousEnsemble      + " (" + sousEnsembleT      + ")"

    let list = [
        codeComposant; versionComposant;
        quantiteComposant; sousEnsemble ]
    

type BomLine = {
    CodeProduit : string
    VersionVariante : string
    Evolution : string
    Libelle : string
    CodeFamilleLog : int option
    Nature : string
    Quantite : int
    CodeComposant: string option
    VersionComposant : string option
    QuantiteComposant: int option
    SousEnsemble : string option  
}


module CsvFile = 

    let [<Literal>] schema = 
        InfoProduit.schema + "," + InfoComposants.schema