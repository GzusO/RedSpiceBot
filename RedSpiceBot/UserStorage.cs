using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedSpiceBot
{
    public class UserStorage
    {
        public string displayName; // Track the users display name here cause why not
        public int spice;
        public List<Artifact> artifacts;

        public static Dictionary<string, UserStorage> LoadStorage(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string storageString = r.ReadToEnd();
                r.Close();
                return JsonConvert.DeserializeObject<Dictionary<string, UserStorage>>(storageString);
            }
        }
    }
}
