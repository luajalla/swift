namespace Swift

open System
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations
open Samples.FSharpPreviewRelease2011.ProvidedTypes

open FormatParser

/// Provided types and members implementations
module ComponentProviders =
    // If the field is empty use default values
    let getIntValueOrDefault (v: obj) = if v = null then 0 else unbox v
    let getDecimalValueOrDefault (v: obj) = if v = null then 0M else unbox v

    let inline internal getField i (args: Expr list) = <@@ ((%%(args.[0]) : obj) :?> obj[]).[i] @@>
    let inline internal setField<'a> i (args: Expr list) = <@@ ((%%(args.[0]) : obj) :?> obj[]).[i] <- (%%(args.[1]): 'a) :> obj @@>

    let inline private getIntField i (args: Expr list) = let v = getField i args in <@@ getIntValueOrDefault %%v @@>
    let inline private getDecimalField i (args: Expr list) = let v = getField i args in <@@ getDecimalValueOrDefault %%v @@>
    let inline private getInfoByType<'a> i = typeof<'a>, getField i, setField<'a> i

    let internal getTypeGetterSetter i = function
        | "d" -> typeof<decimal>, getDecimalField i, setField<decimal> i
        | "n" | "h" -> typeof<int>, getIntField i, setField<int> i
        | "a" | "c" | "y" | "x" | "z" | "s" -> getInfoByType<string> i
        | "A"   -> getInfoByType<OptionA> i
        | "B"   -> getInfoByType<OptionB> i
        | "D"   -> getInfoByType<OptionD> i
        | "AD"  -> getInfoByType<OptionAD> i
        | "ABD" -> getInfoByType<OptionABD> i
        | _ -> getInfoByType<obj> i // others
    
    let inline unboxWithSign v zero positive =
        if v = null then zero
        else let x = unbox v in if positive && x < zero then -x else x

    let printIntFormatted (v: obj) (format: string) positive = (unboxWithSign v 0 positive).ToString format
    let printDecimalFormatted (v: obj) (format: string) positive = (unboxWithSign v 0M positive).ToString format
     
    /// General ToString() implementation
    let toStringGeneralImpl i = fun (args: Expr list) -> let v = getField i args in <@@ string %%v @@>

    /// ToString() implementation for a given field format
    let toStringImpl (f: FieldFormat) i optional = 
        let text, fix, count, positive = f.Text, f.Fixed, f.Count, f.OnlyPositive
        match f.Format with
        //n = Digits
        | 'n' -> 
            let formatString = (if fix then "D" else "G") + string count
            fun (args: Expr list) -> let v = getField i args in <@@ if optional && %%v = null then "" else text + printIntFormatted %%v formatString positive @@>
        //d = Digits with decimal comma
        | 'd' -> fun (args: Expr list) -> 
                    let v = getField i args in <@@ if optional && %%v = null then "" else text + printDecimalFormatted %%v ("G" + string count) positive @@>
        //h = Uppercase hexadecimal
        | 'h' -> fun (args: Expr list) -> 
                    let v = getField i args in <@@ if optional && %%v = null then "" else text + printIntFormatted %%v ("X" + string count) positive @@>
        //a = Uppercase letters
        //c = Uppercase alphanumeric
        //x = SWIFT character set
        //y = Uppercase level A ISO 9735 characters
        //z = SWIFT extended character set 
        | 'a' | 'c' | 'y' | 'x' | 'z' | 's' -> fun (args: Expr list) -> 
            let text, v = f.Text, getField i args
            match f.RowLength with
            | Some len -> <@@ if optional && %%v = null then "" else text + splitByRows(string %%v) count len @@>
            | _ -> <@@ if optional && %%v = null then "" else text + truncOrPadString fix count 'X' (string %%v) @@>
        | '0' -> fun _ -> <@@ "" @@>
        | c -> failwith ("Unknown field format " + string c)

    /// For a complex property ToString() means concatenated values like :32A:110125USD50000
    let composeComplexProperty (args: Expr list) = 
        Seq.fold (fun res impl -> let q = impl args in <@@ (%%res: string) + (%%q: string) @@>) <@@""@@>
 
    /// Gets ToString() for a main type
    let getMainToStringImplementation (impls: ResizeArray<_>) (args: Expr list) =
        // Every field looks like :58A:BACABSNSXXX
        impls
        |> Seq.fold (fun (i, res) (optional, (tag: string, isOption, impl, subtypeimpls)) ->
            let q =
                if subtypeimpls <> null then
                    composeComplexProperty [ getField i args ] subtypeimpls
                else impl args
            let newLine = (if i = 0 then "" else Environment.NewLine) + ":"
            let fieldId = if isOption then tag.Substring(0, tag.Length-1) else tag+":"
            let state =  
                <@@ if optional && isNullOrWsp (%%q: string) then (%%res: string) // suppress empty optional fields
                    else (%%res: string) + newLine + fieldId + (%%q : string) @@>
            i + 1, state) (0, <@@ "" @@>)
        |> snd


    /// Provides Property for the given name, field number and format
    let provideProperty (name: string) i format =
        let code = match format with
                   | Format.Simple f -> string f.Format
                   | Format.Option f -> f
                   | Format.Complex (f, _) -> f
        let ty, getter, setter = getTypeGetterSetter i code
        ProvidedProperty(propertyName = name.RemoveSpecialChars(),
                         propertyType = ty,
                         GetterCode = getter,
                         SetterCode = setter)

    /// Provides Type with hidden obj methods (baseType is obj)
    let inline internal provideType asm nsp tyName =
        ProvidedTypeDefinition(assembly = asm, 
                               namespaceName = nsp,
                               typeName = tyName,
                               baseType = Some typeof<obj>,
                               HideObjectMethods = true)

    /// Provides Subtype with hidden obj methods
    let inline internal provideSubtype (tyName: string) =
        ProvidedTypeDefinition(typeName = tyName.RemoveSpecialChars(), 
                               baseType = Some typeof<obj>,
                               HideObjectMethods = true)

    /// Provides Static Parameter (mandatory)
    let inline internal provideStaticParameter name ty = ProvidedStaticParameter(name, ty)

    /// Provides ToString() for a given implementation
    let inline internal provideToStringMethod invokeCode = 
        ProvidedMethod(methodName = "ToString", 
                       parameters = [],
                       returnType = typeof<string>,
                       InvokeCode = invokeCode)

    /// Provides Constructor for a type erased to obj[]
    let inline internal provideConstructorForObjArray len = 
        ProvidedConstructor(parameters = [],
                            InvokeCode = fun _ -> <@@ Array.create len (null: obj) @@>)

    /// Adds Xml Doc delayed
    let inline internal addXmlDocDelayed f (prop: ProvidedProperty) = prop.AddXmlDocDelayed f; prop

    /// Adds member - delayed for optional properties
    let inline internal addMember (ty: ProvidedTypeDefinition) optional (info: System.Reflection.MemberInfo) =
        if optional then ty.AddMember info else ty.AddMemberDelayed (fun () -> info)

    /// Gets a combination of FieldFormat and its name
    /// if the names are unknown they are generated with indices
    let getSubtypeFieldsInfo (name: string) (formats: FieldFormat list) =
        match name.Split([|','|], StringSplitOptions.RemoveEmptyEntries) with
        | names when names.Length = formats.Length -> names
        | _ -> Array.init formats.Length (fun i -> name + string i) 
        |> Seq.zip formats
