<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Numerics.dll</Reference>
  <Namespace>System.Numerics</Namespace>
</Query>

// ====================================================================================================================================
// A raffle ticket number can be considered lucky if the sum of its first half of digits is equal to the sum of its second half digits.
// 
// Write a function that gets the first 10 lucky random ticket numbers.
// ====================================================================================================================================

void Main()
{
    var list = Ticket.GetRandomTickets()
        .Where(x => x.IsLucky)
        .Take(10)
        .ToList();
        
    list.ForEach(x => Console.WriteLine(x.Value));
}

// Define other methods and classes here
public class Ticket
{
    public const int DefaultTicketSize = 6;
    private static Random random = new Random();
    
    public BigInteger Value { get; private set; }

    public Ticket(BigInteger value)
    {
        if (value < 1)
            throw new InvalidOperationException("Not a valid ticket.");
            
        this.Value = value;
    }
    
    public static Ticket BuildRandomTicket(int numDigits = DefaultTicketSize)
    {
        var digitsString = Enumerable.Range(1, numDigits)
            .Select(x => random.Next(0, 10).ToString())
            .Aggregate(
                new StringBuilder(), 
                (acc, x) =>
                {
                    acc.Append(x);
                    return acc;
                },
                acc => acc.ToString());
                
        if (!BigInteger.TryParse(digitsString, out var value))
            throw new InvalidOperationException($"Encountered malformed string of digits.");
                
        return new Ticket(value);
    }
    
    public static IEnumerable<Ticket> GetRandomTickets(int numDigits = DefaultTicketSize)
    {
        while (true)
        {
            yield return BuildRandomTicket(numDigits);
        }
    }
    
    public bool IsLucky
    {
        get
        {
            var digits = Value.ToString();        
            var partitionDigits = (int)Math.Ceiling(digits.Length / 2.0);
            
            var firstHalf = digits.Take(partitionDigits);       
            var secondHalf = digits.Skip(digits.Length - partitionDigits);
            
            var firstSum = firstHalf.Select(x => Int32.Parse(x.ToString())).Sum();
            var secondSum = secondHalf.Select(x => Int32.Parse(x.ToString())).Sum();
            
            return (firstSum == secondSum);
        }
    }
}
