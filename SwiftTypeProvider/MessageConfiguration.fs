namespace Swift

open System
open System.Net
open HtmlAgilityPack

/// Providing the message specification
module MessageConfiguration =
    /// Field Status: Mandatory (M) or Optional (O)
    type FieldStatus = | Mandatory | Optional

    type SpecType = (int * (string * string * string * FieldStatus)) list

    let predefinedSpecs = 
        [
            191, [1, ("20", "Transaction Reference Number", "16x", Mandatory)
                  2, ("21", "Related Reference", "16x", Mandatory)
                  3, ("32B", "", "3!a15d", Mandatory)
                  4, ("52a", "Ordering Institution", "A or D", Optional)
                  5, ("57a", "Account With Institution", "A, B or D", Optional)
                  6, ("71B", "", "6*35x", Mandatory)
                  7, ("72", "Sender to Receiver Information", "6*35x", Optional)]
            
//            202, [1, ("20", "Transaction Reference Number", "16x", Mandatory)
//                  2, ("21", "Related Reference", "16x", Mandatory)
//                  3, ("13C", "Time Indicator", "/8c/4!n1!s4!n", Optional)
//                  4, ("32A", "Value Date, Currency Code, Amount", "6!n3!a15d", Mandatory)
//                  5, ("52a", "Ordering Institution", "A or D", Optional)
//                  6, ("53a", "Sender's Correspondent", "A, B or D", Optional)
//                  7, ("54a", "Receiver's Correspondent", "A, B or D", Optional)
//                  8, ("56a", "Intermediary", "A or D", Optional)
//                  9, ("57a", "Account With Institution", "A, B or D", Optional)
//                  10,("58a", "Beneficiary Institution", "A or D", Mandatory)
//                  11,("72", "Sender to Receiver Information", "6*35x", Optional)]
        ] |> Map.ofList
    
    /// Swift Handbook url to the required message spec
    let inline createUrl messageNo = Uri ("http://www2.anasys.com/swifthandbook/fmt" + string messageNo + ".html")

    /// Asynchronously download the spec and parse the HTML
    let private downloadDocumentAsync url = async {
      try let wc = new WebClient()
          let! html = wc.AsyncDownloadString url
          let doc = new HtmlDocument()
          doc.LoadHtml html
          return Some doc
      with _ -> return None }

    /// Extract the fields from the page
    let private extractFieldsAsync (doc: HtmlDocument) = 
        seq {
            for a in doc.DocumentNode.SelectNodes("//table[@class=\"tblwithborder\"]").Descendants() do  
            if a.Name = "tr" then 
                yield async { return 
                    a.Descendants() 
                    |> Seq.filter (fun d -> d.Name = "td") 
                    |> Seq.map (fun node -> node.InnerText)
                    |> Seq.toList 
                }}
        |> Async.Parallel

    let private processData (data: string list seq) =
        data
        |> Seq.fold (fun res d ->
            match d with
            | [st; tag; name; format; no] ->
                let status = if st = "M" then FieldStatus.Mandatory else FieldStatus.Optional
                let number = try Int32.Parse no with e -> failwith ("Cannot parse a field number: " + no)
                (number, (tag, name, format, status)) :: res
            | _ -> res) []
        |> List.sortBy fst

    let private downloadSpec url = async {
        let! doc = downloadDocumentAsync url
        match doc with
        | Some spec ->
            let! fields = extractFieldsAsync spec
            return processData fields |> Seq.toList
        | _ -> return []
    }

    /// Search for the spec [predifined or SWIFT handbook]
    let searchForSpec messageNo =
        match Map.tryFind messageNo predefinedSpecs with
        | Some predifinedSpec -> predifinedSpec
        | _ -> downloadSpec (createUrl messageNo) |> Async.RunSynchronously   