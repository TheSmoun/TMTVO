using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data.Ini
{
    public class IniFile
    {
        private Dictionary<string, string> fileValue;
        public string Path { get; private set; }

        public IniFile(string path)
        {
            Path = path;
            fileValue = new Dictionary<string, string>();
            parse();
        }

        private void parse()
        {
            StreamReader reader = File.OpenText(Path);
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                string[] kv = line.Split(new char[] { '=' });
                fileValue.Add(kv[0], kv[1]);
            }
        }

        public string GetValue(string key)
        {
            string v = null;
            fileValue.TryGetValue(key, out v);
            return (v != null) ? v : '!' + key + '!';
        }
    }
}
