namespace Swift

open System
open System.Reflection
open Samples.FSharpPreviewRelease2011.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations

open FormatParser
open ComponentProviders
open MessageConfiguration

[<TypeProvider>]
type public SwiftTypeProvider(cfg: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()

    // Get the assembly and namespace used to house the provided types
    let assembly = Assembly.GetExecutingAssembly()
    let rootNamespace = "Swift"
    let staticParams = [ 
        provideStaticParameter "messageNo" typeof<int> None
        provideStaticParameter "path" typeof<string> (Some "")
    ]

    let swiftTy = provideType assembly rootNamespace "SwiftMessage"
    
    do swiftTy.DefineStaticParameters(
        parameters = staticParams, 
        instantiationFunction = (fun typeName parameterValues ->
          match parameterValues with 
          | [| :? int as messageNo; :? string as path |] ->  
            // Search for predifined ones, then online (SWIFT handbook)
            let spec = searchForSpec messageNo path
                       |> List.sortBy fst
                       |> List.map snd
            let numberOfFields = spec.Length
            if numberOfFields = 0 then failwith ("Cannot find the specification for the MT" + string messageNo)
            
            // The provided type for requested message
            let ty = provideType assembly rootNamespace typeName
            
            let toStringImpls = ResizeArray<_>()

            // Add all neccessary members
            spec |> List.iteri (fun i (tag, name, formatStr, opt) ->
                let optional = opt = FieldStatus.Optional
                let parsedFormat = parseFormat formatStr    
                let name = if isNullOrWsp name then "Field" + tag else name //For some messages name can be empty 
                   
                match parsedFormat with
                | Option pname ->
                    toStringImpls.Add (optional, (tag, true, toStringGeneralImpl i, null))
                | Simple format ->
                    toStringImpls.Add (optional, (tag, false, toStringImpl format i optional (checkIfDate name format), null)) 
                | Complex (pname, formats) -> 
                    let subtypeToStrings = ResizeArray<_>()
                    let subtype = provideSubtype name
                    
                    getSubtypeFieldsInfo name formats
                    |> Seq.iteri (fun j (format, spName) -> 
                            if format.Format <> '0' then 
                                provideProperty spName j (Format.Simple format)
                                |> addXmlDocDelayed (fun () -> sprintf "%s: %d%c" spName format.Count format.Format)
                                |> subtype.AddMember
                            subtypeToStrings.Add (toStringImpl format j optional (checkIfDate spName format)))

                    subtype.AddMember (provideConstructorForObjArray formats.Length)
                    
                    // Add subtype to the main type 
                    ty.AddMember subtype
                    // Message ToString() implementation includes complex properties
                    toStringImpls.Add (optional, (tag, false, (fun _ -> <@@ "" @@>), subtypeToStrings))

                // Create mutable property
                provideProperty name i parsedFormat 
                |> addXmlDocDelayed (fun() -> sprintf "%s - %s: %s" tag name formatStr)  
                |> addMember ty optional)
             
            // Provide ToString()
            toStringImpls
            |> getMainToStringImplementation
            |> provideToStringMethod
            |> ty.AddMember

            // Declare a constructor
            let ctor = provideConstructorForObjArray numberOfFields
            ctor.AddXmlDoc "Swift Message initializer"
            ty.AddMember ctor
            
            ty
          | _ -> failwith "Unexpected parameter values")) 

    do this.AddNamespace(rootNamespace, [swiftTy])


[<assembly:TypeProviderAssembly>]
do()
