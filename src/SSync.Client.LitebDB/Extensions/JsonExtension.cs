using System.Text.Json.Nodes;
using System.Text.Json;

namespace SSync.Client.LitebDB.Extensions
{
    public static class JsonExtension
    {
        public static string ConvertCamelCaseToPascalCaseJson(this string json)
        {
    
            var jsonObject = JsonNode.Parse(json) as JsonObject;

         
            if (jsonObject == null) return json;

       
            ConvertCamelCaseToPascalCase(jsonObject);

      
            return jsonObject.ToJsonString();
        }

        static void ConvertCamelCaseToPascalCase(JsonObject jsonObject)
        {
            
            var properties = jsonObject.ToList();  

            foreach (var property in properties)
            {
  
                string pascalCaseKey = ConvertCamelToPascalCase(property.Key);

              
                jsonObject.Remove(property.Key);

       
                if (property.Value is JsonObject childObject)
                {
                    ConvertCamelCaseToPascalCase(childObject);
                    jsonObject[pascalCaseKey] = childObject;
                }
                else if (property.Value is JsonArray jsonArray)
                {
                    ConvertCamelCaseToPascalCaseArray(jsonArray);
                    jsonObject[pascalCaseKey] = jsonArray;  
                }
                else
                {
                 
                    jsonObject[pascalCaseKey] = property.Value;
                }
            }
        }

        static void ConvertCamelCaseToPascalCaseArray(JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is JsonObject childObject)
                {
                    ConvertCamelCaseToPascalCase(childObject);
                }
                else if (item is JsonArray childArray)
                {
                    ConvertCamelCaseToPascalCaseArray(childArray);
                }
            }
        }

        static string ConvertCamelToPascalCase(string camelCase)
        {
         
            if (string.IsNullOrEmpty(camelCase) || char.IsUpper(camelCase[0]))
            {
                return camelCase;
            }

            return char.ToUpper(camelCase[0]) + camelCase.Substring(1);
        }
    }
}

