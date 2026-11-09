using System;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace OrderProcessingSystem.Storage;

public static class JsonSettings
{
    public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        //1-
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    static JsonSettings()
    {
     //  serialization convert object to json file 
     // enum storage as number started from 0 as default
     //  JsonStringEnumConvertere convert this number to its value ( ex..pending) and save the value in json file
        Options.Converters.Add(new JsonStringEnumConverter());
    }

}
