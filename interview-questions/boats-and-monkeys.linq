<Query Kind="Program" />

// ==========================================================================================================================================
// You have a pier with monkeys lined up waiting to get on boats. Each monkey weighs between 5 and 25 pounds (randomly). The boats
// can hold from 200 to 500 pounds (randomly) of monkeys.  The order of the monkeys getting on the boat is determined by their place in line.
// 
// Question: How many boats does it take to hold 100 of the these monkeys?
// If there's time: How many of the monkeys can fit in 100 boats?
// ==========================================================================================================================================

void Main()
{
    // ------------------------------------------------
    // How many boats does it take to hold 100 monkeys?
    // ------------------------------------------------
    var numMonkeys = 100;
    var monkeys = Monkey.InfiniteMonkeys().Take(numMonkeys);
    
    var boatEnumerator = Boat.InfiniteBoats().GetEnumerator();    
    var boatCounter = 1;
    int capacityAllocated = 0;
    
    Func<Boat> getNextBoat = () => { boatEnumerator.MoveNext(); return boatEnumerator.Current; };
    
    var currentBoat = getNextBoat();
    
    // Iterate through our 100 monkeys
    foreach (var monkey in monkeys)
    {
        // Would this monkey overflow the boat?
        if (capacityAllocated + monkey.Weight > currentBoat.Capacity)
        {
            capacityAllocated = 0;
            boatCounter += 1;
            
            currentBoat = getNextBoat();
        }
        
        capacityAllocated += monkey.Weight;        
    }
    
    Console.WriteLine($"{numMonkeys} monkeys requires {boatCounter} boats.");
    
    
    // --------------------------------------
    // How many monkeys can fit in 100 boats?
    // --------------------------------------
    var numBoats = 100;
    var boats = Boat.InfiniteBoats().Take(numBoats);
    
    var monkeyEnumerator = Monkey.InfiniteMonkeys().GetEnumerator();
    var monkeyCounter = 0;
    
    Func<Monkey> getNextMonkey = () => { monkeyEnumerator.MoveNext(); return monkeyEnumerator.Current; };
    
    var currentMonkey = getNextMonkey();
    
    // Iterate through our 100 boats
    foreach (var boat in boats)
    {
        capacityAllocated = 0;
    
        // Go through monkeys as long as there is spare capacity
        while (capacityAllocated + currentMonkey.Weight <= boat.Capacity)
        {
            capacityAllocated += currentMonkey.Weight;
            monkeyCounter += 1;
            
            currentMonkey = getNextMonkey();
        }
    }
    
    Console.WriteLine($"{numBoats} boats can fit {monkeyCounter} monkeys.");
}

public static int Sum(List<int> list)
{
    var total = 0;

    foreach (int item in list)
    {
        total += item;
    }
    
    return total;
}

// Define other methods and classes here

public class Monkey
{
    private static Random random = new Random();
    
    public int Weight { get; private set; }

    public Monkey(int weight)
    {
        if (weight <= 0)
            throw new ArgumentException(nameof(weight));
    
        Weight = weight;
    }
    
    public static IEnumerable<Monkey> InfiniteMonkeys(int minWeight = 5, int maxWeight = 25)
    {
        while (true)
        {
            yield return new Monkey(random.Next(minWeight, maxWeight + 1));
        }
    }
}

public class Boat
{
    private static Random random = new Random();

    public int Capacity { get; private set; }
    
    public Boat(int capacity)
    {
        if (capacity < 1)
            throw new ArgumentException(nameof(capacity));
            
        Capacity = capacity;
    }
    
    public static IEnumerable<Boat> InfiniteBoats(int minCapacity = 200, int maxCapacity = 500)
    {
        while (true)
        {
            yield return new Boat(random.Next(minCapacity, maxCapacity + 1));
        }
    }
}