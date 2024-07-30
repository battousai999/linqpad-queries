<Query Kind="Program">
  <NuGetReference>FsCheck</NuGetReference>
  <Namespace>FsCheck</Namespace>
</Query>

void Main()
{
    Prop.ForAll((Func<List<int>, bool>)ReverseOfReverseIsEqualToOriginal).QuickCheck();
    Prop.ForAll((Func<List<int>, bool>)ReverseIsEqualToOriginal).QuickCheck();
    Prop.ForAll((Func<List<double>, bool>)ReverseOfReverseIsEqualToOriginalFloat).QuickCheck();
}

static bool ReverseOfReverseIsEqualToOriginal(List<int> xs)
{
    var ie = xs as IEnumerable<int>;
    
    return ie.Reverse().Reverse().SequenceEqual(xs);
}

static bool ReverseIsEqualToOriginal(List<int> xs)
{
    var ie = xs as IEnumerable<int>;
    
    return ie.Reverse().SequenceEqual(xs);
}

static bool ReverseOfReverseIsEqualToOriginalFloat(List<double> xs)
{
    var ie = xs as IEnumerable<double>;
    
    return ie.Reverse().Reverse().SequenceEqual(xs, DoubleComparer.Default);
}

public class DoubleComparer : IEqualityComparer<double>
{
    public readonly static DoubleComparer Default = new DoubleComparer();

    public bool Equals(double x, double y)
    {
        return x == y;
    }

    public int GetHashCode(double value)
    {
        return value.GetHashCode();
    }
}