<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>System.DirectoryServices</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
</Query>

void Main()
{
    var username = "kurtjo";
    var searcher = new DirectorySearcher();
    
    searcher.Filter = $"(&(objectCategory=User)(samAccountName={username}))";
    
    var userPath = searcher.FindOne();
    
    if (userPath == null)
    {
        Console.WriteLine($"Could not find user '{username}'");
        return;
    }
    
    var user = userPath.GetDirectoryEntry();
    
    var rootGroups = user.Properties["memberOf"].Cast<string>().ToList();
    
    //rootGroups.Dump();
    
    var totalGroups = GetGroups(searcher, rootGroups);
    
    Console.WriteLine($">>> AllGroups.Count = {Group.AllGroups.Count: #,##0}");
    
    //totalGroups.Dump();
    
    var content = JsonConvert.SerializeObject(totalGroups, Newtonsoft.Json.Formatting.Indented);

    File.WriteAllText(@$"c:\temp\groups-{DateTime.Now:yyyyMMdd_hhmmss}.json", content);
}

// You can define other methods, fields, classes and namespaces here

public class Group
{
    public static Dictionary<string, Group> AllGroups { get; } = new Dictionary<string, Group>();
    
    public static Group Create(string name, bool isAlreadyVisited, List<Group> memberOf)
    {
        var group = new Group
        {
            Name = name,
            IsAlreadyVisited = isAlreadyVisited,
            MemberOf = memberOf
        };
        
        if (!isAlreadyVisited)
            AllGroups.Add(name, group);
        
        return group;
    }
    
    public string Name { get; set; }
    public bool IsAlreadyVisited { get; set; } = false;
    public List<Group> MemberOf { get; set; }
    public int MemberOfCount => MemberOf == null ? -1 : MemberOf.Count;
    private int? _memberOfFullCount;
    public int MemberOfFullCount
    {
        get
        {   
            if (_memberOfFullCount != null)
                return _memberOfFullCount.Value;

            if (IsAlreadyVisited)
                _memberOfFullCount = 0;
            else
            {
                int sum = 0;
                
                foreach (var group in MemberOf)
                {
                    sum += group.MemberOfFullCount;
                }
                
                _memberOfFullCount = MemberOfCount + sum;
            }
                
            return _memberOfFullCount.Value;
        }
    }
}

public Regex _groupNameRegex = new Regex("(CN=)(.*?),.*");

public List<Group> GetGroups(DirectorySearcher searcher, IEnumerable<string> groupNames)
{
    var alreadyVisited = new HashSet<string>();
    int counter = 0;
    
    List<Group> GetGroupsInternal(IEnumerable<string> groupNames)
    {
        return groupNames
            .Select(groupName =>
            {
                var match = _groupNameRegex.Match(groupName);
                var friendlyGroupName = match.Success ? match.Groups[2].Value : groupName;
                var isAlreadyVisited = alreadyVisited.Contains(friendlyGroupName);

                if (isAlreadyVisited)
                    return Group.Create(friendlyGroupName, true, null);
                
                searcher.Filter = $"(&(distinguishedName={groupName}))";

                var groupPath = searcher.FindOne();

                if (groupPath == null)
                {
                    Console.WriteLine($"Could not find group '{groupName}'");
                    return null;
                }

                var group = groupPath.GetDirectoryEntry();
                var memberOf = group.Properties["memberOf"].Cast<string>().ToList();
                
                alreadyVisited.Add(friendlyGroupName);

                counter += 1;

                if (counter % 50 == 0)
                    Console.WriteLine($"{counter: #,##0} groups searched (AllGroups.Count = {Group.AllGroups.Count:#,##0})");
                
                return Group.Create(friendlyGroupName, false, GetGroupsInternal(memberOf));
            })
            .Where(x => x != null)
            .ToList();
    }
    
    return GetGroupsInternal(groupNames);
}