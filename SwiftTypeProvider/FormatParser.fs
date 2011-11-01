namespace Swift

open System
open System.Text.RegularExpressions

/// Parsing the SWIFT message field formats
module FormatParser =
    /// General field format
    type FieldFormat(text: string, count: int, rowlen: int option, fullformat: string, ?positive: bool) =
        member x.Text = text
        member x.Count = count
        member x.RowLength = rowlen
        member x.Format = fullformat.Last()
        member x.Fixed = fullformat.[0] = '!'
        member x.OnlyPositive = if positive.IsSome then positive.Value else true
    
    /// Represent two kinds of formats: options and general
    type Format = 
        | Simple of FieldFormat
        | Complex of string * FieldFormat list 
        | Option of string
    
    let private createGeneralFieldFormat (gr: GroupCollection) =
        let inline getValue (ind: string) = gr.[ind].Value
        try
            let text, format, positive = getValue "text", getValue "format", String.IsNullOrEmpty (getValue "neg")
            let count = Int32.Parse (getValue "count")
            let rowlen =
                match getValue "rowlen" with
                | null | "" -> None
                | v -> Some (Int32.Parse v)
            FieldFormat(text, count, rowlen, format, positive)
        with e -> failwith ("Cannot parse format: " + e.Message)

    // Parse Options
    let private optionsRegex = Regex(@"^([A-Z],\s?)*[A-Z](\sor\s[A-Z])?\Z", RegexOptions.Compiled)
    let private letterRegex = Regex("Option[A-Z]+")
    let private possiblySupportedOptions = 
        typeof<OptionBase>.DeclaringType.GetNestedTypes()
        |> Array.filter (fun t -> letterRegex.IsMatch t.Name)
        |> Array.map (fun t -> t.Name.Replace("Option", ""))
    
    // Parse Fields
    let formatRegex = Regex("(?<neg>\[N\])?(?<text>\D+)??(?<count>[0-9]+)(\*(?<rowlen>[0-9]+))?(?<format>!?[acdnhsx0])", RegexOptions.Compiled)

    /// Check if the format string is Option (e.g. A)
    let (|OptionFormat|_|) str =
        if optionsRegex.IsMatch str then 
            // sort letters and get uppercase ones
            let name = String (str.ToCharArray() |> Array.filter Char.IsUpper |> Array.sort)
            // check for possibly supported options
            if Array.exists ((=)name) possiblySupportedOptions then Some name
            else failwith ("Option type is not supported: " + name)
        else None

    /// Check if the string is in General Format (e.g. 16x)
    let (|GeneralFieldFormat|_|) str =
        // 0 is empty field - like 15A (New Sequence)
        if str = "0" then Some [ yield FieldFormat(String.Empty, 0, None, "0") ]
        else 
            let matches = formatRegex.Matches str
            if matches.Count > 0 then Some [ for m in matches do yield createGeneralFieldFormat m.Groups ]
            else None
        
    /// Parse format string
    let parseFormat formatStr = 
        match formatStr with
        | OptionFormat name -> Format.Option name
        | GeneralFieldFormat formats -> 
            if Seq.length formats = 1 then 
                Format.Simple (Seq.head formats) 
            else Format.Complex (formatStr, formats)
        | str -> failwith ("Invalid format string: " + str)
