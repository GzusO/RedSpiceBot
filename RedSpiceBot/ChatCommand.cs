using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedSpiceBot
{
    public static  class ChatCommand
    {
        public static Dictionary<string,string> LoadFromJson(string path)
        {     
            using (StreamReader r = new StreamReader(path))
            {
                string data = r.ReadToEnd();
                r.Close();
                Dictionary<string,string> commands = JsonConvert.DeserializeObject<Dictionary<string,string>>(data);
                if (commands is null)
                    commands = new Dictionary<string, string>();

                return commands;
            }
        }

        public static void SaveToJson(string path, Dictionary<string,string> data)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
        }
    }
}
