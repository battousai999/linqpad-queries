<Query Kind="Program">
  <Reference>E:\dev\BlueBoxMain\BlueBoxAPI\bin\Debug\BlueBoxAPI.dll</Reference>
  <Reference>E:\dev\BlueBoxMain\BlueBoxAPI\bin\Debug\Iag.Unity.DataAccess.dll</Reference>
  <Reference>E:\dev\BlueBoxMain\BlueBoxAPI\bin\Debug\Newtonsoft.Json.dll</Reference>
  <Namespace>BlueBoxAPI.BlueBox</Namespace>
</Query>

void Main()
{
	var password = Util.GetPassword("bluebox.uat.db.password");

	BlueBox.Initialize($"Connection Timeout=60;Data Source=idrive-uat.database.windows.net;Initial Catalog=BlueBoxUat;Persist Security Info=True;User ID=idriveuat;Password={password}");
	
	
}

// Define other methods and classes here



// Helper methods to allow invocation of private methods

public static T InvokePrivateMethod<T>(object instance, string methodName, params object[] parameters)
{
	return InvokePrivateMethod<T>(instance, methodName, null, parameters);
}

public static T InvokePrivateMethod<T>(object instance, string methodName, Type[] typeArguments, params object[] parameters)
{
	if (string.IsNullOrWhiteSpace(methodName))
		throw new ArgumentException("Method name cannot be null or empty.", nameof(methodName));

	Type type = instance?.GetType() ?? throw new ArgumentNullException(nameof(instance));

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
	InvokePrivateMethod<object>(instance, methodName, null, parameters);
}

public static void InvokePrivateMethod(object instance, string methodName, Type[] typeArguments, params object[] parameters)
{
	InvokePrivateMethod<object>(instance, methodName, typeArguments, parameters);
}
