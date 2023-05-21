using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Generators
{
    [Generator]
    public class IDsGenerator : ISourceGenerator
    {
        readonly Regex idNameRegex = new Regex("([0-9]+): ([A-Za-z, ]+)\n?");
        readonly string instanceInfo = @"
# current tier should always be at the top
# raid instances start at the start of the line. 
# their encounters are indented by 2. 
# ': ' seperates id from name.
# enum names are generated based on the full name in here.
# DRAGONFLIGHT
2546: Aberrus, the Shadowed Crucible
  4135: Kazzara, the Hellforged
  4138: Assault of the Zaqali
  4140: The Amalgamation Chamber
2461: Vault of the Incarnates
  4216: Eranog
  4221: The Primalist Council
  4231: Terros
  4218: Kurog Grimtotem
# SHADOWLANDS...";
        private List<Instance> instances;

        //TODO: figure out a way to easily configure the IDs for encounters, instances, and their connections.
        //maybe a json-esque file format, that is located within the project, but how would you get it reliably?
        public void Execute(GeneratorExecutionContext context)
        {
            ParseInstanceInfo();
            StringBuilder instanceIdBuilder = new StringBuilder();
            for(int i = 0; i < instances.Count; i++)
            {
                var inst = instances[i];
                instanceIdBuilder.Append($"    {inst.name.ToEnumString()} = {inst.id}");
                instanceIdBuilder.Append(i == instances.Count -1 ? "" : ",\n");
            }

            string generatedSource = $@"// <auto generated />
namespace CombatlogParser.Generated;

public enum InstanceIDs
{{
{instanceIdBuilder}
}}
";
            //string generatedSource = string.Format(sourceTemplate, instanceIdBuilder.ToString());
            //string executingPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"TextFile.txt");
            //DiagnosticDescriptor descriptor = new DiagnosticDescriptor("CLG00001", "Hello", executingPath, "Debug", DiagnosticSeverity.Warning, true);
            //Diagnostic report = Diagnostic.Create(descriptor, null);
            //context.ReportDiagnostic(report);
            context.AddSource("GeneratedIDs.generated.cs", generatedSource);
        }

        private void ParseInstanceInfo()
        {
            //initialized needs to process the instance string and produce Instance[] 
            StringReader reader = new StringReader(instanceInfo);
            instances = new List<Instance>();
            Instance currentInstance = null;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                //skip comments and empty lines
                if (line.Length <= 1 || string.IsNullOrWhiteSpace(line) || line[0] == '#')
                {
                    continue;
                }
                //DiagnosticDescriptor descriptor = new DiagnosticDescriptor("CLG00002", "Line Value", $"'{line}' length: {line.Length}", "Debug", DiagnosticSeverity.Info, true);
                //Diagnostic report = Diagnostic.Create(descriptor, null);
                //context.ReportDiagnostic(report);
                //continue;
                //get match based on regex
                var match = idNameRegex.Match(line);
                if (line[0] == ' ')
                {
                    currentInstance?.Encounters.Add(new Encounter(match.Groups[1].Value, match.Groups[2].Value));
                }
                else
                {
                    if (currentInstance != null)
                    {
                        instances.Add(currentInstance);
                    }
                    currentInstance = new Instance(match.Groups[1].Value, match.Groups[2].Value);
                }
            }
            //add last instance if needed.
            if (currentInstance != null && !instances.Contains(currentInstance))
            {
                instances.Add(currentInstance);
            }
            reader.Dispose();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            
        }
    }

    public class Instance
    {
        public readonly string name;
        public readonly string id;
        public List<Encounter> Encounters { get; }
        public Instance(string id, string name)
        {
            this.name = name;
            this.id = id;
            this.Encounters = new List<Encounter>(12); //i dont think theres any raid with more than 12 encounters.
        }
    }
    public class Encounter
    {
        public readonly string name;
        public readonly string id;
        public Encounter(string id, string name)
        {
            this.name = name;
            this.id = id;
        }
    }

    public static class StringExtensions
    {
        public static string ToEnumString(this string value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == ',') continue;
                if (c == ' ') c = '_';
                stringBuilder.Append(c);
            }
            return stringBuilder.ToString();
        }
    }
    
}
