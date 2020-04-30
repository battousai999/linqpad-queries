<Query Kind="Program" />

// ===============================================================================================================================
// A toy factory randomly produces one of three toys (hence the name) at a time. As the toys come off the conveyor belt, you must 
// put them in the right container. The toys are sold as a set, one monkey, one airplane, and one soldier. As soon as you have one 
// of each toy, you must pull them from the bin and put them in a RetailBox and put them on a truck.
// 
// Question: How many toys do you have to produce before you have 100 retail boxes ready to ship?
// If there's Time: The monkeys are the most popular toy amongst the workers. Break out the counts and percentages of the types 
//                  of toys being made so that we know if we're producing "too many" monkeys.
// ===============================================================================================================================

void Main()
{
    var retailBoxesGoal = 100;

    var truck = new Truck();
    var barrel = new Barrel();
    var hanger = new Hanger();
    var barrack = new Barrack();

    int toyCounter = 0;
    int monkeyCounter = 0;
    int airplaneCounter = 0;
    int soldierCounter = 0;
    
    Action<IToy> countingObserver = toy =>
    {
        toyCounter += 1;
        
        DoToyAction(toy,
            _ => monkeyCounter += 1,
            _ => airplaneCounter += 1,
            _ => soldierCounter += 1);
    };
    
    var toyStream = GetObservableToyStream(countingObserver);
    var enumerator = toyStream.GetEnumerator();
    
    while (truck.Count <= retailBoxesGoal)
    {
        enumerator.MoveNext();
        var toy = enumerator.Current;
        
        DoToyAction(toy,
            x => barrel.Add(x),
            x => hanger.Add(x),
            x => barrack.Add(x));
            
        // Console.WriteLine($"(monkeys = {barrel.Count}, airplanes = {hanger.Count}, soldiers = {barrack.Count})");
            
        if (!barrel.IsEmpty && !hanger.IsEmpty && !barrack.IsEmpty)
        {
            truck.Add(WithRetailBox(barrel.Remove()));
            truck.Add(WithRetailBox(hanger.Remove()));
            truck.Add(WithRetailBox(barrack.Remove()));
        }
    }
    
    Console.WriteLine($"Needed to produce {toyCounter} toys in order to have at least 100 retail boxes.");
    Console.WriteLine();
    Console.WriteLine("Produced:");
    Console.WriteLine($"    {monkeyCounter} monkeys ({CalcPercent(monkeyCounter, toyCounter):0} %)");
    Console.WriteLine($"    {airplaneCounter} airplanes ({CalcPercent(airplaneCounter, toyCounter):0} %)");
    Console.WriteLine($"    {soldierCounter} soldiers ({CalcPercent(soldierCounter, toyCounter):0} %)");
}

public static double CalcPercent(int numerator, int denominator)
{
    return ((numerator * 100.0) / denominator);
}

public static void DoToyAction(IToy toy, Action<Monkey> monkeyAction, Action<Airplane> airplaneAction, Action<Soldier> soldierAction)
{
    if (toy is Monkey monkey)
        monkeyAction(monkey);
    else if (toy is Airplane airplane)
        airplaneAction(airplane);
    else if (toy is Soldier soldier)
        soldierAction(soldier);
    else
        throw new InvalidOperationException($"Unexpected toy of type '{toy.GetType().Name}'.");
}

public static RetailBox WithRetailBox(IToy toy)
{
    var box = new RetailBox();
    
    box.Add(toy);
    
    return box;
}

public static IEnumerable<IToy> GetObservableToyStream(Action<IToy> observer)
{
    while (true)
    {
        var toy = ToyFactory.GetToy();
        observer(toy);
        yield return toy;
    }
}

// Define other methods and classes here

public abstract class Container<T>
{
    private List<T> _items { get; set; } = new List<T>();
    public void Add(T newItem) { _items.Add(newItem); }
    
    public T Remove() 
    { 
        if (IsEmpty)
            throw new InvalidOperationException("Cannot remove from an empty container.");
    
        var item = _items[0];
        
        _items.RemoveAt(0);
    
        return item;
    }
    
    public int Count => _items.Count;
    public bool IsEmpty => _items.Count == 0;
}

public class Truck : Container<RetailBox> { }

public interface IToy { }
public class Monkey : IToy { }
public class Airplane : IToy { }
public class Soldier : IToy { }
        
public abstract class ToyContainer<T> : Container<T> where T : IToy { }
public class Barrel : ToyContainer<Monkey> { }
public class Hanger : ToyContainer<Airplane> { }
public class Barrack : ToyContainer<Soldier> { }
public class RetailBox : ToyContainer<IToy> { }
        
 public static class ToyFactory
{
    private static Random _random = new Random();

    public static IToy GetToy()
    {
        var toyToBuild = _random.Next(0, 4);
        switch (toyToBuild)
        {
            case 0:
                return new Soldier();
            case 1:
                return new Airplane();
            case 2:
            default:
                return new Monkey();
        }

    }
}
        