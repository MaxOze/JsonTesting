using System;
using System.Collections;
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
        var deserializedObject = JsonConvert.DeserializeObject<T>(jsonString, SerializerSettings);
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
            CheckObjectProperty(typeof(T), content.ServerObject, clientObject, property);

        return content.ServerObject.ToString();
    }

    private static void CheckObjectProperty(Type objectType, JObject serverObject, JObject clientObject,
        JProperty property)
    {
        if (!serverObject.TryGetValue(property.Name, out var serverProperty) ||
            !clientObject.TryGetValue(property.Name, out var clientProperty))
            return;

        switch (property.Value.Type)
        {
            case JTokenType.Object:
                CheckInnerObjectProperty(objectType, (JObject)serverProperty, (JObject)clientProperty, property);
                break;
            case JTokenType.Array:
                CheckArrayProperty(objectType, (JArray)serverProperty, (JArray)clientProperty, property);
                break;
            default:
                serverProperty.Replace(clientProperty);
                break;
        }
    }

    private static void CheckInnerObjectProperty(Type parentType, JObject serverInnerObject, JObject clientInnerObject,
        JProperty property)
    {
        var innerObjectType = parentType.GetProperty(property.Name)?.PropertyType;

        foreach (var innerProperty in serverInnerObject.Properties())
            CheckObjectProperty(innerObjectType, serverInnerObject, clientInnerObject, innerProperty);
    }

    /*
        ЕСЛИ ЭТО ПРОСТОЙ ТИП ТО СВЕРИТЬ НА СОВПАДЕНИЯ НАВЕРНО?
        ЕСЛИ СЛОЖНЫЙ ТО УЖЕ ПРОВЕРЯТЬ ПО ПРОПЕРТИ
    */
    private static void CheckArrayProperty(Type parentType, JArray serverArray, JArray clientArray, JProperty property)
    {
        var array = parentType.GetProperty(property.Name)?.PropertyType;
        var arrayType = array!.GetElementType() ?? array.GetGenericArguments().SingleOrDefault();

        var serverDictionary = new Dictionary<object, object>();
        var clientDictionary = new Dictionary<object, object>();

        // Console.WriteLine(serverArray.);
        
        foreach (var serverArrayItem in serverArray)
        {
            // arrayType.GetProperty
        }
    }
}