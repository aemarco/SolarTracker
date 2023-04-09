using Newtonsoft.Json;

namespace SolarTracker.Database;

internal static class SolarContextExtensions
{
    internal static T? GetInfo<T>(this SolarContext context, string? key = null)
    {
        key ??= typeof(T).Name;
        var entry = context.KeyValueInfos.Find(key);
        if (entry is null)
            return default;

        var value = JsonConvert.DeserializeObject<T>(entry.Value);
        return value;
    }


    internal static SolarContext SetInfo<T>(this SolarContext context, T value, string? key = null)
    {
        key ??= typeof(T).Name;
        var entry = context.KeyValueInfos.Find(key);

        if (entry is null)
        {
            entry = new KeyValueInfo
            {
                Key = key
            };
            context.KeyValueInfos.Add(entry);
        }

        entry.Value = JsonConvert.SerializeObject(value);
        context.SaveChanges();
        return context;
    }
}