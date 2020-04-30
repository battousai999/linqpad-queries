<Query Kind="Program" />

// ===========================================================================================
//
// Build a diamond with each row containing a letter from 'A' to a given uppercase letter and 
// then back to 'A'.  Assume only uppercase letters.  It should look like the following for 'E'...
//
//     A
//    B B
//   C   C
//  D     D
// E       E
//  D     D
//   C   C
//    B B
//     A
//
// ===========================================================================================

void Main()
{
    var results = CreateDiamond('E');
    
    Console.WriteLine(results);
}

// Define other methods and classes here
string CreateDiamond(char letter)
{
    if (letter < 'A' || letter > 'Z')
        throw new ArgumentException(nameof(letter), "Must be a normal, uppercase letter.");
    
    var maxIndent = Convert.ToUInt16(letter) - Convert.ToUInt16('A');
    
    Func<int, string> buildSpace = num => new String(' ', num);
    Func<int, int> calcSpace = num => Math.Max(((maxIndent - num) * 2) - 1, 0);
    
    // The top part of the diamond
    var triangle = BuildCharRange('A', letter)
        .Select((x, i) => 
            {
                var ch = x.ToString();
                var initialPadding = buildSpace(maxIndent - i);
                var innerSpace = buildSpace(calcSpace(maxIndent - i));
            
                return $"{initialPadding}{ch}{innerSpace}{(innerSpace == "" ? "" : ch)}";
            })
        .ToList();
        
    // Concatenate the top part with the bottom part (the bottom part consisting of most
    // of the top part reversed).
    return triangle
        .Concat(triangle.Take(triangle.Count - 1).Reverse())
        .ToText();
}

static IEnumerable<char> BuildCharRange(char startingChar, char endingChar)
{
    if (endingChar < startingChar)
    {
        var swap = startingChar;
        startingChar = endingChar;
        endingChar = swap;
    }
    
    for (char ch = startingChar; ch <= endingChar; ch++)
    {
        yield return ch;
    }
}

static class Utilities
{
    public static string ToText(this IEnumerable<string> collection)
    {
        return collection.Aggregate(
            new StringBuilder(),
            (acc, x) => acc.AppendLine(x),
            acc => acc.ToString());
    }
}

