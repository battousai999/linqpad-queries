<Query Kind="Program" />

void Main()
{
    var enclosure = TextEnclosure.SingleBorder(40);
    
    DisplayWithFixedFont(enclosure.Enclose("This is a test"));
    DisplayWithFixedFont(enclosure.Enclose("This is a test\nAnd this is another"));

    DisplayWithFixedFont(
        enclosure.Enclose(x =>
        {
            var text = "How about another test, eh?";
            var spacing = new string(' ', Math.Max((x.MaxTextSize ?? text.Length) - text.Length, 0) / 2);

            return $"{spacing}{text}{spacing}";
        }));
        
    DisplayWithFixedFont(TextEnclosure.DoubleBorder().Enclose("This is something else"));
    
    var enclosure2 = new TextEnclosure('═', '═', null, null, null, null, null, null, null, null, null, 0);
    
    DisplayWithFixedFont(enclosure2.Enclose("This is a Header"));
    
    DisplayWithFixedFont(TextEnclosure.Border('*').Enclose("With an asterisk"));
    
    DisplayWithFixedFont(TextEnclosure.Border('█', null, null, null, 2, 1).Enclose("With a block border"));
}

public void DisplayWithFixedFont<T>(T value)
{
    Util.WithStyle(value, "font-family:consolas").Dump();
}

// You can define other methods, fields, classes and namespaces here
public class TextEnclosure
{
    public readonly char? TopBorder;
    public readonly char? BottomBorder;
    public readonly char? LeftBorder;
    public readonly char? RightBorder;
    public readonly char? UpperLeftCornerBorder;
    public readonly char? UpperRightCornerBorder;
    public readonly char? LowerRightCornerBorder;
    public readonly char? LowerLeftCornerBorder;
    public readonly int? MinTextSize;
    public readonly int? MaxTextSize;
    public readonly string Ellipsis;
    public readonly int HorizontalPadding;
    public readonly int VerticalPadding;
    
    public TextEnclosure()
        : this('─', '─', '│', '│', '┌', '┐', '┘', '└')
    {
    }
    
    public TextEnclosure(
        char? topBorder,
        char? bottomBorder,
        char? leftBorder,
        char? rightBorder,
        char? upperLeftCornerBorder,
        char? upperRightCornerBorder,
        char? lowerRightCornerBorder,
        char? lowerLeftCornerBorder,
        int? minTextSize = null,
        int? maxTextSize = null,
        string ellipsis = "...",
        int horizontalPadding = 1,
        int verticalPadding = 0)
    {
        if (horizontalPadding < 0)
            throw new ArgumentException($"Horizontal padding ({horizontalPadding}) must be greater than or equal to 0", nameof(horizontalPadding));
            
        if (verticalPadding < 0)
            throw new ArgumentException($"Vertical padding ({verticalPadding}) must be greater than or equal to 0", nameof(verticalPadding));
        
        TopBorder = topBorder;
        BottomBorder = bottomBorder;
        LeftBorder = leftBorder;
        RightBorder = rightBorder;
        UpperLeftCornerBorder = upperLeftCornerBorder;
        UpperRightCornerBorder = upperRightCornerBorder;
        LowerRightCornerBorder = lowerRightCornerBorder;
        LowerLeftCornerBorder = lowerLeftCornerBorder;
        MinTextSize = minTextSize;
        MaxTextSize = maxTextSize;
        Ellipsis = ellipsis;
        HorizontalPadding = horizontalPadding;
        VerticalPadding = verticalPadding;
    }
    
    public static TextEnclosure SingleBorder(
        int? maxTextSize = null, 
        int? minTextSize = null, 
        string ellipsis = "...", 
        int horizontalPadding = 1, 
        int verticalPadding = 0)
    {
        return new TextEnclosure('─', '─', '│', '│', '┌', '┐', '┘', '└', minTextSize, maxTextSize, ellipsis, horizontalPadding, verticalPadding);
    }


    public static TextEnclosure DoubleBorder(
        int? maxTextSize = null, 
        int? minTextSize = null, 
        string ellipsis = "...", 
        int horizontalPadding = 1, 
        int verticalPadding = 0)
    {
        return new TextEnclosure('═', '═', '║', '║', '╔', '╗', '╝', '╚', minTextSize, maxTextSize, ellipsis, horizontalPadding, verticalPadding);
    }
    
    public static TextEnclosure Border(
        char borderChar, 
        int? maxTextSize = null, 
        int? minTextSize = null, 
        string ellipsis = "...", 
        int horizontalPadding = 1, 
        int verticalPadding = 0)
    {
        return new TextEnclosure(
            borderChar, 
            borderChar, 
            borderChar, 
            borderChar, 
            borderChar, 
            borderChar, 
            borderChar, 
            borderChar, 
            maxTextSize, 
            minTextSize, 
            ellipsis, 
            horizontalPadding, 
            verticalPadding);
    }
    
    public string Enclose(Func<TextEnclosure, string> textFunc)
    {
        return Enclose(textFunc(this));
    }
    
    public string Enclose(string text)
    {
        return String.Join(Environment.NewLine, EncloseAsList(text));
    }
    
    public string Encluse(IEnumerable<string> lines)
    {
        return String.Join(Environment.NewLine, EncloseAsList(lines));
    }
    
    public IEnumerable<string> EncloseAsList(Func<TextEnclosure, string> textFunc)
    {
        return EncloseAsList(textFunc(this));
    }

    public IEnumerable<string> EncloseAsList(string text)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));
        
        return EncloseAsList(text.ToSingleton());
    }
    
    public IEnumerable<string> EncloseAsList(IEnumerable<string> lines)
    {
        if (lines == null)
            throw new ArgumentNullException(nameof(lines));

        // Function to give each line padding, ellipsis, etc.
        string transformLine(string line)
        {
            if (line.Length < MinTextSize)
                line = line.PadRight(MinTextSize.Value);

            if (line.Length > MaxTextSize)
                line = line.PadRightWithEllipsis(MaxTextSize.Value, Ellipsis);

            if (HorizontalPadding > 0)
            {
                var leadingSpace = line.TakeWhile(x => x == ' ').Count();
                var trailingSpace = line.Reverse().TakeWhile(x => x == ' ').Count();
                var leftPadding = new string(' ', Math.Max(HorizontalPadding - leadingSpace, 0));
                var rightPadding = new string(' ', Math.Max(HorizontalPadding - trailingSpace, 0));

                if (String.IsNullOrEmpty(line))
                    line = (leftPadding.Length > rightPadding.Length ? leftPadding : rightPadding);
                else
                    line = line.Bracket(leftPadding, rightPadding);
            }
            
            return line;
        }

        var splitLines = lines
            .SelectMany(x => x.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None))
            .Select(transformLine)
            .ToList();
            
        var largestLineLength = splitLines.Max(x => x.Length);

        var hasTopRow = UpperLeftCornerBorder != null || TopBorder != null || UpperRightCornerBorder != null;
        var hasBottomRow = LowerLeftCornerBorder != null || BottomBorder != null || LowerRightCornerBorder != null;
        var hasLeftColumn = UpperLeftCornerBorder != null || LeftBorder != null || LowerLeftCornerBorder != null;
        var hasRightColumn = UpperRightCornerBorder != null || RightBorder != null || LowerRightCornerBorder != null;
        
        // Top border row
        if (hasTopRow)
        {
            var upperLeft = UpperLeftCornerBorder?.ToString() ?? (hasLeftColumn ? " " : "");
            var upperMiddle = new string(TopBorder ?? ' ', largestLineLength);
            var upperRight = UpperRightCornerBorder?.ToString() ?? (hasRightColumn ? " " : "");

            yield return upperMiddle.Bracket(upperLeft, upperRight, true);
        }
        
        // Middle "text" rows
        var midLeft = LeftBorder?.ToString() ?? (hasLeftColumn ? " " : "");
        var midRight = RightBorder?.ToString() ?? (hasRightColumn ? " " : "");

        for (int i = 0; i < VerticalPadding; i++)
        {
            yield return String.Empty.PadRight(largestLineLength).Bracket(midLeft, midRight, true);
        }

        foreach (var line in splitLines)
        {
            yield return line.PadRight(largestLineLength).Bracket(midLeft, midRight, true);
        }

        for (int i = 0; i < VerticalPadding; i++)
        {
            yield return String.Empty.PadRight(largestLineLength).Bracket(midLeft, midRight, true);
        }

        // Bottom border row
        if (hasBottomRow)
        {
            var lowerLeft = LowerLeftCornerBorder?.ToString() ?? (hasLeftColumn ? " " : "");
            var lowerMiddle = new string(BottomBorder ?? ' ', largestLineLength);
            var lowerRight = LowerRightCornerBorder?.ToString() ?? (hasRightColumn ? " " : "");

            yield return lowerMiddle.Bracket(lowerLeft, lowerRight, true);
        }
    }
}

public static class Extensions
{
    public static string Bracket(this string text, string openBracket, string closeBracket, bool bracketIfEmpty = false)
    {
        if (!bracketIfEmpty && String.IsNullOrWhiteSpace(text))
            return text;

        return $"{openBracket}{text ?? ""}{closeBracket}";
    }

    public static string PadRightWithEllipsis(this string text, int size, string ellipsis = "...")
    {
        if (String.IsNullOrWhiteSpace(text))
            return String.Empty.PadRight(size);

        var ellipsisLength = ellipsis?.Length ?? 0;

        if (size < (ellipsisLength + 1))
            return text.Substring(0, size);
        else if (text.Length > size)
            return $"{text.Substring(0, size - ellipsisLength)}{ellipsis ?? ""}";
        else
            return text.PadRight(size);
    }

    public static IEnumerable<T> ToSingleton<T>(this T element)
    {
        yield return element;
    }
    
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        
        foreach (var item in collection)
        {
            action(item);
        }
    }
}