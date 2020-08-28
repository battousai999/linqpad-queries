<Query Kind="Program" />

void Main()
{
    var entryHeaderRegex = new Regex(@"^\[(\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}.\d{4})\s\|\s+(\w+)\]\s([\w\.\[\]`]+)\s-\s(.*)$");
    var content = File.ReadAllLines(@"c:\users\kurtjo\downloads\interceptor.20200603.0.log");

    var entries = content.Aggregate(
        new { LineNumber = 1, Results = new List<Entry>() }, 
        (acc, x) =>
        {
            var match = entryHeaderRegex.Match(x);
            
            if (match.Success)
            {
                var logDate = DateTimeOffset.Parse(match.Groups[1].Value);
                var logLevel = match.Groups[2].Value;
                var @namespace = match.Groups[3].Value;
                var message = match.Groups[4].Value;
                
                acc.Results.Add(new Entry(acc.LineNumber, logDate, logLevel, @namespace, message));
            }
            else
            {
                var lastEntry = acc.Results.LastOrDefault();
                
                if (lastEntry == null)
                    throw new InvalidOperationException("First line must contain a entry header.");
                    
                lastEntry.ExtraLines.Add(x);
            }

            return new { LineNumber = acc.LineNumber + 1, Results = acc.Results };
        },
        acc => acc.Results);
        
    AssociatePayloads(entries);
    
    //entries.Dump();
    
    var messageFlow = entries.Where(x => x.IsMessage).ToList();
    
    //messageFlow.Dump();
    
    var genesysMessages = messageFlow.Where(x => x.IsReceivedFromGenesys || x.IsSentToGenesys).ToList();
    var fubuMessages = messageFlow.Where(x => x.IsReceivedFromFubu || x.IsSentToFubu).ToList();
    var sscMessages = messageFlow.Where(x => x.IsReceivedFromSsc || x.IsSentToSsc).ToList();
    
    var maxGenesysMessageNameLength = genesysMessages.Max(x => x.GetGenesysMessage()?.Length ?? 0);
    var maxSscMessageNameLength = sscMessages.Max(x => x.GetSscMessage()?.Length ?? 0);
    var maxFubuMessageNameLength = fubuMessages.Max(x => x.GetFubuMessage()?.Length ?? 0);

    var dateFormat = "HH:mm:ss.ffff zzz";
    var genesysPadding = Math.Max(maxGenesysMessageNameLength, 10);
    var sscPadding = Math.Max(maxSscMessageNameLength, 10);
    var lineNumberPadding = Math.Max(messageFlow.Max(x => x.LineNumber.ToString().Length), 7);
    var datePadding = DateTimeOffset.Now.ToString(dateFormat).Length;
    var fubuPadding = Math.Max(maxFubuMessageNameLength, 13);
    var interceptorPadding = 17;
    
    Console.Write("Line #".PadRight(lineNumberPadding + 1));
    Console.Write("Date".PadRight(datePadding + 3));
    Console.Write("Genesys".PadRight(genesysPadding));
    Console.Write("  (Interceptor)  ");
    Console.Write("SSC".PadRight(sscPadding));
    Console.WriteLine(" Fubu Services");
    Console.WriteLine(new String('=', lineNumberPadding + 1 + datePadding + 1 + genesysPadding + interceptorPadding + sscPadding + 1 + fubuPadding));
    
    var nonGenesysInitialSpacing = new String(' ', genesysPadding);
    var fubuAfterSpacing = new String(' ', sscPadding + 1);

    messageFlow.ForEach(message =>
    {
        Console.Write(message.LineNumber.ToString().PadRight(lineNumberPadding));
        Console.Write(' ');
        Console.Write(message.LogDate.ToString(dateFormat));
        Console.Write("   ");
        
        if (message.IsGenesysMessage)
        {
            Console.Write(message.GetGenesysMessage().PadRight(genesysPadding));
            Console.WriteLine(message.IsReceivedMessage ? "  ==>  | |       " : " <==   | |       ");
        }
        else if (message.IsSscMessage)
        {
            Console.Write(nonGenesysInitialSpacing);
            Console.Write(message.IsSentMessage ? "       | |   ==> " : "       | |  <==  ");
            Console.WriteLine(message.GetSscMessage());
        }
        else if (message.IsFubuMessage)
        {
            Console.Write(nonGenesysInitialSpacing);
            Console.Write(message.IsSentMessage ? "       | |   ==> " : "       | |  <==  ");
            Console.Write(fubuAfterSpacing);
            Console.WriteLine(message.GetFubuMessage());
        }
    });
}


public void AssociatePayloads(List<Entry> entries)
{
    var messageItems = entries.Select((x, i) => new { Index = i, Entry = x }).ToList();
    var alreadyAssociatedEntries = new HashSet<Entry>();

    foreach (var messageItem in messageItems)
    {
        IEnumerable<Entry> GetSubsequentEntries(Entry entry)
        {
            foreach (var item in entries.SkipWhile(x => x != entry).Skip(1))
            {
                if (!alreadyAssociatedEntries.Contains(item))
                    yield return item;
            }
        }
        
        var payload = GetSubsequentEntries(messageItem.Entry).FirstOrDefault(x => Entry.HasPayloadFor(x, messageItem.Entry));
        
        if (payload != null)
        {
            messageItem.Entry.PayloadEntry = payload;
            alreadyAssociatedEntries.Add(payload);
        }
    }
}

// Define other methods and classes here

public class Entry
{
    public static readonly Regex sentToSscRegex = new Regex(@"Sending\s<([\w\d.]+)>\smessage\sto\s<SSC>", RegexOptions.IgnoreCase);
    public static readonly Regex sentToFubuRegex = new Regex(@"Sending\s<([\w\d.]+)>\smessage\sto\s<CoreBus>", RegexOptions.IgnoreCase);
    public static readonly Regex sentToGenesysRegex = new Regex(@"Sending\s<([\w\d.]+)>\smessage\sto\s<TServer>", RegexOptions.IgnoreCase);
    public static readonly Regex receivedFromGenesysRegex = new Regex(@"^Received\sGenesys\smessage:\s+([^\s]+)", RegexOptions.IgnoreCase);
    public static readonly Regex receivedFromFubuRegex = new Regex(@"^Received\s<([\w\d.]+)>\smessage\sfrom\s<lq\.tcp://[^>]+>", RegexOptions.IgnoreCase);
    public static readonly Regex receivedFromSscRegex = new Regex(@"^Received\s<([\w\d.]+)>\smessage\sfrom\s<SSC>", RegexOptions.IgnoreCase);
    public static readonly Regex payloadDetailsRegex = new Regex(@"^<([\w\d.]+)>\smessage\sdetails:", RegexOptions.IgnoreCase);
    
    public int LineNumber { get; }
    public DateTimeOffset LogDate { get; }
    public string LogLevel { get; }
    public string Namespace { get; }
    public string LogMessage { get; }
    public List<string> ExtraLines { get; } = new List<string>();
    public Entry PayloadEntry { get; set; }
    
    public Entry(int lineNumber, DateTimeOffset logDate, string logLevel, string @namespace, string message)
    {
        this.LineNumber = lineNumber;
        this.LogDate = logDate;
        this.LogLevel = logLevel;
        this.Namespace = @namespace;
        this.LogMessage = message;
    }
    
    public IEnumerable<string> Content => LogMessage.ToSingleton().Concat(ExtraLines.ToList());
    
    public bool IsSentToSsc => sentToSscRegex.IsMatch(LogMessage);
    public bool IsSentToFubu => sentToFubuRegex.IsMatch(LogMessage);
    public bool IsSentToGenesys => sentToGenesysRegex.IsMatch(LogMessage);
    public bool IsReceivedFromSsc => receivedFromSscRegex.IsMatch(LogMessage);
    public bool IsReceivedFromFubu => receivedFromFubuRegex.IsMatch(LogMessage);
    public bool IsReceivedFromGenesys => receivedFromGenesysRegex.IsMatch(LogMessage);
    
    public bool IsSentMessage => IsSentToSsc || IsSentToFubu || IsSentToGenesys;
    public bool IsReceivedMessage => IsReceivedFromSsc || IsReceivedFromFubu || IsReceivedFromGenesys;
    
    public bool IsGenesysMessage => IsSentToGenesys || IsReceivedFromGenesys;
    public bool IsFubuMessage => IsSentToFubu || IsReceivedFromFubu;
    public bool IsSscMessage => IsSentToSsc || IsReceivedFromSsc;
    
    public bool IsMessage => IsSentMessage || IsReceivedMessage;
    
    public string GetGenesysMessage() 
    {
        var matchSent = sentToGenesysRegex.Match(LogMessage);
        var matchReceived = receivedFromGenesysRegex.Match(LogMessage);
        
        if (matchSent.Success)
            return matchSent.Groups[1].Value.RemoveTrailingData();
        else if (matchReceived.Success)
            return matchReceived.Groups[1].Value.RemoveTrailingData();
        else
            return null;
    }

    public string GetFubuMessage()
    {
        var matchSent = sentToFubuRegex.Match(LogMessage);
        var matchReceived = receivedFromFubuRegex.Match(LogMessage);
        
        if (matchSent.Success)
            return matchSent.Groups[1].Value;
        else if (matchReceived.Success)
            return matchReceived.Groups[1].Value;
        else
            return null;
    }
    
    public string GetSscMessage()
    {
        var matchSent = sentToSscRegex.Match(LogMessage);
        var matchReceived = receivedFromSscRegex.Match(LogMessage);
        
        if (matchSent.Success)
            return matchSent.Groups[1].Value;
        else if (matchReceived.Success)
            return matchReceived.Groups[1].Value;
        else
            return null;
    }
    
    public string GetMessageName()
    {
        if (IsGenesysMessage)
            return GetGenesysMessage();
        else if (IsFubuMessage)
            return GetFubuMessage();
        else if (IsSscMessage)
            return GetSscMessage();
        else
            return null;
    }
    
    public static bool HasPayloadFor(Entry potentialPayload, Entry messageEntry)
    {
        if (messageEntry == null || potentialPayload == null)
            return false;
            
        var match = payloadDetailsRegex.Match(potentialPayload.LogMessage);
        
        if (!match.Success)
            return false;
            
        var payloadName = match.Groups[1].Value;
        
        if (messageEntry.IsGenesysMessage)
            payloadName = payloadName.RemoveTrailingData();
            
        return StringComparer.OrdinalIgnoreCase.Equals(messageEntry.GetMessageName(), payloadName);
    }
}

public static class EnumerableExtensions
{
    public static IEnumerable<T> ToSingleton<T>(this T item)
    {
        yield return item;
    }

    public static string RemoveTrailingData(this string text)
    {
        if (text == null)
            return null;
        
        if (text.EndsWith("data", StringComparison.OrdinalIgnoreCase))
            return text.Remove(text.Length - 4);
        else
            return text;
    }
}