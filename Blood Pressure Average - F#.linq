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
    { systolic = 144; diastolic = 81; pulse = 87 }
    { systolic = 139; diastolic = 79; pulse = 82 }
    { systolic = 136; diastolic = 79; pulse = 84 }
]

readings |> averaged |> Dump |> ignore