using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DKPInserter
{
    public class Program
    {

        private HashSet<int> _usedIDs = new HashSet<int>();

        private Dictionary<int, int> _dkp = new Dictionary<int, int>();
        private Dictionary<int, string> _color = new Dictionary<int, string>();

        private int _updatedLines = 0;
        private string _log = "== CHANGE LOG ==";

        public static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            CreateDKP();
            string[] lines = File.ReadAllLines("PipDKP.lua");

            List<string> output = new List<string>();

            foreach (string line in lines)
            {
                if (line.Trim().StartsWith("[") == false)
                {
                    output.Add(line);
                }
                else
                {
                    int id = int.Parse(line.Trim().Substring(1, 5));
                    int value = int.Parse(line.Trim().Split('=')[1].Split(',')[0].Trim().Replace("{", ""));
                    if (_usedIDs.Contains(id))
                    {
                        _log += string.Format("\n - Duplicate of item {0} found and removed.", id);
                        continue;
                    }
                    _usedIDs.Add(id);

                    if (_dkp.ContainsKey(id))
                    {
                        string color = _color[id];
                        if (string.IsNullOrEmpty(color))
                        {
                            color = "|cFF9D9D9D";
                        }

                        output.Add(string.Format("\t[{0}] = {{{1}, \"{2}\"}},", id, _dkp[id], color));

                        if (_dkp[id] != value)
                        {
                            _log += string.Format("\n + Changed value of item {0} from {1}, to {2}", id, value, _dkp[id]);
                        }
                        _updatedLines++;
                    }
                    else
                    {
                        output.Add(line);
                    }
                }
            }

            Console.WriteLine(string.Format("Updated {0} Items.", _updatedLines));
            File.WriteAllLines("PipDKP_Update.lua", output.ToArray());
            File.WriteAllText("./Run_" + DateTime.Now.Ticks + ".log", _log);
        }

        private void CreateDKP()
        {
            string[] toUpdate = File.ReadAllLines("update.csv");
            for (int i = 0; i < toUpdate.Length; i++)
            {
                string line = toUpdate[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (Regex.IsMatch(line, "\\d+,\\d+"))
                {
                    string[] content = line.Split(',');

                    int id = int.Parse(content[0].Trim());
                    int value = int.Parse(content[1].Trim());
                    string color = content[2].Trim();

                    if (_dkp.ContainsKey(id) == false)
                    {
                        _dkp.Add(id, value);
                        _color.Add(id, color);
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Duplicate Item with id {0} found.", id));
                    }
                }
            }
        }
    }
}
