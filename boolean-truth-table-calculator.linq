<Query Kind="Program" />

void Main()
{
    var context = new Context
    {
        Variables = [
            new VariableTerm("a", x => x.a, (state, value) => state.a = value),
            new VariableTerm("b", x => x.b, (state, value) => state.b = value),
            new VariableTerm("c", x => x.c, (state, value) => state.c = value),
            new VariableTerm("d", x => x.d, (state, value) => state.d = value),
        ],
        Results = [
            new ResultTerm("a | b", x => x.a || x.b),
            new ResultTerm("(a | b) | c", x => (x.a || x.b) || x.c),
            new ResultTerm("((a | b) | c) & d", x => ((x.a || x.b) || x.c) && x.d),
        ]
    };
    
    var table = new TruthTable(context);
    
    var results = table.Calculate();
    
    results.Dump();
}

// You can define other methods, fields, classes and namespaces here
public class State
{
    public bool a { get; set; }
    public bool b { get; set; }
    public bool c { get; set; }
    public bool d { get; set; }
    public bool e { get; set; }
    // add more as needed
    
    public List<(string term, bool value)> Results { get; set; } = [];
}

public class Context
{
    public List<VariableTerm> Variables { get; set; }
    public List<ResultTerm> Results { get; set; }
}

public class VariableTerm
{
    public string Name { get; set; }
    public Func<State, bool> Projection { get; set; }
    public Action<State, bool> Setter { get; set; }
    
    public VariableTerm() {}
    
    public VariableTerm(string name, Func<State, bool> projection, Action<State, bool> setter)
    {
        this.Name = name;
        this.Projection = projection;
        this.Setter = setter;
    }
}

public class ResultTerm
{
    public string Name { get; set; }
    public Func<State, bool> Projection { get; set; }
    
    public ResultTerm() {}
    
    public ResultTerm(string name, Func<State, bool> projection)
    {
        this.Name = name;
        this.Projection = projection;
    }
}

public class TermValue
{
    public VariableTerm variable { get; set; }
    public bool value { get; set; }
    
    public TermValue(VariableTerm variable, bool value)
    {
        this.variable = variable;
        this.value = value;
    }
}

public class TruthTable
{
    public Context Context { get; private set; }
    
    public TruthTable(Context context)
    {
        this.Context = context;
    }
    
    public string Calculate()
    {
        var results = GetPermutations(Context.Variables).ToList();
        
        results = results.Select(AddResults).ToList();
        
        //results.Select(GetOutput).Dump();
        
        var headers = Context.Variables.Select(x => x.Name).Concat(Context.Results.Select(x => x.Name));
        var headersLine = String.Join("\t", headers);
        var rows = results.Select(state =>
        {
            var items = Context.Variables.Select(x => x.Projection(state) ? "1" : "0")
                .Concat(Context.Results.Select(x => x.Projection(state) ? "1" : "0"));
                
            return String.Join("\t", items); 
        });

        return Tabulate(SingletonOf(headersLine).Concat(rows));
    }
    
    private string Tabulate(IEnumerable<string> lines)
    {
        var partitionedLines = lines.Select(x => x.Split('\t'));
        var widths = partitionedLines.Select(x => x.Select(y => y.Length).ToList());
        var columnWidths = Enumerable.Range(0, widths.First().Count()).Select(i => widths.Max(x => x[i]));

        Func<IEnumerable<string>, string> output = entries =>
        {
            return String.Join("   ", entries.Zip(columnWidths, (entry, width) => entry.PadRight(width)));
        };

        var firstRow = partitionedLines.First();
        var restOfRows = partitionedLines.Skip(1);
        var borderRow = columnWidths.Select(x => new String('=', x));

        var adjustedRows = SingletonOf(firstRow).Concat(SingletonOf(borderRow)).Concat(restOfRows);
        var outputRows = adjustedRows.Select(output).ToList();
        return String.Join(Environment.NewLine, outputRows);
    }

    private IEnumerable<T> SingletonOf<T>(T element)
    {
        yield return element;
    }

    private string GetOutput(State state)
    {
        var results = Context.Variables.Select(x => $"{x.Name}={(x.Projection(state) ? 1 : 0)}")
            .Concat(state.Results.Select(x => $"{x.term}={(x.value ? 1 : 0)}"));
            
        return String.Join(", ", results);
    }
    
    private State AddResults(State state)
    {
        foreach (var resultTerm in Context.Results)
        {
            state.Results.Add((resultTerm.Name, resultTerm.Projection(state)));
        }
        
        return state;
    }
    
    private IEnumerable<State> GetPermutations(IEnumerable<VariableTerm> variables)
    {
        List<TermValue> current = variables.Select(x => new TermValue(x, false)).ToList();
        
        State ToState(List<TermValue> stateInputs)
        {
            var state = new State();
            
            foreach (var input in stateInputs)
            {
                input.variable.Setter(state, input.value);
            }
            
            return state;
        }
        
        yield return ToState(current);
        
        while (!current.All(x => x.value))
        {
            for (int i = current.Count - 1; i >= 0; i--)
            {
                var item = current[i];
                
                if (!item.value)
                {
                    item.value = true;
                    break;
                }
                else
                    item.value = false;
            }
            
            yield return ToState(current);
        }
    }
}