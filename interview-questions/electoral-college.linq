<Query Kind="Program" />

// ==================================================================================================================================================
// Electoral college votes are awarded to red or blue based on which color receives the most votes. Tally the electoral votes using this file layout.
// 
// File Layout (votes.csv):
//     [[Region, Electoral Votes, Red, Blue], ...]
// 
// • Display the winners in each region (1-5). The color which gains the most votes in a region wins the region.
// • Display the overall winner (Red or Blue). The color with the most regions wins the entire vote.
// ==================================================================================================================================================

void Main()
{
    var rawData = Vote.LoadFromCsvFile(@"c:\temp\votes.csv");
    
    // rawData.Dump();
    
    // ------------------------------
    // Display winners in each region
    // ------------------------------
    var winners = rawData
        .GroupBy(x => x.Region, (key, items) => 
        {
            var redVotes = items
                .Where(x => x.NumRedVotes > x.NumBlueVotes)
                .Sum(x => x.NumElectoralVotes);
                
            var blueVotes = items
                .Where(x => x.NumBlueVotes > x.NumRedVotes)
                .Sum(x => x.NumElectoralVotes);
            
            return new { Region = key, IsRedWin = (redVotes > blueVotes), IsBlueWin = (blueVotes > redVotes) };
        })
        .OrderBy(x => x.Region)
        .ToList();
        
    Console.WriteLine("Winners (by region)");
    
    winners.ForEach(x =>
    {
        string results;
        
        if (x.IsRedWin)
            results = "won by <Red>";
        else if (x.IsBlueWin)
            results = "won by <Blue>";
        else
            results = "a tie";
    
        Console.WriteLine($"({x.Region.ToString()}) region was {results}");
    });
        
    // ----------------------
    // Display overall winner
    // ----------------------
    var redRegionsWon = winners.Count(x => x.IsRedWin);
    var blueRegionsWon = winners.Count(x => x.IsBlueWin);
    
    string overallWinner;
    
    if (redRegionsWon > blueRegionsWon)
        overallWinner = "<Red>";
    else if (blueRegionsWon > redRegionsWon)
        overallWinner = "<Blue>";
    else 
        overallWinner = "a tie";
    
    Console.WriteLine();
    Console.WriteLine($"The overall winner was {overallWinner}.");
}

// Define other methods and classes here
public enum Region { WestCoast = 1, West = 2, MidWest = 3, East = 4, EastCoast = 5 }

public class Vote
{
    public Region Region { get; private set; }
    public int NumElectoralVotes { get; private set; }
    public int NumRedVotes { get; private set; }
    public int NumBlueVotes { get; private set; }
    
    private Vote(string csvRow)
    {
        var items = csvRow.Split(',')
            .Select(x => x.Trim())
            .Where(x => !String.IsNullOrWhiteSpace(x))
            .Select(x => Int32.Parse(x))
            .ToList();
            
        if (items.Count != 4)
            throw new InvalidOperationException($"Could not parse csv row: '{csvRow}'");
            
        Region = (Region)items[0];
        NumElectoralVotes = items[1];
        NumRedVotes = items[2];
        NumBlueVotes = items[3];
    }
    
    public static IEnumerable<Vote> LoadFromCsvFile(string csvFilename, bool containsHeader = true)
    {
        var content = File.ReadAllLines(csvFilename); 
    
        return content
            .Skip(containsHeader ? 1 : 0)
            .Select(x => new Vote(x)).ToList();
    }
}