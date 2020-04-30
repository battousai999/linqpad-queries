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
    
    Action<StringBuilder, char, int, int> addLine = (builder, ch, indent, innerSpace) =>
    {
        //Console.WriteLine($"indent={indent}, space={innerSpace}");
    
        builder.Append(' ', indent);
    
        if (ch == 'A')
            builder.Append(ch).AppendLine();
        else
        {
            builder
                .Append(ch)
                .Append(' ', innerSpace)
                .Append(ch)
                .AppendLine();
        }
    };
        
    var results = new StringBuilder();
    var currentChar = 'A';
    var maxIndent = Convert.ToUInt16(letter) - Convert.ToUInt16('A');
    var currentIndent = maxIndent;
    
    Func<int, int> calcSpace = indent => Math.Max(((maxIndent - indent) * 2) - 1, 0);
    
    // The top part of the diamond...
    while (currentChar <= letter)
    {
        addLine(results, currentChar, currentIndent, calcSpace(currentIndent));
        currentChar++;
        currentIndent--;
    }
    
    // Decrement from the character one after 'letter' to the character one before 'letter'
    currentChar--;
    currentChar--;
    // Reset indent
    currentIndent = 1;
    
    // The bottom part of the diamond...
    while (currentChar >= 'A')
    {
        addLine(results, currentChar, currentIndent, calcSpace(currentIndent));
        currentChar--;
        currentIndent++;
    }
    
    return results.ToString();
}
