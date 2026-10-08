<Query Kind="Program" />

void Main()
{
	var st = @"";	// <=== paste stack trace here
	
	RenderExceptionStackTrace(st).Dump();
}

// Define other methods and classes here
public string RenderExceptionStackTrace(string stackTrace)
{
	return Regex.Replace(stackTrace, @"\s*\bat\s+", "\n   at ");
}