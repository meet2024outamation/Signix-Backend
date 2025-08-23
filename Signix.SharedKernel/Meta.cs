using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharedKernal;
public static class Meta
{
    public static DateTime Now => DateTime.Now;

    public static string MergeJson(string destination, string source)
    {
        JObject destinationObj = JObject.Parse(destination);
        destinationObj.Merge(JObject.Parse(source),
                        new JsonMergeSettings
                        {
                            MergeNullValueHandling = MergeNullValueHandling.Merge,
                            MergeArrayHandling = MergeArrayHandling.Replace,
                        });
        return JsonConvert.SerializeObject(destinationObj)!;
    }
    public static string MergeJsonNodes(string oldJson, string newJson)
    {
        var oldObj = JObject.Parse(oldJson);
        var newObj = JObject.Parse(newJson);

        foreach (var property in oldObj.Properties())
        {
            var propertyName = property.Name;
            var oldValue = property.Value;
            var newValue = newObj.GetValue(propertyName);

            if (newValue == null || newValue.Type == JTokenType.Null || string.IsNullOrEmpty(newValue.ToString()))
            {
                newObj[propertyName] = oldValue;
            }
        }
        return newObj.ToString();
    }
}