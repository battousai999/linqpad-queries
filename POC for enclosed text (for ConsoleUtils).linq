<Query Kind="Program" />

void Main()
{
    var enclosure = TextEnclosure.SingleBorder(40);
    
    DisplayWithFixedFont(enclosure.Enclose(" This is a test "));

    DisplayWithFixedFont(
        enclosure.Enclose(x =>
        {
            var text = "How about another test, eh?";
            var spacing = new string(' ', Math.Max((x.MaxTextSize ?? text.Length) - text.Length, 0) / 2);

            return $"{spacing}{text}{spacing}";
        }));
        
    DisplayWithFixedFont(TextEnclosure.DoubleBorder().Enclose(" This is something else "));
    
    var enclosure2 = new TextEnclosure('═', '═', null, null, null, null, null, null, null, null, null);
    
    DisplayWithFixedFont(enclosure2.Enclose("This is a Header"));
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
    
    public TextEnclosure()
        : this('─', '─', '│', '│', '┌', '┐', '┘', '└', null, null, "...")
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
        int? minTextSize,
        int? maxTextSize,
        string ellipsis)
    {
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
    }
    
    public static TextEnclosure SingleBorder(int? maxTextSize = null, int? minTextSize = null, string ellipsis = "...")
    {
        return new TextEnclosure('─', '─', '│', '│', '┌', '┐', '┘', '└', minTextSize, maxTextSize, ellipsis);
    }


    public static TextEnclosure DoubleBorder(int? maxTextSize = null, int? minTextSize = null, string ellipsis = "...")
    {
        return new TextEnclosure('═', '═', '║', '║', '╔', '╗', '╝', '╚', minTextSize, maxTextSize, ellipsis);
    }
    
    public string Enclose(Func<TextEnclosure, string> textFunc)
    {
        return Enclose(textFunc(this));
    }

    public string Enclose(string text)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));
        
        if (text.Length < MinTextSize)
            text = text.PadRight(MinTextSize.Value);
            
        if (text.Length > MaxTextSize)
            text = text.PadRightWithEllipsis(MaxTextSize.Value, Ellipsis);
            
        var hasTopRow = UpperLeftCornerBorder != null || TopBorder != null || UpperRightCornerBorder != null;
        var hasBottomRow = LowerLeftCornerBorder != null || BottomBorder != null || LowerRightCornerBorder != null;
        var hasLeftColumn = UpperLeftCornerBorder != null || LeftBorder != null || LowerLeftCornerBorder != null;
        var hasRightColumn = UpperRightCornerBorder != null || RightBorder != null || LowerRightCornerBorder != null;
        
        IEnumerable<string> helper()
        {
            if (hasTopRow)
            {
                var upperLeft = UpperLeftCornerBorder?.ToString() ?? (hasLeftColumn ? " " : "");
                var upperMiddle = new string(TopBorder ?? ' ', text.Length);
                var upperRight = UpperRightCornerBorder?.ToString() ?? (hasRightColumn ? " " : "");

                yield return $"{upperLeft}{upperMiddle}{upperRight}";
            }
            
            var midLeft = LeftBorder?.ToString() ?? (hasLeftColumn ? " " : "");
            var midRight = RightBorder?.ToString() ?? (hasRightColumn ? " " : "");
            
            yield return $"{midLeft}{text}{midRight}";
            
            if (hasBottomRow)
            {
                var lowerLeft = LowerLeftCornerBorder?.ToString() ?? (hasLeftColumn ? " " : "");
                var lowerMiddle = new string(BottomBorder ?? ' ', text.Length);
                var lowerRight = LowerRightCornerBorder?.ToString() ?? (hasRightColumn ? " " : "");

                yield return $"{lowerLeft}{lowerMiddle}{lowerRight}";
            }
        }
        
        return String.Join(Environment.NewLine, helper());
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
}