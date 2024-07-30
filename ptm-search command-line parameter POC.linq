<Query Kind="FSharpProgram">
  <Namespace>System.Globalization</Namespace>
</Query>

type UsernameAndDateTimeSpec = {
    UserName : string
    DateTime : DateTimeOffset
}

type SearchArgs = string

type SearchType = 
| Username of string
| DateTime of DateTimeOffset
| UsernameAndDateTime of UsernameAndDateTimeSpec

type SearchSpec = {
    Spec : SearchType
    Args : SearchArgs
}

type LookupArgs = string

type LookupSpec = {
    LogGuid : Guid
    Args : LookupArgs
}

type Command =
| Search of SearchSpec
| Lookup of LookupSpec
| Help of string

type Result<'a> =
| Valid of 'a
| Invalid of string

let (|InvariantEqual|_|) (str:string) arg = 
  if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
    then Some() else None
    
let parseSearch (args : string array) =
    let getDateParam (arg : string) =
        let (isValid, date) = DateTimeOffset.TryParse (arg, null, DateTimeStyles.AssumeLocal)
        if isValid then
            Some date
        else
            None
            
    let isOption (arg : string) = 
        ["-"; "--"; "/"] |> List.exists arg.StartsWith
            
    if Array.isEmpty args then
        Invalid "todo: search command help"
    else
        match getDateParam args.[0] with
        | Some date ->
            let remainingArgs = Array.tail args
            
            if Array.isEmpty remainingArgs then
                Valid (Search { Spec = DateTime date; Args = "" })
            else
                let nextArg = remainingArgs.[0]
            
                if isOption nextArg then
                    Valid (Search { Spec = DateTime date; Args = String.Join(", ", remainingArgs) })
                else
                    let remainingArgs = Array.tail remainingArgs
                    Valid (Search { Spec = UsernameAndDateTime { UserName = nextArg; DateTime = date }; Args = String.Join(", ", remainingArgs) })
        | None ->
            let nextArg = args.[0]
            
            if isOption nextArg then
                Invalid "todo: search command help"
            else
                let remainingArgs = Array.tail args
                Valid (Search { Spec = Username nextArg; Args = String.Join(", ", remainingArgs) })
                    
let parseLookup (args : string array) =
    let getGuidParam (arg : string) =
        let (isValid, guid) = Guid.TryParse arg
        if isValid then
            Some guid
        else
            None

    if Array.isEmpty args then
        Invalid "todo: lookup command help"
    else
        match getGuidParam args.[0] with
        | Some guid ->
            let remainingArgs = Array.tail args
            Valid (Lookup { LogGuid = guid; Args = String.Join(", ", remainingArgs) })
        | None ->
            Invalid "todo: lookup command help"

let parse (args : string array) =
    let firstArg = if Array.length args = 0 then "" else args.[0]
    match firstArg with
    | InvariantEqual "sumo"
    | InvariantEqual "sumologic" ->
        match parseSearch <| Array.tail args with
        | Valid result -> result
        | Invalid helpText -> Help helpText
    | InvariantEqual "file"
    | InvariantEqual "fileshare" ->
        match parseLookup <| Array.tail args with
        | Valid result -> result
        | Invalid helpText -> Help helpText
    | _ -> Help "todo: top-level help text here"




let a = Search { Spec = Username "kurtjo"; Args = "args" }
let b = Search { Spec = DateTime DateTimeOffset.Now; Args = "args" }
let c = Search { Spec = UsernameAndDateTime { UserName = "kurtjo"; DateTime = DateTimeOffset.Now }; Args = "args" }
let d = Lookup { LogGuid = Guid.Parse("b3126753-bfc2-4685-bc05-5f366bc290ea"); Args = "args" }

let p1 = parse [| "sumo"; "2022-04-26"; "--stuff" |]
let p2 = parse [| "sumo"; "2022-04-27"; "kurtjo" |]
let p3 = parse [| "sumologic"; "2022-04-28"; "kurtjo"; "--stuff" |]

let p4 = parse [| "file"; "b3126753-bfc2-4685-bc05-5f366bc290ea" |]
let p5 = parse [| "fileshare"; "b3126753-bfc2-4685-bc05-5f366bc290ea"; "--stuff" |]

let p6 = parse [| "something"; "else" |]
let p7 = parse [| "sumo"; "--this" |]
let p8 = parse [| "sumologic" |]
let p9 = parse [| "file"; "--doh" |]
let p10 = parse [| |]

printfn "%A" p1
printfn "%A" p2
printfn "%A" p3
printfn "%A" p4
printfn "%A" p5
printfn "%A" p6
printfn "%A" p7
printfn "%A" p8
printfn "%A" p9
printfn "%A" p10

//printfn "%A" a
//printfn "%A" b
//printfn "%A" c
//printfn "%A" d
//