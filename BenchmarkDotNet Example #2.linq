<Query Kind="Program">
  <Reference>C:\dev\extend-health\genesys-cloud-gateway\src\GenesysCloudGateway\bin\Debug\net6.0\GenesysCloudGateway.dll</Reference>
  <NuGetReference>BenchmarkDotNet</NuGetReference>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>BenchmarkDotNet.Configs</Namespace>
  <Namespace>GenesysCloudGateway.Services</Namespace>
  <Namespace>BenchmarkDotNet.Running</Namespace>
</Query>

#LINQPad optimize+

void Main()
{
    var config = ManualConfig.Create(DefaultConfig.Instance);
    
    config.WithOptions(ConfigOptions.DisableOptimizationsValidator);
    
    BenchmarkRunner.Run<EvaluationTest>(config);
}

[MinColumn, MaxColumn]
public class EvaluationTest
{
    private readonly ExpressionEvaluatorService service = new ExpressionEvaluatorService();

    private const string roslynExpression = "dnis == \"8552244477\" || dnis == \"8444029132\"";
    private const string ncalcExpression = "dnis == '8552244477' || dnis == '8444029132'";
    
    private readonly Dictionary<string, string> participantData = new Dictionary<string, string>
    {
      { "dnis", "8552244477" }  
    };
    
    [Benchmark]
    public void WithRoslyn()
    {
        service.Evaluate(roslynExpression, participantData);
    }
    
    [Benchmark]
    public void WithNcalc()
    {
        service.EvaluateNCalc(ncalcExpression, participantData.ToDictionary(x => x.Key, x => (object)x.Value));
    }
}
