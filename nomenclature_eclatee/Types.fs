[<AutoOpen>]
module Types
    type Code = Code of string
        with
            member x.Value = 
                let (Code c) = x
                c
    type SousEnsemble = SousEnsemble of string
        with
            member x.Value = 
                let (SousEnsemble ssE) = x
                ssE
    type Designation = Designation of string
        with
            member x.Value = 
                let (Designation d) = x
                d
    type Nature = Nature of string
        with
            member x.Value = 
                let (Nature n) = x
                n
    type Variante = Variante of string
        with
            member x.Value = 
                let (Variante v) = x
                v
    type Evolution = Evolution of string
        with
            member x.Value = 
                let (Evolution e) = x
                e
    type Level = Level of int
        with
            member x.Value = 
                let (Level l) = x
                l
            member x.add (n: int) =
                Level (x.Value + n)
            member x.substract (n: int) =
                Level (x.Value - n)            
    
    type BomId = 
        {
            CodeProduit : Code
            Nature : Nature
        }

    type Parent = 
        {
            CodeParent : Code
            SousEnsemble : SousEnsemble
        }

    type BomCompo = {
        CodeComposant : Code
        DesignationComposant : Designation
        NatureComposant : Nature option
        Quantite: float
        Parents: Parent list
        SousEnsemble: SousEnsemble
        Level : Level
        }
        with
            static member create code designation nature quantity parents sousEnsemble level = 
                { 
                CodeComposant = code; 
                DesignationComposant = designation;
                NatureComposant = nature;
                Quantite = quantity;
                Parents = parents; 
                SousEnsemble = sousEnsemble 
                Level = level
                }    
    