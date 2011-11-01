namespace Swift

[<AutoOpen>]
module OptionTypes =
    let inline private prependWithParty party info =
        if isNullOrWsp party then info
        else "/" + truncTo 36 party + System.Environment.NewLine + info

    [<AbstractClass>]
    [<AllowNullLiteral>]
    type OptionBase(opt: string) =
        /// Option Name
        member i.OptionName = opt

    /// Option A
    type OptionA(partyId: string, bic: string) =
        inherit OptionBase("A")
        new (bic: string) = OptionA(null, bic)

        /// Party Identifier - [/36x]
        member i.PartyIdentifier = partyId
        /// BIC - 11c
        member i.BIC = bic

        override i.ToString() = 
            if isNullOrWsp i.BIC || (i.BIC.Length <> 8 && i.BIC.Length <> 11) then failwith "Invalid BIC string"
            prependWithParty i.PartyIdentifier i.BIC

    /// Option B
    type OptionB(partyId: string, location: string) = 
        inherit OptionBase("B")
        new (location: string) = OptionB(null, location)

        /// Party Identifier - [/36x]
        member i.PartyIdentifier = partyId
        /// Location - [35x]
        member i.Location = location

        override i.ToString() = prependWithParty i.PartyIdentifier (truncTo 35 i.Location)

    /// Option D
    type OptionD(partyId: string, nameAddress: string) =
        inherit OptionBase("D")
        new (nameAddress: string) = OptionD(null, nameAddress)

        /// Party Identifier - [/36x]
        member i.PartyIdentifier = partyId
        /// Name and Address - 4*35x
        member i.NameAndAddress = nameAddress

        override i.ToString() = 
            prependWithParty i.PartyIdentifier (splitByRows i.NameAndAddress 4 35)

    /// Option A or D
    type OptionAD = 
        | A of OptionA 
        | D of OptionD
        with override x.ToString() = 
                match x with 
                | A optionA -> "A:" + optionA.ToString()
                | D optionD -> "D:" + optionD.ToString()
    
    /// Option A, B or D
    type OptionABD =
        | A of OptionA
        | B of OptionB
        | D of OptionD
        with override x.ToString() = 
                match x with 
                | A optionA -> "A:" + optionA.ToString()
                | B optionB -> "B:" + optionB.ToString()
                | D optionD -> "D:" + optionD.ToString()
