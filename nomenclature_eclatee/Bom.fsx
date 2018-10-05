namespace Bom
open System

module InfoProduit = 

    module ColTypes = 
        let [<Literal>] codeProduitT = "string"
        let [<Literal>] designationT = "string"
        let [<Literal>] varianteT = "string"
        let [<Literal>] evolutionT = "string" 
        let [<Literal>] codeFamilleLogT = "string"
        let [<Literal>] natureT = "string"
        let [<Literal>] quantiteT = "float"
        
    let [<Literal>] codeProduit = "CodeProduit"
    let [<Literal>] designation = "Designation"
    let [<Literal>] variante = "Variante"
    let [<Literal>] evolution = "Evolution"
    let [<Literal>] codeFamilleLog = "CodeFamilleLog"
    let [<Literal>] nature = "Nature"
    let [<Literal>] quantite = "Quantite"

    open ColTypes

    let [<Literal>] schema = 
        codeProduit         + " (" + codeProduitT       + "), "
        + designation       + " (" + designationT           + "), "
        + variante          + " (" + varianteT   + "), "
        + evolution         + " (" + evolutionT         + "), "
        + codeFamilleLog    + " (" + codeFamilleLogT    + "), "
        + nature            + " (" + natureT            + "), "
        + quantite          + " (" + quantiteT          + ")"
    
    let list = [
        codeProduit; variante; 
        evolution; designation; 
        codeFamilleLog; 
        nature; quantite ] 
        
    
    
module InfoComposants = 
    module ColTypes = 
        let [<Literal>] codeComposantT = "string"
        let [<Literal>] varianteComposantT = "string"
        let [<Literal>] quantiteComposantT = "float"
        let [<Literal>] sousEnsembleComposantT = "string"
        let [<Literal>] natureComposantT = "string"
        let [<Literal>] designationComposantT = "string"
        

    let [<Literal>] codeComposant = "CodeComposant"
    let [<Literal>] varianteComposant = "VarianteComposant"
    let [<Literal>] quantiteComposant = "QuantiteComposant"
    let [<Literal>] sousEnsembleComposant = "SousEnsembleComposant"
    let [<Literal>] natureComposant = "NatureComposant"
    let [<Literal>] designationComposant = "DesignationComposant"

    open ColTypes

    let [<Literal>] schema = 
        codeComposant       + " (" + codeComposantT     + "), "
        + designationComposant + " (" + designationComposantT + "),"
        + varianteComposant  + " (" + varianteComposantT  + "), "
        + quantiteComposant + " (" + quantiteComposantT + "), "
        + sousEnsembleComposant + " (" + sousEnsembleComposantT      + ")"

    let list = [
        codeComposant; designationComposant; varianteComposant;
        quantiteComposant; sousEnsembleComposant; 
        natureComposant ]
    

type Observation = {
    CodeProduit : string
    Designation : string
    Variante : string
    Evolution : string
    CodeFamilleLog : string
    Nature : string
    Quantite : float
    CodeComposant: string
    DesignationComposant : string
    VarianteComposant : string
    QuantiteComposant: float
    SousEnsembleComposant : string
}


module CsvFile = 

    let [<Literal>] schema = 
        InfoProduit.schema + "," + InfoComposants.schema