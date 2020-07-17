<Query Kind="Program" />

void Main()
{
    // Example #1
    var list1 = new List<TestClass>
    {
        new TestClass { Name = "Mike Lee", Id = 42 },
        new TestClass { Name = "John Spencer", Id = 33 },
        new TestClass { Name = "Jenny Lawrence", Id = 2 }
    };
    
    var list2 = new List<TestClass>
    {
        new TestClass { Name = "Michael Lee", Id = 42 },
        new TestClass { Name = "Jonathan Spencer", Id = 33 },
        new TestClass { Name = "Stacy Smith", Id = 5 }
    };
    
    // Here, using PropertyComparer is easier than creating a new class that derives
    // from IEqualityComparer<TestClass> that compares based upon the Id property.
    var results1 = list1.Except(list2, new PropertyComparer<TestClass, int>(x => x.Id));
    
    results1.Dump();

    // Example #2
    var target = new TestClass { Name = "JOHN SPENCER", Id = 33 };

    // Here, the PropertyComparer instance allows us to search the dictionary with a
    // representative (but not entirely equal) key.
    var dict1 = new Dictionary<TestClass, string>(new PropertyComparer<TestClass, string>(x => x.Name, StringComparer.OrdinalIgnoreCase))
    {
        { new TestClass { Name = "Mike Lee", Id = 42 }, "Mike Lee has an id of 42" },
        { new TestClass { Name = "John Spencer", Id = 33 }, "John Spencer has an id of 33" },
        { new TestClass { Name = "Jenny Lawrence", Id = 2 }, "Jenny Lawrence has an id of 2" }
    };
    
    var results2 = dict1[target];
    
    results2.Dump();
}

public class TestClass
{
    public string Name { get; set; }
    public int Id { get; set; }
}

// Define other methods and classes here

public class PropertyComparer<TBase, TProp> : IEqualityComparer<TBase> where TProp : IEquatable<TProp>
{
    private readonly Func<TBase, TProp> projection;
    private readonly IEqualityComparer<TProp> comparer;
    
    public PropertyComparer(Func<TBase, TProp> projection)
    {
        this.projection = projection;
        this.comparer = null;
    }

    public PropertyComparer(Func<TBase, TProp> projection, IEqualityComparer<TProp> comparer)
    {
        this.projection = projection;
        this.comparer = comparer;
    }

    public bool Equals(TBase x, TBase y)
    {
        if (x == null && y == null)
            return true;
            
        if (x == null || y == null)
            return false;
            
        var xValue = projection(x);
        var yValue = projection(y);
            
        if (comparer == null)
            return xValue.Equals(yValue);
        else
            return comparer.Equals(xValue, yValue);
    }

    public int GetHashCode(TBase obj)
    {
        if (obj == null)
            return 0;
            
        var value = projection(obj);
        
        if (comparer == null)
            return value.GetHashCode();
        else
            return comparer.GetHashCode(value);
    }
}