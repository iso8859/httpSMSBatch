using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class CommandLineParser
{
    public Dictionary<string, List<string>> args = new Dictionary<string, List<string>>();

    public static CommandLineParser Parse() => Parse(Environment.CommandLine);

    public static CommandLineParser Parse(string cmdLine)
    {
        CommandLineParser result = new CommandLineParser();
        string[] args = cmdLine.Split(' ');
        string current = "";
        List<string> accumulator = new List<string>();
        string searchForEnd = "";
        foreach (string item in args)
        {
            string item0 = item.Trim('\r', '\n');
            string current0 = "";
            if (item0.StartsWith("-") || item0.StartsWith("/"))
                current0 = item0.Substring(1);
            if (item0.StartsWith("--"))
                current0 = item0.Substring(2);
            if (current0.Length > 0)
            {
                if (current.Length > 0)
                {
                    if (!result.args.ContainsKey(current))
                        result.args[current] = new List<string>();
                    if (accumulator.Count > 0)
                        result.args[current].Add(string.Join(" ", accumulator.ToArray()));
                    else
                        result.args[current].Add("true");
                }
                current = current0;
                accumulator.Clear();
                searchForEnd = "";
            }
            else
            {
                if (searchForEnd.Length > 0)
                {
                    if (item0.EndsWith(searchForEnd))
                    {
                        accumulator.Add(item0.Substring(0, item0.Length - searchForEnd.Length));
                        searchForEnd = "";
                    }
                    else
                        accumulator.Add(item0);
                }
                else
                {
                    if (item0.StartsWith("'") && accumulator.Count == 0)
                    {
                        var tmp = item0.Trim('\'');
                        accumulator.Add(tmp);
                        searchForEnd = "'";
                    }
                    else if (item0.StartsWith("\"") && accumulator.Count == 0)
                    {
                        var tmp = item0.Trim('\"');
                        accumulator.Add(tmp);
                        searchForEnd = "\"";
                    }
                    else if (item0.StartsWith("#") && accumulator.Count == 0)
                    {
                        var tmp = item0.Trim('#');
                        accumulator.Add(tmp);
                        searchForEnd = "#";
                    }
                    else
                        accumulator.Add(item0);
                }
            }
        }
        if (current.Length > 0)
        {
            if (!result.args.ContainsKey(current))
                result.args[current] = new List<string>();
            if (accumulator.Count > 0)
                result.args[current].Add(string.Join(" ", accumulator.ToArray()));
            else
                result.args[current].Add("true");

        }
        return result;
    }

    public string GetString(string name, string _default = "", int index = 0)
    {
        string result = _default;
        if (args.ContainsKey(name))
        {
            var list = args[name];
            if (index < list.Count)
                result = list[index];
        }
        return result;
    }

    public bool GetBool(string name, bool _default = false, int index = 0)
    {
        var tmp = GetString(name, "", index);
        if (tmp == "true")
            return true;
        else
            return _default;
    }

    public int GetInt32(string name, int _default = -1, int index = 0)
    {
        var tmp = GetString(name, _default.ToString(), index);
        int result = 0;
        if (!int.TryParse(tmp, out result))
            result = _default;
        return result;
    }

    // Try to find the file, absolute, running directory, binnary directory
    public FileInfo GetFile(string name, string _default = "", int index = 0)
    {
        string file = GetString(name, _default, index);
        FileInfo fi = new FileInfo(name);
        if (fi.Exists)
            return fi;
        fi = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(file)));
        if (fi.Exists)
            return fi;
        return new FileInfo(Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), Path.GetFileName(file)));
    }

    public string GetEnv(string name, string _default = "", int index = 0)
    {
        string result = GetString(name, _default, index);
        if (!string.IsNullOrEmpty(result))
            result = Environment.GetEnvironmentVariable(result);
        return result;
    }

    // You can use the dictionnary or getters
    public void Dump()
    {
        foreach (var arg in args)
        {
            Console.Write(arg.Key + "=");
            string j = "";
            int index = 0;
            string sep = "";
            while ((j = GetString(arg.Key, "", index++)) != "")
            {
                Console.Write(sep + j);
                sep = ", ";
            }
            Console.WriteLine();
        }
    }
}
