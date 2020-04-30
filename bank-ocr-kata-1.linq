<Query Kind="Program">
  <Reference Relative="assemblies\MoreLinq.dll">&lt;MyDocuments&gt;\LINQPad Queries\assemblies\MoreLinq.dll</Reference>
  <Reference Relative="assemblies\System.ValueTuple.dll">&lt;MyDocuments&gt;\LINQPad Queries\assemblies\System.ValueTuple.dll</Reference>
  <Namespace>MoreLinq</Namespace>
</Query>

void Main()
{
    var decoder = new ScannedFigureDecoder();
    
    Func<IEnumerable<string>, IEnumerable<DigitFigure>> decode = 
        x => decoder.Decode(x).Select(y => new DigitFigure(y.ToByte()));

	// Test case #1
    var lines = FixTestCase(@". _  _  _  _  _  _  _  _  _ 
                              .| || || || || || || || || |
                              .|_||_||_||_||_||_||_||_||_|");
        
    decode(lines).ToText().Dump();
    
    lines = FixTestCase(@".
                          .  |  |  |  |  |  |  |  |  |
                          .  |  |  |  |  |  |  |  |  |");
                          
    decode(lines).ToText().Dump();
    
    lines = FixTestCase(@". _  _  _  _  _  _  _  _  _ 
                          . _| _| _| _| _| _| _| _| _|
                          .|_ |_ |_ |_ |_ |_ |_ |_ |_ ");
                          
    decode(lines).ToText().Dump();
    
    lines = FixTestCase(@". _  _  _  _  _  _  _  _  _ 
                          . _| _| _| _| _| _| _| _| _|
                          . _| _| _| _| _| _| _| _| _|");
                          
    decode(lines).ToText().Dump();
    
    lines = FixTestCase(@".
                          .|_||_||_||_||_||_||_||_||_|
                          .  |  |  |  |  |  |  |  |  |");
                          
    decode(lines).ToText().Dump();
    
    lines = FixTestCase(@". _  _  _  _  _  _  _  _  _ 
                          .|_ |_ |_ |_ |_ |_ |_ |_ |_ 
                          . _| _| _| _| _| _| _| _| _|");
                          
    decode(lines).ToText().Dump();
}

// Define other methods and classes here
public IEnumerable<string> FixTestCase(string text, bool includeTrailingNewline = true)
{   
    return text
        .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
        .Select(x => x.TrimStart().Substring(1))
        .Concat(includeTrailingNewline ? String.Empty.ToSingleton() : Enumerable.Empty<string>());
}

public static class Utils
{
    public static IEnumerable<T> ToSingleton<T>(this T value)
    {
        yield return value;
    }
    
    public static string ToText(this IEnumerable<DigitFigure> col)
    {
        return String.Join("", col.Select(x => x.ToChar()));
    }
}

public struct DigitFigure
{
	//  0     _ 
	// 123   |_|
	// 456   |_|
	// 
	// 0 = 2^0 + 2^1 + 2^3 + 2^4 + 2^5 + 2^6		= 0b1111011
	// 1 = 2^3 + 2^6								= 0b1001000
	// 2 = 2^0 + 2^2 + 2^3 + 2^4 + 2^5				= 0b0111101
	// 3 = 2^0 + 2^2 + 2^3 + 2^5 + 2^6				= 0b1101101
	// 4 = 2^1 + 2^2 + 2^3 + 2^6					= 0b1001110
	// 5 = 2^0 + 2^1 + 2^2 + 2^5 + 2^6				= 0b1100111
	// 6 = 2^0 + 2^1 + 2^2 + 2^4 + 2^5 + 2^6		= 0b1110111
	// 7 = 2^0 + 2^3 + 2^6							= 0b1001001
	// 8 = 2^0 + 2^1 + 2^2 + 2^3 + 2^4 + 2^5 + 2^6 	= 0b1111111 
	// 9 = 2^0 + 2^1 + 2^2 + 2^3 + 2^5 + 2^6		= 0b1101111
	// 

	private static Dictionary<byte, int> binaryEncodings = new Dictionary<byte, int>
	{	
		[(byte)0b1111011] = 0,
		[(byte)0b1001000] = 1,
		[(byte)0b0111101] = 2,
		[(byte)0b1101101] = 3,
		[(byte)0b1001110] = 4,
		[(byte)0b1100111] = 5,
		[(byte)0b1110111] = 6,
		[(byte)0b1001001] = 7,
		[(byte)0b1111111] = 8,
		[(byte)0b1101111] = 9
	};

	public int BinaryValue { get; private set; }
    public int? DigitValue { get; private set; }
    
    public bool IsValidDigit => DigitValue != null;

	public DigitFigure(byte value)
	{
        this.BinaryValue = value;
        
        if (binaryEncodings.TryGetValue(value, out var digit))
            this.DigitValue = digit;
        else
            this.DigitValue = null;
	}
    
    public char ToChar()
    {
        if (DigitValue == null)
            return '?';
            
        return DigitValue.ToString().Reverse().First();
    }
}

public class ScannedFigureDecoder
{
    public class Block
    {
        private Func<string, byte> firstLineEvaluator;
        private Func<string, byte> secondLineEvaluator;
        private Func<string, byte> thirdLineEvaluator;
    
        public string FirstLine { get; private set; }
        public string SecondLine { get; private set; }
        public string ThirdLine { get; private set; }
    
        public Block(string firstLine, string secondLine, string thirdLine)
        {
            if ((firstLine?.Length ?? 0) != 3)
                throw new ArgumentException(nameof(firstLine), "Must have a length of 3");
                
            if ((secondLine?.Length ?? 0) != 3)
                throw new ArgumentException(nameof(secondLine), "Must have a length of 3");
                
            if ((thirdLine?.Length ?? 0) != 3)
                throw new ArgumentException(nameof(thirdLine), "Must have a length of 3");
                
            this.FirstLine = firstLine;
            this.SecondLine = secondLine;
            this.ThirdLine = thirdLine;
            
            Func<char, bool> isUnderscore = ch => ch == '_' || ch == ' ';
            Func<char, bool> isPipe = ch => ch == '|' || ch == ' ';
            Func<char, bool> isSpace = ch => ch == ' ';
            Func<int, Func<char, byte>> toPowerOf = power => ch => (byte)(ch != ' ' ? (1 << power) : 0);
            Func<char, byte> ignore = ch => 0;
            
            firstLineEvaluator = BuildLineEvaluator(isSpace, isUnderscore, isSpace, ignore, toPowerOf(0), ignore);
            secondLineEvaluator = BuildLineEvaluator(isPipe, isUnderscore, isPipe, toPowerOf(1), toPowerOf(2), toPowerOf(3));
            thirdLineEvaluator = BuildLineEvaluator(isPipe, isUnderscore, isPipe, toPowerOf(4), toPowerOf(5), toPowerOf(6));
        }
        
        private Func<string, byte> BuildLineEvaluator(Func<char, bool> isFirstCharValid,
            Func<char, bool> isSecondCharValid,
            Func<char, bool> isThirdCharValid,
            Func<char, byte> getFirstCharValue,
            Func<char, byte> getSecondCharValue,
            Func<char, byte> getThirdCharValue)
        {
            return line => 
            {
                var firstChar = line[0];
                var secondChar = line[1];
                var thirdChar = line[2];
            
                if (!isFirstCharValid(firstChar))
                    throw new InvalidOperationException($"Invalid character ({firstChar}) found in block.");
                    
                if (!isSecondCharValid(secondChar))
                    throw new InvalidOperationException($"Invalid character ({secondChar}) found in block.");
                    
                if (!isThirdCharValid(thirdChar))
                    throw new InvalidOperationException($"Invalid character ({thirdChar}) found in block.");
                    
                return (byte)(getFirstCharValue(firstChar) + getSecondCharValue(secondChar) + getThirdCharValue(thirdChar));
            };
        }
        
        public bool IsEmpty()
        {
            return (String.IsNullOrWhiteSpace(FirstLine) && 
                String.IsNullOrWhiteSpace(SecondLine) &&
                String.IsNullOrWhiteSpace(ThirdLine));
        }
        
        public byte ToByte()
        {
            return (byte)(firstLineEvaluator(FirstLine) +
                secondLineEvaluator(SecondLine) +
                thirdLineEvaluator(ThirdLine));
        }
    }
    
    public IEnumerable<Block> Decode(IEnumerable<string> lines)
    {
        // Try to take 5 lines (to see if there are too many)
        var blockLines = lines.Take(5).ToList();
        
        if (blockLines.Count < 3 || blockLines.Count > 4)
            throw new ArgumentException(nameof(lines), "There must be either 3 or 4 lines");
            
        if (blockLines.Count < 4 && !String.IsNullOrWhiteSpace(blockLines[3]))
            throw new ArgumentException(nameof(lines), "The fourth line (if included) must be empty or only contain whitespace");
            
        blockLines = blockLines.Take(3).ToList();
        
        // Helper function to convert IEnumerable<char> to string
        Func<IEnumerable<char>, string> concat = col => 
            col.Aggregate(new StringBuilder(), (acc, x) => acc.Append(x), acc => acc.ToString());
        
        // Convert List<string> into List<List<"3 character string">>
        var blockRows = blockLines.Select(x => x.Batch(3).Select(y => concat(y).PadRight(3, ' '))).ToList();
        
        // Convert previous list (List<List<string>>) into List<Block>
        return blockRows[0]
            .ZipLongest(
                blockRows[1],
                blockRows[2],
                (row1, row2, row3) => new Block(row1 ?? "   ", row2 ?? "   ", row3 ?? "   "))
            .Where(x => !x.IsEmpty())
            .ToList();            
    }
}