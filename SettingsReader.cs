using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// https://www.newtonsoft.com/json/help/html/SerializingJSON.htm
// https://stackoverflow.com/questions/44011596/c-sharp-newtonsoft-deserialize-json-array
// https://piotrgankiewicz.com/2016/06/06/storing-c-app-settings-with-json/
// https://json2csharp.com/

namespace Fanuc13319
{

    class SettingsReader
    {
        private readonly string _configurationFilePath="config.json";  // build actions content, copy if newer

        public object LoadConfig(Type type)
        {

            var jsonFile = File.ReadAllText("config.json");
            //var jsonFile = File.ReadAllText(_configurationFilePath);
            //object o = JsonConvert.DeserializeObject(jsonFile, type, JsonSerializerSettings);
            // Node[] node = JsonConvert.DeserializeObject <Node[]> (jsonFile);
            object obj = JsonConvert.DeserializeObject(jsonFile,type);
          //  var fetch = JsonConvert.DeserializeObject<Node[]>(jsonFile);
          //  var node = fetch.First(); // here we have a single FileList object
          //  return node;
            //Console.WriteLine(o.ToString());
            return obj;
            
            // Product deserializedProduct = JsonConvert.DeserializeObject<Product>(output);
        }

    }
}
