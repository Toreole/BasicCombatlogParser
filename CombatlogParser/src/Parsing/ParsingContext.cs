using System;
using System.IO;
using System.Text;

namespace CombatlogParser.Parsing;

internal class ParsingContext : IDisposable
{
    private readonly Dictionary<string, string> uniqueUnhandledSubevents = new();

    internal void RegisterUnhandledSubevent(string subevent, string contextLine)
    {
        if (!uniqueUnhandledSubevents.ContainsKey(subevent))
        {
            uniqueUnhandledSubevents.Add(subevent, contextLine);
        }
    }

    public void Dispose()
    {
        if (uniqueUnhandledSubevents.Count == 0)
            return;

        var time = DateTimeOffset.Now;
        string fileName = Path.Combine("Logs", $"Parse_Log_{time.ToUnixTimeMilliseconds()}.txt");
        if (!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");
        FileMode openMode = File.Exists(fileName) ? FileMode.Open : FileMode.Create;
        using FileStream stream = File.Open(fileName, openMode);
        using StreamWriter writer = new StreamWriter(stream);
        
        if(uniqueUnhandledSubevents.Count > 0)
        {
            foreach(var pair in uniqueUnhandledSubevents)
            {
                writer.WriteLine($"Unhandled subevent: {pair.Key} | Example: {pair.Value}");
            }
        }

        writer.Flush();
        stream.Flush();
    }
}
