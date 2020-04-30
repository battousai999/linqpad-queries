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

// Define other methods and classes here
public IEnumerable<IEnumerable<string>> GetRotatedGroupings(IEnumerable<string> elements)
{
	var list = elements.ToList();
	var processedMap = list.Select(x => false).ToList();
	var results = new List<IEnumerable<string>>();
	
	Func<IEnumerable<string>> getPotentialMatches = () =>
	{
		return list.Where((_, i) => !processedMap[i]);
	};
	
	Action<IEnumerable<string>> markAsProcessed = items =>
	{
		list.ForEach((x, index) => 
		{
			if (items.Contains(x, StringComparer.OrdinalIgnoreCase))
				processedMap[index] = true; 
		});
	};
	
	for (int i = 0; i < list.Count; i++)
	{
		if (processedMap[i])
			continue;
			
		processedMap[i] = true;
		
		var rotations = GetRotations(list[i]);
		
		var matches = getPotentialMatches()
			.Where(x => rotations.Contains(x, StringComparer.OrdinalIgnoreCase))
			.ToList();		
		
		results.Add(list[i].ToSingleton().Concat(matches));
		
		markAsProcessed(matches);
	}
	
	return results;
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
	public static void ForEach<T>(this IEnumerable<T> col, Action<T, int> action)
	{
		int index = 0;
	
		foreach (var element in col)
		{
			action(element, index);
			index += 1;
		}
	}
	
	public static IEnumerable<T> ToSingleton<T>(this T value)
	{
		yield return value;
	}
}


