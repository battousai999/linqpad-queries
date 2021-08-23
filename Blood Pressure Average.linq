<Query Kind="Program" />

void Main()
{
    var readings = new[]
    {
        new Reading(139, 87, 72),
        new Reading(134, 84, 72),
        new Reading(138, 86, 72),
    };
        
    readings.Averaged().Dump();
}

// You can define other methods, fields, classes and namespaces here
public record Reading(int systolic, int diastolic, int pulse);

public static class Extensions
{
    public static int ToRounded(this double value)
    {
        return (int)Math.Round(value);
    }

    public static Reading Averaged(this IEnumerable<Reading> readings)
    {
        return new Reading(
            readings.Average(x => x.systolic).ToRounded(),
            readings.Average(x => x.diastolic).ToRounded(),
            readings.Average(x => x.pulse).ToRounded());
    }
}
