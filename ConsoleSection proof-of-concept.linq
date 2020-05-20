<Query Kind="Program" />

// Note that because this program uses methods such as Console.SetCursorPosition(), it cannot be
// run from within LinqPad.  Instead open a command prompt, navigate to the folder containing the
// script, and run the script using 'lprun'.
//
// At some point, consider adding this to https://github.com/battousai999/ConsoleUtils

void Main()
{
    var linePrefix1 = "Processing item #1";
    var linePrefix2 = "Processing item #2";
    var linePrefix3 = "Processing item #3";
    var linePrefix4 = "Processing item #4";
    
    // Lines with varying alignment
    var lines = new List<ConsoleLine> 
    {
        new ConsoleLine(linePrefix1 + " - Waiting"),
        new ConsoleLine(linePrefix2 + " - Waiting", ConsoleAlignment.Center),
        new ConsoleLine(linePrefix3 + " - Waiting", ConsoleAlignment.Right),
        new ConsoleLine(linePrefix4, "(waiting)")
    };

    var section = new ConsoleSection(lines);
    var duration = TimeSpan.FromSeconds(2);
    
    // It looks better if the console cursor is not constantly flying around all
    // over the place.
    section.HideCursor();
    
    // First line processing
    DoOverTimePeriod(duration, 100, iteration =>
    {
        section.UpdateLine(0, $"{linePrefix1} - {iteration} percent complete");
    });

    section.UpdateLine(0, linePrefix1 + " - Done.");

    // Second line processing
    DoOverTimePeriod(duration, 100, iteration =>
    {
        section.UpdateLineWithText(1, $"{linePrefix2} - {iteration} percent complete");
    });

    section.UpdateLineWithText(1, linePrefix2 + " - Done.");

    // Third line processing
    DoOverTimePeriod(duration, 100, iteration =>
    {
        section.UpdateLineWithText(2, $"{linePrefix3} - {iteration} percent complete");
    });

    section.UpdateLineWithText(2, linePrefix3 + " - Done.");

    // Fourth line processing
    DoOverTimePeriod(duration, 100, iteration =>
    {
        section.UpdateLine(3, new ConsoleLine(linePrefix3, $"({iteration} %)"));
    });

    section.UpdateLine(3, new ConsoleLine(linePrefix3, "(done)"));

    // Add a line when all of the process is done
    section.AddLine("All processing complete.");
    
    // Close the ConsoleSection to keep the command prompt (or subsequent Console.WriteLine's, in 
    // the case of other usages of this class) from writing over the last line.
    // 
    // Note: this also "freezes" the ConsoleSection.
    section.Close();
}

// Define other methods and classes here

public void DoOverTimePeriod(TimeSpan duration, int iterations, Action<int> action)
{
    var delta = TimeSpan.FromMilliseconds(duration.TotalMilliseconds / iterations);
    
    for (int i = 1; i <= iterations; i++)
    {
        System.Threading.Thread.Sleep(delta);
        action(i);
    }
}

////////

public class ConsoleSection
{
    private List<ConsoleLine> lines = new List<UserQuery.ConsoleLine>();
    private bool isFrozen = false;
    private bool? startingCursorVisible = null;
    
    public ConsoleSection()
    {        
    }
    
    public ConsoleSection(IEnumerable<ConsoleLine> lines)
    {
        foreach (var line in lines)
        {
            AddLine(line);
        }
    }
    
    public IEnumerable<ConsoleLine> Lines => lines.ToList();
    public int Count => lines.Count;
    
    public void HideCursor()
    {
        startingCursorVisible = Console.CursorVisible;
        Console.CursorVisible = false;
    }
    
    public void AddLine(string text)
    {
        AddLine(new ConsoleLine(text));
    }
    
    public void AddLine(ConsoleLine line)
    {
        lines.Add(line);
        Console.SetCursorPosition(0, Console.CursorTop + 1);
        RenderLine(0, CalculateLineText(line));
    }
    
    public void UpdateLine(string text)
    {
        UpdateLine(new ConsoleLine(text));
    }

    public void UpdateLineWithText(string text)
    {
        var lastLineIndex = Count - 1;
        UpdateLine(lastLineIndex, lines[lastLineIndex].WithText(text));
    }

    public void UpdateLine(ConsoleLine line)
    {
        UpdateLine(Count - 1, line);
    }

    public void UpdateLine(int index, string text)
    {
        UpdateLine(index, new ConsoleLine(text));
    }
    
    public void UpdateLineWithText(int index, string text)
    {
        if (index < 0)
            throw new ArgumentException("Index cannot be less than zero", nameof(index));

        if (index >= Count)
            throw new ArgumentException("Index cannot be greater than or equal to Count", nameof(index));

        UpdateLine(index, lines[index].WithText(text));
    }
    
    public void UpdateLine(int index, ConsoleLine line)
    {
        if (index < 0)
            throw new ArgumentException("Index cannot be less than zero", nameof(index));
            
        if (index >= Count)
            throw new ArgumentException("Index cannot be greater than or equal to Count", nameof(index));
            
        lines[index] = line;
        
        RenderLine(index - Count + 1, CalculateLineText(line));
    }
    
    private string CalculateLineText(ConsoleLine line)
    {
        if (line.Alignment == ConsoleAlignment.Left)
        {
            return line.Text.PadRight(Console.WindowWidth).Substring(0, Console.WindowWidth);
        }
        else if (line.Alignment == ConsoleAlignment.Right)
        {
            return line.Text.PadLeft(Console.WindowWidth).Substring(0, Console.WindowWidth);
        }
        else // ConsoleAlignment.Center
        {
            var leftPadWidth = ((Console.WindowWidth - line.Text.Length) / 2) + line.Text.Length;
            return line.Text.PadLeft(leftPadWidth).PadRight(Console.WindowWidth).Substring(0, Console.WindowWidth);
        }
    }
    
    private void RenderLine(int position, string text)
    {
        if (isFrozen)
            throw new InvalidOperationException("This ConsoleSection has been frozen and can no longer write to the console.");
        
        var currentPosition = Console.CursorTop;
        
        Console.SetCursorPosition(0, currentPosition + position);        
        Console.Write(text);
        Console.SetCursorPosition(0, currentPosition);
    }
    
    public void Close()
    {
        if (startingCursorVisible != null)
            Console.CursorVisible = startingCursorVisible.Value;

        // Rewrite the last line in order to ensure that the console prompt does not overwrite the last line
        var lastLine = lines.LastOrDefault();

        if (lastLine != null)
        {
            Console.Write(CalculateLineText(lastLine));
        }

        isFrozen = true;
    }
}

public class ConsoleLine
{
    public const string Ellipsis = "...";
    
    public ConsoleLine(string text, ConsoleAlignment alignment = ConsoleAlignment.Left)
    {
        this.Text = text;
        this.Alignment = alignment;
    }
    
    public ConsoleLine(string leftText, string rightText, bool allowLeftEllipsis = true)
    {
        var maxLeftWidth = Math.Max(Console.WindowWidth - rightText.Length - 1, 0);
        string displayableRightText;
        
        if (rightText.Length > Console.WindowWidth)
            displayableRightText = String.Concat(rightText.Reverse().Take(Console.WindowWidth).Reverse());
        else
            displayableRightText = rightText;
            
        string displayableLeftText;
        
        if (maxLeftWidth <= 0)
            displayableLeftText = String.Empty;
        else if (leftText.Length > maxLeftWidth)
            displayableLeftText = (leftText.Substring(0, maxLeftWidth - Ellipsis.Length) + Ellipsis).PadRight(maxLeftWidth + 1);
        else
            displayableLeftText = leftText.PadRight(maxLeftWidth + 1);
            
        this.Text = displayableLeftText + displayableRightText;
        this.Alignment = ConsoleAlignment.Left;
    }
    
    public string Text { get; private set; }
    public ConsoleAlignment Alignment { get; private set; }
    
    public ConsoleLine WithText(string text) => new ConsoleLine(text, Alignment);
    public ConsoleLine WithAlignment(ConsoleAlignment alignment) => new ConsoleLine(Text, alignment);
}

public enum ConsoleAlignment
{
    Left,
    Center,
    Right
}