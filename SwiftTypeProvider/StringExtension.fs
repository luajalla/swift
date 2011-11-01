namespace Swift

open System
open System.Text.RegularExpressions

[<AutoOpen>]
module StringExtension =
    let inline isNullOrWsp str = System.String.IsNullOrWhiteSpace str
    let inline failForEmptyString str = if isNullOrWsp str then failwith "Non empty string is required"
    
    let private regex = Regex("[\W-[']]", RegexOptions.Compiled)

    type String with
        /// Splits the string into the n rows by max characters
        member x.SplitByRows (n, max) =
            let rec split (rest: string) (res, c) =
                if c >= n || rest.Length = 0 then res
                else
                    let row, rest' = if rest.Length > max then (rest.Substring (0, max), rest.Substring max) else rest, ""
                    let sep = if res = String.Empty then "" else Environment.NewLine
                    split rest' (res + sep + row, c + 1)
            split x (String.Empty, 0)   

        /// Removes the whitespace chars, punctuation etc and change 
        /// the first letters of the words to uppercase 
        member x.RemoveSpecialChars() =
            let res = 
                regex.Replace(x, " ").Split([|' '|], StringSplitOptions.RemoveEmptyEntries)
                |> Array.map (fun x -> string (Char.ToUpper x.[0]) + x.Substring(1))
            String.Join("", res)

        /// Last character
        member inline x.Last() = x.[x.Length - 1]

    /// Trunc or pad string to max n characters
    let truncOrPadString fixedLength n padChar str =
        failForEmptyString str
        if str.Length > n then str.Substring (0, n)
        elif fixedLength then str.PadRight (n, padChar)
        else str

    let inline truncTo n str = truncOrPadString false n ' ' str

    let inline splitByRows (str: string) count len = str.SplitByRows(count, len) 