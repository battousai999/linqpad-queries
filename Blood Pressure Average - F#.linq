<Query Kind="FSharpProgram" />

type Reading = {
    systolic : int;
    diastolic : int;
    pulse : int;
}

let averaged xs =
    let mapAverage f = xs |> List.averageBy (f >> double) |> round |> int
    {
        systolic = mapAverage (fun x -> x.systolic)
        diastolic = mapAverage (fun x -> x.diastolic)
        pulse = mapAverage (fun x -> x.pulse)
    }

let readings = [
    { systolic = 139; diastolic = 77; pulse = 79 }
    { systolic = 134; diastolic = 74; pulse = 76 }
    { systolic = 127; diastolic = 76; pulse = 76 }
]

readings |> averaged |> Dump |> ignore