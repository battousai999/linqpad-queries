<Query Kind="Program">
  <Reference>E:\dev\BlueBoxMain\BlueBoxAPI\bin\Debug\BlueBoxAPI.dll</Reference>
  <Reference>E:\dev\BlueBoxMain\BlueBoxAPI\bin\Debug\Iag.Unity.DataAccess.dll</Reference>
  <Reference>E:\dev\BlueBoxMain\BlueBoxAPI\bin\Debug\Newtonsoft.Json.dll</Reference>
  <Namespace>BlueBoxAPI.BlueBox</Namespace>
</Query>

void Main()
{
	var isProd = false;
    var password = Util.GetPassword($"bluebox.{(isProd ? "prod" : "uat")}.db.password");
    var server = Util.GetPassword($"bluebox.{(isProd ? "prod" : "uat")}.db.server");

    if (isProd)
        BlueBox.Initialize($"Database=BlueBoxProduction;Server={server};User ID=BlueBoxAdmin;Password={password}");
    else
        BlueBox.Initialize($"Connection Timeout=60;Data Source={server};Initial Catalog=BlueBoxUat;Persist Security Info=True;User ID=idriveuat;Password={password}");
	
	// Remember to update Edit -> Preferences -> Advanced -> "Do not shadow assembly reference" (under Execution) if you need line numbers in
	// your stacktraces (also, changing that setting requires an restart of Linqpad).
	
	
}

// Define other methods and classes here



// Helper methods to allow invocation of private methods

public static T InvokePrivateMethod<T>(object instance, string methodName, params object[] parameters)
{
	return InvokeSpecificPrivateMethod<T>(null, instance, methodName, null, parameters);
}

public static T InvokePrivateStaticMethod<T>(Type staticType, string methodName, params object[] parameters)
{
	return InvokeSpecificPrivateMethod<T>(staticType, null, methodName, null, parameters);
}

public static T InvokeSpecificPrivateMethod<T>(Type staticType, object instance, string methodName, Type[] typeArguments, params object[] parameters)
{
	if ((staticType == null && instance == null) || (staticType != null && instance != null))
		throw new ArgumentException("Provide either a staticType or an instance, but not both.");
	if (string.IsNullOrWhiteSpace(methodName))
		throw new ArgumentException("Method name cannot be null or empty.", nameof(methodName));

	Type type = staticType ?? instance?.GetType() ?? throw new ArgumentNullException(nameof(instance));

	BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
	MethodInfo method = null;

	if (typeArguments != null && typeArguments.Length > 0)
	{
		// For generic methods, find the method and make it generic
		method = type.GetMethod(methodName, flags);
		if (method != null && method.IsGenericMethodDefinition)
		{
			method = method.MakeGenericMethod(typeArguments);
		}
	}
	else
	{
		method = type.GetMethod(methodName, flags);
	}

	if (method == null)
		throw new InvalidOperationException($"Method '{methodName}' not found on type '{type.Name}'.");

	object result = method.Invoke(instance, parameters);
	return (T)result;
}

public static void InvokePrivateMethod(object instance, string methodName, params object[] parameters)
{
	InvokeSpecificPrivateMethod<object>(null, instance, methodName, null, parameters);
}

public static void InvokePrivateMethod(object instance, string methodName, Type[] typeArguments, params object[] parameters)
{
	InvokeSpecificPrivateMethod<object>(null, instance, methodName, typeArguments, parameters);
}

// This one supports calling private methods with out parameters.
//
// For example:
// // private bool TryCompute(int x, out int result)      { result = x * 2; return true; }
// // private bool TryCompute(string s, out string result) { result = s + s; return true; }
//
// object[] args = { 21, null };
// bool ok = InvokeSpecificPrivateMethod<bool>(null, instance, "TryCompute", null, args);
// int computed = (int)args[1];   // 42 — the int overload was resolved and its out value read back
public static T AlternateInvokeSpecificPrivateMethod<T>(
	Type staticType,
	object instance,
	string methodName,
	Type[] typeArguments,
	object[] parameters)
{
	if ((staticType == null && instance == null) || (staticType != null && instance != null))
		throw new ArgumentException("Provide either a staticType or an instance, but not both.");
	if (string.IsNullOrWhiteSpace(methodName))
		throw new ArgumentException("Method name cannot be null or empty.", nameof(methodName));

	Type type = staticType ?? instance?.GetType() ?? throw new ArgumentNullException(nameof(instance));
	BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public
					   | BindingFlags.Instance | BindingFlags.Static
					   | BindingFlags.DeclaredOnly; // DeclaredOnly: we walk the hierarchy ourselves

	parameters = parameters ?? Array.Empty<object>();
	bool wantGeneric = typeArguments != null && typeArguments.Length > 0;

	// Private methods are only returned for the exact type that declares them, so walk
	// BaseType from most-derived up. Dedupe override/shadow chains, keeping the most-derived.
	var seen = new HashSet<string>();
	var candidates = new List<MethodInfo>();
	for (Type t = type; t != null; t = t.BaseType)
	{
		foreach (MethodInfo m in t.GetMethods(flags))
		{
			if (m.Name != methodName) continue;
			if (m.GetParameters().Length != parameters.Length) continue;
			if (wantGeneric
					? !(m.IsGenericMethodDefinition && m.GetGenericArguments().Length == typeArguments.Length)
					: m.IsGenericMethodDefinition)
				continue;

			string sig = string.Join(",", m.GetParameters().Select(p => p.ParameterType.ToString()));
			if (!seen.Add(sig)) continue; // already have a more-derived method with this signature

			candidates.Add(m);
		}
	}

	if (candidates.Count == 0)
		throw new InvalidOperationException(
			$"Method '{methodName}' not found on type '{type.Name}' (or its base types) " +
			$"with {parameters.Length} parameter(s).");

	MethodInfo method = candidates.Count == 1
		? candidates[0]
		: candidates.FirstOrDefault(m => ParametersMatch(m, parameters, typeArguments))
		  ?? throw new AmbiguousMatchException(
			  $"Multiple overloads of '{methodName}' match; unable to disambiguate from the argument values.");

	if (wantGeneric)
		method = method.MakeGenericMethod(typeArguments);

	object result = method.Invoke(instance, parameters); // out/ref values written back into `parameters`

	if (method.ReturnType == typeof(void))
		return default;

	return (T)result;
}

private static bool ParametersMatch(MethodInfo candidate, object[] args, Type[] typeArguments)
{
	MethodInfo probe;
	try
	{
		probe = candidate.IsGenericMethodDefinition
			? candidate.MakeGenericMethod(typeArguments)
			: candidate;
	}
	catch (ArgumentException)
	{
		return false; // type arguments violate this candidate's constraints
	}

	ParameterInfo[] ps = probe.GetParameters();
	for (int i = 0; i < ps.Length; i++)
	{
		Type pType = ps[i].ParameterType;
		bool byRef = pType.IsByRef;
		if (byRef)
			pType = pType.GetElementType(); // unwrap Int32& into Int32

		object arg = args[i];
		if (arg == null)
		{
			if (!byRef && pType.IsValueType && Nullable.GetUnderlyingType(pType) == null)
				return false;
		}
		else if (!pType.IsInstanceOfType(arg))
		{
			return false;
		}
	}
	return true;
}
