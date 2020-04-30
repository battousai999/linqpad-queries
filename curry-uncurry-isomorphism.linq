<Query Kind="Program" />

void Main()
{
    var value1 = FirstFunction("testing", 3);
    var value2 = SecondFunction("testing")(3);
    
    value1.Dump();
    value2.Dump();
    (value1 == value2).Dump();
    
    Console.WriteLine("---------------------");
    
    Func<string, Func<byte, int>> func3 = Curry<string, byte, int>(FirstFunction);
    Func<string, byte, int> func4 = Uncurry<string, byte, int>(SecondFunction);
    
    var value3 = func3("testing")(3);
    var value4 = func4("testing", 3);
    
    value3.Dump();
    value4.Dump();
    
    Console.WriteLine("----------------------");
    
    Func<string, byte, int> func5 = Uncurry<string, byte, int>(Curry<string, byte, int>(FirstFunction));
    Func<string, Func<byte, int>> func6 = Curry<string, byte, int>(Uncurry<string, byte, int>(SecondFunction));
    
    var value5 = func5("testing", 3);
    var value6 = func6("testing")(3);
    
    value5.Dump();
    value6.Dump();
}

// Define other methods and classes here

// (string, byte) -> int
public int FirstFunction(string str, byte val)
{
    return (str.Length + val);
}

// string -> (byte -> int)  or  string -> byte -> int
public Func<byte, int> SecondFunction(string str)
{
    return (val => str.Length + val);
}

// ((A, B) -> C) -> (A -> B -> C)
public Func<A, Func<B, C>> Curry<A, B, C>(Func<A, B, C> func)
{
    return a => b => func(a, b);
}

// (A -> B -> C) -> ((A, B) -> C)
public Func<A, B, C> Uncurry<A, B, C>(Func<A, Func<B, C>> func)
{
    return (a, b) => func(a)(b);
}