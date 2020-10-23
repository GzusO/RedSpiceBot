using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedSpiceBot
{
    public class ConfigInfo
    {
        public ConfigIdentity identity;
        public string[] channels;
        public string accessToken;
        public string clientID;
        public string refreshToken;

        public static ConfigInfo LoadInfo(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string config = r.ReadToEnd();
                r.Close();
                return JsonConvert.DeserializeObject<ConfigInfo>(config);
            }
        }

    }

    public class ConfigIdentity
    {
        public string username;
        public string password;
    }
}
