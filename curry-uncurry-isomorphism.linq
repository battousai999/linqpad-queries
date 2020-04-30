<Query Kind="Program" />

void Main()
{
    var uncurried1 = AlreadyUncurriedFunction("testing", 3);
    var curried1 = AlreadyCurriedFunction("testing")(3);
    
    uncurried1.Dump();
    curried1.Dump();
    (uncurried1 == curried1).Dump();
    
    Console.WriteLine("---------------------");
    
    Func<string, Func<byte, int>> func3 = Curry<string, byte, int>(AlreadyUncurriedFunction);
    Func<string, byte, int> func4 = Uncurry<string, byte, int>(AlreadyCurriedFunction);
    
    var curried2 = func3("testing")(3);
    var uncurried2 = func4("testing", 3);
    
    curried2.Dump();
    uncurried2.Dump();
    
    Console.WriteLine("----------------------");
    
    Func<string, byte, int> func5 = Uncurry<string, byte, int>(Curry<string, byte, int>(AlreadyUncurriedFunction));
    Func<string, Func<byte, int>> func6 = Curry<string, byte, int>(Uncurry<string, byte, int>(AlreadyCurriedFunction));
    
    var uncurried3 = func5("testing", 3);
    var curried3 = func6("testing")(3);
    
    uncurried3.Dump();
    curried3.Dump();
}

// Define other methods and classes here

// (string, byte) -> int
public int AlreadyUncurriedFunction(string str, byte val)
{
    return (str.Length + val);
}

// string -> (byte -> int)  or  string -> byte -> int
public Func<byte, int> AlreadyCurriedFunction(string str)
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