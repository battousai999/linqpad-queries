<Query Kind="Program" />

// Given a list of strings, return a list of list of strings composed of the original strings but in
// groupings of the strings that are rotations of each other.
//
// For instance, given the following list:
// [ "Tokyo", "London", "Rome", "Donlon", "Kyoto", "Paris" ]
// 
// Return the following list of lists:
// [
//     [ "Tokyo", "Kyoto" ],
//     [ "London", "Donlon" ],
//     [ "Rome" ],
//     [ "Paris" ]
// ]

void Main()
{
	var list = new List<string> { "Tokyo", "London", "Rome", "Donlon", "Kyoto", "Paris" };
	
	GetRotatedGroupings(list).Dump();
}

public class ProcessableItem<T>
{
	public ProcessableItem(T value)
	{
		this.Value = value;
	}

	public T Value { get; private set; }
	public bool IsProcessed { get; set; }
}

// Define other methods and classes here
public IEnumerable<IEnumerable<string>> GetRotatedGroupings(IEnumerable<string> elements)
{
	var list = elements.Select((x, i) => new ProcessableItem<string>(x)).ToList();
	
	Func<IEnumerable<string>> getPotentialMatches = () =>
	{	
		return list.Where(x => !x.IsProcessed).Select(x => x.Value);
	};
	
	Action<IEnumerable<string>> markAsProcessed = items =>
	{
		list.ForEach(x =>
		{
			if (items.Contains(x.Value, StringComparer.OrdinalIgnoreCase))
				x.IsProcessed = true;
		});
	};
	
	return list
		.Select(
			item =>
			{
				if (item.IsProcessed)
					return Enumerable.Empty<string>();
					
				item.IsProcessed = true;
				
				var rotations = GetRotations(item.Value);
				
				var matches = getPotentialMatches()
					.Where(x => rotations.Contains(x, StringComparer.OrdinalIgnoreCase))
					.ToList();
					
				markAsProcessed(matches);
				
				return item.Value.ToSingleton().Concat(matches);
			})
		.Where(x => x.Any());
}

public IEnumerable<string> GetRotations(string value)
{
	for (int i = 1; i < value.Length; i++)
	{
		yield return $"{value.Substring(i)}{value.Substring(0, i)}";
	}
}

public static class Utilities
{
	public static IEnumerable<T> ToSingleton<T>(this T value)
	{
		yield return value;
	}
}


