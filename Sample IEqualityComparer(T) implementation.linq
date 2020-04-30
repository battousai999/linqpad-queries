<Query Kind="Program">
  <Namespace>System.Collections.Generic</Namespace>
</Query>

void Main()
{
    var list1 = new List<CallExtension>
    {
        new CallExtension { Dn = "test-dn-1", Location = "test-location-1" },
        new CallExtension { Dn = "test-dn-2", Location = "test-location-2" },
        new CallExtension { Dn = "test-dn-3", Location = "test-location-3" }
    };

    var list2 = new List<CallExtension>
    {
        new CallExtension { Dn = "test-dn-1", Location = "test-location-1" },
        new CallExtension { Dn = "test-dn-2", Location = "test-location-2" }
    };
    
    var diff = list1.Except(list2, CallExtensionComparer.Default).ToList();
    //var diff = list1.Except(list2).ToList();
    
    diff.Dump();
    
    EqualityComparer<int>.Default.Equals(1, 1).Dump();
}

// Define other methods and classes here
public class CallExtension
{
    public string Dn { get; set; }
    public string Location { get; set; }
}
        
public class CallExtensionComparer : IEqualityComparer<CallExtension>
{
    public readonly static CallExtensionComparer Default = new CallExtensionComparer();

    public bool Equals(CallExtension x, CallExtension y)
    {
        if (x == null || y == null)
            return false;
    
        return StringComparer.OrdinalIgnoreCase.Equals(x.Dn, y.Dn);
    }
    
    public int GetHashCode(CallExtension extension)
    {
        return extension.Dn.GetHashCode();
    }
}