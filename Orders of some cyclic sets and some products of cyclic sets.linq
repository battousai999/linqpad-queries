<Query Kind="Program" />

// This was to help answer a problem on briliant.org related to isomorphisms of cyclic sets (and their products)
// =============================================================================================================
//
// Which of the following groups are isomorphic?
// 
// I.Z_{24}
// II.Z_{ 12} \times Z_2
// III.Z_8 \times Z_3​
// IV. Z_6 \times Z_4​


void Main()
{
    Log("Z_24");
    Log("====");

    Z_24.AllElements
        .Select(x => $"{$"|{x}|".PadLeft(4, ' ')} => {x.Order}")
        .ToList()
        .ForEach(Log);

    Log("");
    Log("Z_12_x_Z_2");
    Log("====");

    Z_12_x_Z_2.AllElements
        .Select(x => $"{$"|{x}|".PadLeft(8, ' ')} => {x.Order}")
        .ToList()
        .ForEach(Log);

    Log("");
    Log("Z_8_x_Z_3");
    Log("====");

    Z_8_x_Z_3.AllElements
        .Select(x => $"{$"|{x}|".PadLeft(6, ' ')} => {x.Order}")
        .ToList()
        .ForEach(Log);

    Log("");
    Log("Z_6_x_Z_4");
    Log("====");

    Z_6_x_Z_4.AllElements
        .Select(x => $"{$"|{x}|".PadLeft(6, ' ')} => {x.Order}")
        .ToList()
        .ForEach(Log);
}

public void Log(string text)
{
    Console.WriteLine(text);
}

public struct Z_24
{
    public static readonly List<Z_24> AllElements;
    public static readonly Z_24 Identity;

    private readonly int value;

    public Z_24(int value)
    {
        this.value = Math.Abs(value % 24);
    }

    static Z_24()
    {
        AllElements = Enumerable.Range(0, 24).Select(x => new Z_24(x)).ToList();
        Identity = new Z_24(0);
    }

    public Z_24 Add(Z_24 x, Z_24 y)
    {
        return new Z_24(x.value + y.value);
    }
    
    public Z_24 Add(Z_24 value)
    {
        return Add(this, value);
    }

    public override string ToString()
    {
        return value.ToString();
    }

    public static bool operator ==(Z_24 x, Z_24 y)
    {
        return x.value == y.value;
    }
    
    public static bool operator !=(Z_24 x, Z_24 y) => !(x == y);

    public int Order
    {
        get
        {
            var list = new List<Z_24> { this };

            while (list.Last() != Identity)
            {
                list.Add(list.Last().Add(this));
            }
            
            return list.Count;
        }
    }
}


public struct Z_12_x_Z_2
{
    public static readonly List<Z_12_x_Z_2> AllElements;
    public static readonly Z_12_x_Z_2 Identity;

    private readonly (int a, int b) value;

    public Z_12_x_Z_2(int a, int b)
    {
        value = (Math.Abs(a % 12), Math.Abs(b % 2));
    }

    static Z_12_x_Z_2()
    {
        AllElements = Enumerable.Range(0, 12)
            .SelectMany(a => Enumerable.Range(0, 2).Select(b => new Z_12_x_Z_2(a, b)))
            .ToList();

        Identity = new Z_12_x_Z_2(0, 0);
    }

    public Z_12_x_Z_2 Add(Z_12_x_Z_2 x, Z_12_x_Z_2 y)
    {
        return new Z_12_x_Z_2(x.value.a + y.value.a, x.value.b + y.value.b);
    }

    public Z_12_x_Z_2 Add(Z_12_x_Z_2 value)
    {
        return Add(this, value);
    }

    public override string ToString()
    {
        return $"({value.a},{value.b})";
    }

    public static bool operator ==(Z_12_x_Z_2 x, Z_12_x_Z_2 y)
    {
        return x.value.a == y.value.a && x.value.b == y.value.b;
    }

    public static bool operator !=(Z_12_x_Z_2 x, Z_12_x_Z_2 y) => !(x == y);

    public int Order
    {
        get
        {
            var list = new List<Z_12_x_Z_2> { this };

            while (list.Last() != Identity)
            {
                list.Add(list.Last().Add(this));
            }

            return list.Count;
        }
    }
}

public struct Z_8_x_Z_3
{
    public static readonly List<Z_8_x_Z_3> AllElements;
    public static readonly Z_8_x_Z_3 Identity;
    
    private readonly (int a, int b) value;
    
    public Z_8_x_Z_3(int a, int b)
    {
        value = (Math.Abs(a % 8), Math.Abs(b % 3));
    }
    
    static Z_8_x_Z_3()
    {
        AllElements = Enumerable.Range(0, 8)
            .SelectMany(a => Enumerable.Range(0, 3).Select(b => new Z_8_x_Z_3(a, b)))
            .ToList();

        Identity = new Z_8_x_Z_3(0, 0);
    }

    public Z_8_x_Z_3 Add(Z_8_x_Z_3 x, Z_8_x_Z_3 y)
    {
        return new Z_8_x_Z_3(x.value.a + y.value.a, x.value.b + y.value.b);
    }

    public Z_8_x_Z_3 Add(Z_8_x_Z_3 value)
    {
        return Add(this, value);
    }

    public override string ToString()
    {
        return $"({value.a},{value.b})";
    }

    public static bool operator ==(Z_8_x_Z_3 x, Z_8_x_Z_3 y)
    {
        return x.value.a == y.value.a && x.value.b == y.value.b;
    }

    public static bool operator !=(Z_8_x_Z_3 x, Z_8_x_Z_3 y) => !(x == y);

    public int Order
    {
        get
        {
            var list = new List<Z_8_x_Z_3> { this };

            while (list.Last() != Identity)
            {
                list.Add(list.Last().Add(this));
            }

            return list.Count;
        }
    }
}

public struct Z_6_x_Z_4
{
    public static readonly List<Z_6_x_Z_4> AllElements;
    public static readonly Z_6_x_Z_4 Identity;

    private readonly (int a, int b) value;

    public Z_6_x_Z_4(int a, int b)
    {
        value = (Math.Abs(a % 6), Math.Abs(b % 4));
    }

    static Z_6_x_Z_4()
    {
        AllElements = Enumerable.Range(0, 6)
            .SelectMany(a => Enumerable.Range(0, 4).Select(b => new Z_6_x_Z_4(a, b)))
            .ToList();

        Identity = new Z_6_x_Z_4(0, 0);
    }

    public Z_6_x_Z_4 Add(Z_6_x_Z_4 x, Z_6_x_Z_4 y)
    {
        return new Z_6_x_Z_4(x.value.a + y.value.a, x.value.b + y.value.b);
    }

    public Z_6_x_Z_4 Add(Z_6_x_Z_4 value)
    {
        return Add(this, value);
    }

    public override string ToString()
    {
        return $"({value.a},{value.b})";
    }

    public static bool operator ==(Z_6_x_Z_4 x, Z_6_x_Z_4 y)
    {
        return x.value.a == y.value.a && x.value.b == y.value.b;
    }

    public static bool operator !=(Z_6_x_Z_4 x, Z_6_x_Z_4 y) => !(x == y);

    public int Order
    {
        get
        {
            var list = new List<Z_6_x_Z_4> { this };

            while (list.Last() != Identity)
            {
                list.Add(list.Last().Add(this));
            }

            return list.Count;
        }
    }
}