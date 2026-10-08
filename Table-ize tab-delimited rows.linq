<Query Kind="Program" />

void Main()
{
    var lines = Data.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    var partitionedLines = lines.Select(x => x.Split('\t'));
    var widths = partitionedLines.Select(x => x.Select(y => y.Length).ToList());
    var columnWidths = Enumerable.Range(0, widths.First().Count()).Select(i => widths.Max(x => x[i]));

    Func<IEnumerable<string>, string> output = entries =>
    {
        return String.Join("   ", entries.Zip(columnWidths, (entry, width) => entry.PadRight(width)));
    };
    
    var firstRow = partitionedLines.First();
    var restOfRows = partitionedLines.Skip(1);
    var borderRow = columnWidths.Select(x => new String('=', x));
    
    var adjustedRows = SingletonOf(firstRow).Concat(SingletonOf(borderRow)).Concat(restOfRows);
    var outputRows = adjustedRows.Select(output).ToList();
    var results = String.Join(Environment.NewLine, outputRows);
    
    results.Dump();
}

// You can define other methods, fields, classes and namespaces here

public IEnumerable<T> SingletonOf<T>(T element)
{
    yield return element;
}


const string Data = @"BBOrderID	ShipToAddress1	ShipToAddress2	ShipToCity	ShipToState	ShipToZip
22516105	post office box 1234	NULL	winter haven	FL	33882
22447832	W North Foothills Dr	Unit 1234	Phoenix	AZ	85085
22359074	P o Bix 1234		Carmichael	CA	95609
22176391	Blue Heron Dr, USA	NULL	Northfield	MN	12345
";