using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace JsonTesting;

public static class JsonParser
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        DateTimeZoneHandling = DateTimeZoneHandling.Local,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public static T Deserialize<T>(string jsonString) where T : DynamicObject
    {
        var jObject = JObject.Parse(jsonString);
        CheckDeserializedObject(jObject);
        var deserializedObject = JsonConvert.DeserializeObject<T>(jObject.ToString(), SerializerSettings);
        deserializedObject.ServerObject = jObject;
        return deserializedObject;
    }

    private static void CheckDeserializedObject(JObject obj)
    {
        foreach (var property in obj.Properties())
        {
            if (!obj.TryGetValue(property.Name, out var propertyValue))
                return;

            switch (property.Value.Type)
            {
                case JTokenType.Object:
                    CheckDeserializedObject((JObject)propertyValue);
                    break;
                case JTokenType.Array:
                    CheckArray((JArray)propertyValue);
                    break;
            }
        }
    }

    private static void CheckArray(JArray array)
    {
        var index = 1;

        foreach (var item in array)
        {
            if (item.Type is JTokenType.Array)
                CheckArray((JArray)item);

            if (item.Type is not JTokenType.Object)
                return;

            var itemObject = (JObject)item;
            itemObject.Add("JsonArrayIndex", index++);
            CheckDeserializedObject(itemObject);
        }
    }

    public static string Convert<T>(T content) where T : DynamicObject
    {
        var clientObject = JObject.FromObject(content);

        foreach (var property in content.ServerObject.Properties())
            CheckObjectProperty(content.ServerObject, clientObject, property);

        return content.ServerObject.ToString();
    }

    private static void CheckObjectProperty(JObject serverObject, JObject clientObject, JProperty property)
    {
        if (!serverObject.TryGetValue(property.Name, out var serverProperty) ||
            !clientObject.TryGetValue(property.Name, out var clientProperty))
            return;

        switch (property.Value.Type)
        {
            case JTokenType.Object:
                CheckInnerObjectProperty((JObject)serverProperty, (JObject)clientProperty);
                break;
            case JTokenType.Array:
                CheckArrayProperty((JArray)serverProperty, (JArray)clientProperty);
                break;
            default:
                serverProperty.Replace(clientProperty);
                break;
        }
    }

    private static void CheckInnerObjectProperty(JObject serverInnerObject, JObject clientInnerObject)
    {
        foreach (var innerProperty in serverInnerObject.Properties())
            CheckObjectProperty(serverInnerObject, clientInnerObject, innerProperty);
    }
    
    private static void CheckArrayProperty(JArray serverArray, JArray clientArray)
    {
        var listToRemove = new List<JToken>();
        var isPrimitiveArray = serverArray[0].Type is not JTokenType.Object; 

        foreach (var serverArrayItem in serverArray)
        {
            if (isPrimitiveArray)
            {
                serverArray.Replace(clientArray);
                return;
            }

            var serverObject = (JObject)serverArrayItem;
            var serverObjectIndex = serverObject.GetValue("JsonArrayIndex")?.Value<int>();

            var clientObject = clientArray.SingleOrDefault(item =>
                ((JObject)item).GetValue("JsonArrayIndex")?.Value<int>() == serverObjectIndex);

            if (clientObject is null)
                listToRemove.Add(serverArrayItem);
            else
                CheckInnerObjectProperty(serverObject, (JObject)clientObject);
        }

        listToRemove.ForEach(item => serverArray.Remove(item));

        if (isPrimitiveArray)
            return;
        
        foreach (var clientArrayItem in clientArray.Where(item =>
                     ((JObject)item).GetValue("JsonArrayIndex")?.Value<int>() == 0))
            serverArray.Add(clientArrayItem);
    }
}