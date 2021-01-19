<Query Kind="Program">
  <NuGetReference>Battousai.ConsoleUtils</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>Battousai.Utils</Namespace>
</Query>

void Main()
{
    var test = new LoginModel
    {
        Server = "test-server",
        PeerServer = "test-peer-server",
        Place = "test-place",
        Devices = new List<DeviceModel>
                    {
                        new DeviceModel {DN = "test-dn-1", Switch = "test-switch-1"},
                        new DeviceModel {DN = "test-dn-2", Switch = "test-switch-2"}
                    }
    };

    string json = null;

    var duration = ConsoleUtils.MeasureDuration(() =>
    {
        json = JsonConvert.SerializeObject(test);
    });

    var deserializedTest = JsonConvert.DeserializeObject<LoginModel>(json);

    var str = JsonConvert.SerializeObject(deserializedTest, Newtonsoft.Json.Formatting.Indented);

    Log(str);
    Log();
    Log($"Serialization took {ConsoleUtils.NormalizeDuration(duration)} to complete.");
    Log();

    // Alternate construction (but only demonstrating serialization--not deserialization)

    duration = ConsoleUtils.MeasureDuration(() =>
    {
        json = JsonConvert.SerializeObject(
            new object[]
            {
                new
                {
                    name = "server",
                    value = test.Server
                },
                new
                {
                    name = "peer_server",
                    value = test.PeerServer
                },
                new
                {
                    name = "place",
                    value = test.Place
                },
                new
                {
                    name = "devices",
                    value = test.Devices
                        .Select(x =>
                        {
                            return new
                            {
                                dn = x.DN,
                                @switch = x.Switch
                            };
                        })
                        .ToList()
                },
            },
            Newtonsoft.Json.Formatting.Indented);
    });

    Log(json);
    Log();
    Log($"Alternate serialization took {ConsoleUtils.NormalizeDuration(duration)} to complete.");
}

// Define other methods and classes here

public void Log(string text = null)
{
    Battousai.Utils.ConsoleUtils.Log(text);
}


/*
 * Serialize a LoginModel to look like the following structure:
 * 
 *   [
 *      { 
 *          "name": "server",
 *          "value": "test-server"
 *      },
 *      { 
 *          "name": "peer_server",
 *          "value": "test-peer-server"
 *      },
 *      { 
 *          "name": "place",
 *          "value": "test-place"
 *      },
 *      { 
 *          "name": "devices",
 *          "value": [
 *              {
 *                  "dn": "test-dn-1"
 *                  "switch": "test-switch-1"
 *              },
 *              {
 *                  "dn": "test-dn-2"
 *                  "switch": "test-switch-2"
 *              }
 *          ]
 *      }
 *   ]
 */

public class LoginModelSerializer : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        /**
         * Bracket each array with WriteStartArray/WriteEndArray and each object with WriteStartObject/WriteEndObject, and 
         * use WritePropertyName and serializer.Serialize() to write out each property of the objects.
         */

        var model = value as LoginModel;

        Action<string, Func<LoginModel, object>> writeProperty = (name, projection) =>
        {
            writer.WriteStartObject();
            writer.WritePropertyName("name");
            serializer.Serialize(writer, name);
            writer.WritePropertyName("value");
            serializer.Serialize(writer, projection(model));
            writer.WriteEndObject();
        };

        // As a local function, just for fun
        object BuildDevice(DeviceModel device)
        {
            return new
            {
                dn = device.DN,
                @switch = device.Switch
            };
        }

        writer.WriteStartArray();

        writeProperty("server", x => x.Server);
        writeProperty("peer_server", x => x.PeerServer);
        writeProperty("place", x => x.Place);
        writeProperty("devices", x => x.Devices.Select(BuildDevice));

        writer.WriteEndArray();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        /**
         *  Initially, deserialize into a JObject/JArray, and then use the Newtonsoft API (on the JObject/JArray) to access
         *  specific data elements to manually save into the resultant deserialized object.
         */

        var elements = JArray.Load(reader);
        var results = new LoginModel();

        foreach (var element in elements)
        {
            var name = element.Value<string>("name");
            Func<string, bool> nameIs = nameValue => StringComparer.CurrentCultureIgnoreCase.Equals(name, nameValue);

            if (nameIs("devices"))
            {
                var value = element["value"]
                    .Select(x =>
                    {
                        return new DeviceModel
                        {
                            DN = x.Value<string>("dn"),
                            Switch = x.Value<string>("switch")
                        };
                    })
                    .ToList();

                results.Devices = value;
            }
            else
            {
                var value = element.Value<string>("value");

                if (nameIs("server"))
                    results.Server = value;
                else if (nameIs("peer_server"))
                    results.PeerServer = value;
                else if (nameIs("place"))
                    results.Place = value;
            }
        }

        return results;
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(LoginModel).IsAssignableFrom(objectType);
    }
}

[JsonConverter(typeof(LoginModelSerializer))]
public class LoginModel
{
    public string Server { get; set; }
    public string PeerServer { get; set; }
    public string Place { get; set; }
    public List<DeviceModel> Devices { get; set; }
}

public class DeviceModel
{
    public string DN { get; set; }
    public string Switch { get; set; }
}
