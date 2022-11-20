using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;

public static class JsonExtension
{
    public static string ToJson(this object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static string ToJson(this object obj, Formatting format)
    {
        return JsonConvert.SerializeObject(obj, format);
    }

    public static string ToJson(this object obj, Formatting format, params JsonConverter[] converters)
    {
        return JsonConvert.SerializeObject(obj, format, converters);
    }

    public static string ToJsonWithTypeName(this object obj)
    {
        var settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        return JsonConvert.SerializeObject(obj, settings);
    }

    public static object FromJson(this string json)
    {
        return JsonConvert.DeserializeObject(json);
    }

    public static object FromJson(this string json, Type type)
    {
        return JsonConvert.DeserializeObject(json, type);
    }

    public static T FromJson<T>(this string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static T FromJson<T>(this string json, params JsonConverter[] converters)
    {
        return JsonConvert.DeserializeObject<T>(json, converters);
    }

    public static T FromJsonWithTypeName<T>(this string json)
    {
        var settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        return JsonConvert.DeserializeObject<T>(json, settings);
    }

    public static void PopulateFromJson(this object obj, string json)
    {
        JsonConvert.PopulateObject(json, obj);
    }

    public static byte[] ToBson(this object obj)
    {
        if (obj == null)
        {
            return null;
        }

        byte[] data = null;

        using var ms = new MemoryStream();
        using var writer = new BsonWriter(ms);
        var serializer = new JsonSerializer();
        serializer.Serialize(writer, obj);

        // get data
        data = ms.ToArray();

        return data;
    }

    public static object FromBson(this byte[] data, Type type)
    {
        if (data == null)
        {
            return null;
        }

        object obj = null;

        using var ms = new MemoryStream(data);
        using var reader = new BsonReader(ms);
        var serializer = new JsonSerializer();
        obj = serializer.Deserialize(reader, type);

        return obj;
    }

    public static T FromBson<T>(this byte[] data)
    {
        return (T)FromBson(data, typeof(T));
    }

    public static object FromBsonCollection(this byte[] data, Type type)
    {
        if (data == null)
        {
            return null;
        }

        object obj = null;

        using var ms = new MemoryStream(data);
        using var reader = new BsonReader(ms);
        reader.ReadRootValueAsArray = true;
        var serializer = new JsonSerializer();
        obj = serializer.Deserialize(reader, type);

        return obj;
    }

    public static T FromBsonCollection<T>(this byte[] data)
    {
        return (T)FromBsonCollection(data, typeof(T));
    }

    public static byte[] ToBsonDictionary<K, V>(this IDictionary<K, V> dictionary)
    {
        if (dictionary == null)
        {
            return null;
        }

        byte[] data = null;

        using var ms = new MemoryStream();
        using var writer = new BsonWriter(ms);
        var serializer = new JsonSerializer();

        // serialize dictionary as KeyValuePair list
        var list = new List<KeyValuePair<K, V>>(dictionary);
        serializer.Serialize(writer, list);

        // get data
        data = ms.ToArray();

        return data;
    }

    public static IDictionary<K, V> FromBsonDictionary<K, V>(this byte[] data)
    {
        if (data == null)
        {
            return null;
        }

        IDictionary<K, V> dictionary = new Dictionary<K, V>();

        using var ms = new MemoryStream(data);
        using var reader = new BsonReader(ms);
        reader.ReadRootValueAsArray = true;

        var serializer = new JsonSerializer();

        // deserialize dictionary as KeyValuePair list
        var list = serializer.Deserialize<List<KeyValuePair<K, V>>>(reader);

        foreach (var item in list)
        {
            dictionary.Add(item.Key, item.Value);
        }

        return dictionary;
    }

    // Deep clone
    public static T Clone<T>(this object obj)
    {
        var json = ToJson(obj);
        return FromJson<T>(json);
    }

    public static T Clone<T>(this object obj,out string json)
    {
        json = ToJson(obj);
        return FromJson<T>(json);
    }

}
