<Query Kind="Program">
  <NuGetReference>BenchmarkDotNet</NuGetReference>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>BenchmarkDotNet.Running</Namespace>
</Query>

#LINQPad optimize+

void Main()
{
    BenchmarkRunner.Run<ConcurrentDictionaries>();
}

// You can define other methods, fields, classes and namespaces here
[MinColumn, MaxColumn]
public class ConcurrentDictionaries
{
    private readonly ConcurrentDictionary<string, bool> dictionary1 = new ConcurrentDictionary<string, bool>();
    private readonly ConcurrentDictionary<string, bool> dictionary2 = new ConcurrentDictionary<string, bool>(StringComparer.Ordinal);
    private string[] values;
    
    [Params(100, 500, 1000, 2000)]
    public int DictionarySize { get; set; } = 0;

    [GlobalSetup]
    public void Setup()
    {
        dictionary1.Clear();
        dictionary2.Clear();
        
        values = GetStrings(DictionarySize).ToArray();
        
        foreach (var str in values)
        {
            dictionary1[str] = true;
            dictionary2[str] = true;
        }
    }
    
    private IEnumerable<string> GetStrings(int count)
    {
        var random = new Random();
        var characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToArray();

        return Enumerable.Range(1, count)
            .Select(_ =>
            {
                var stringLength = random.Next(10, 20);
                var bytes = new char[stringLength];
                
                for (var i = 0; i < stringLength; i++)
                {
                    bytes[i] = characters[random.Next(characters.Length)];
                }
                
                return new String(bytes);
            });
    }
    
    [Benchmark]
    public void StandardLookup() => Lookup(dictionary1);
    [Benchmark]
    public void OrdinalLookup() => Lookup(dictionary2);

    private void Lookup(ConcurrentDictionary<string, bool> dictionary)
    {
        foreach (var str in values)
        {
            var value = dictionary[str];
        }
    }
}