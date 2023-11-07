using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Generators
{
    [Generator]
    public class IDsGenerator : ISourceGenerator
    {
        readonly Regex idNameRegex = new Regex("([0-9]+): ([A-Za-z,' -]+)\n?");

        private List<Instance> instances;

        const string InstanceIdTypeName = "InstanceId";
        const string EncounterIdTypeName = "EncounterId";

        public void Execute(GeneratorExecutionContext context)
        {
            var additionalFile = context.AdditionalFiles.First(x => x.Path.EndsWith("Encounters.txt")); //should filter by name in case multiple files are there.
            var inputString = additionalFile.GetText().ToString();
            ParseInstanceInfo(inputString);
            StringBuilder instanceIdBuilder = new StringBuilder("    UNKNOWN = 0,\n");
            StringBuilder encounterIdBuilder = new StringBuilder("    UNKNOWN = 0,\n");
            encounterIdBuilder.Append("    All_Bosses = -1,\n");
            for (int i = 0; i < instances.Count; i++)
            {
                var inst = instances[i];
                instanceIdBuilder.Append($"    {inst.enumFriendlyName} = {inst.id}");
                instanceIdBuilder.Append(i == instances.Count - 1 ? "" : ",\n");

                encounterIdBuilder.Append($"    // {inst.name}\n");
                foreach (var encounter in inst.Encounters)
                {
                    encounterIdBuilder.Append($"    {encounter.enumFriendlyName} = {encounter.id},\n");
                }
            }

            AddEnumSource(context, InstanceIdTypeName, instanceIdBuilder);
            AddEnumSource(context, EncounterIdTypeName, encounterIdBuilder);
            AddUtilitySource(context);

            //string executingPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"TextFile.txt");
            //DiagnosticDescriptor descriptor = new DiagnosticDescriptor("CLG00001", "Hello", executingPath, "Debug", DiagnosticSeverity.Warning, true);
            //Diagnostic report = Diagnostic.Create(descriptor, null);
            //context.ReportDiagnostic(report);
        }

        private void AddEnumSource(GeneratorExecutionContext context, string typeName, StringBuilder enumValuesBuilder)
        {
            string generatedSource = $@"// <auto generated />
namespace CombatlogParser;

public enum {typeName}
{{
{enumValuesBuilder}
}}
";
            context.AddSource($"{typeName}.g.cs", generatedSource);
        }

        private void AddUtilitySource(GeneratorExecutionContext context)
        {
            string generatedSource = $@"// <auto generated />
namespace CombatlogParser;

public static class InstanceEncounterUtility
{{
    public static {EncounterIdTypeName}[] GetEncounters(this {InstanceIdTypeName} instance) 
    {{
        return instance switch
        {{ 
{GetInstanceToEncounterIdArrayMapping()}
            _ => Array.Empty<{EncounterIdTypeName}>()
        }};
    }}

    public static {InstanceIdTypeName} GetInstance(this {EncounterIdTypeName} encounter)
    {{
        return encounter switch
        {{
{GetEncounterToInstanceIdMapping()}
            _ => {InstanceIdTypeName}.UNKNOWN
        }};
    }}
    
    public static string GetDisplayName(this {InstanceIdTypeName} instance)
    {{
        return instance switch
        {{
{GetInstanceNameMapping()}
            _ => ""Unknown""
        }};
    }}

    public static string GetDisplayName(this {EncounterIdTypeName} encounter)
    {{
        return encounter switch
        {{
{GetEncounterNameMapping()}
            _ => ""Unknown""
        }};
    }}
}}
";
            context.AddSource("InstanceEncounterUtility.g.cs", generatedSource);
        }

        private string GetEncounterNameMapping()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"            {EncounterIdTypeName}.All_Bosses => \"All Bosses\",\n");
            foreach (var inst in instances)
            {
                foreach (var encounter in inst.Encounters)
                {
                    builder.Append($"            {EncounterIdTypeName}.{encounter.enumFriendlyName} => \"{encounter.name}\",\n");
                }
            }
            return builder.ToString();
        }

        private string GetInstanceNameMapping()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var inst in instances)
            {
                builder.Append($"            {InstanceIdTypeName}.{inst.enumFriendlyName} => \"{inst.name}\",\n");
            }
            return builder.ToString();
        }

        private string GetEncounterToInstanceIdMapping()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var inst in instances)
            {
                for (int i = 0; i < inst.Encounters.Count; i++)
                {
                    var encounter = inst.Encounters[i];
                    builder.Append($"            {EncounterIdTypeName}.{encounter.enumFriendlyName}{(i == inst.Encounters.Count - 1 ? "\n" : " or\n")}");
                }
                builder.Append($"            => {InstanceIdTypeName}.{inst.enumFriendlyName},\n");
            }
            return builder.ToString();
        }

        private string GetInstanceToEncounterIdArrayMapping()
        {
            StringBuilder builder = new StringBuilder();
            //instance => new EncounterId[] {.....}
            foreach (var inst in instances)
            {
                builder.Append(
$@"            {InstanceIdTypeName}.{inst.enumFriendlyName} => new[] 
            {{
");
                for (int i = 0; i < inst.Encounters.Count; i++)
                {
                    var encounter = inst.Encounters[i];
                    builder.Append(
$"                {EncounterIdTypeName}.{encounter.enumFriendlyName}"
                    );
                    builder.Append(i == inst.Encounters.Count - 1 ? "\n" : ",\n");
                }
                builder.Append("            },\n");
            }
            return builder.ToString();
        }

        private void ParseInstanceInfo(string info)
        {
            //initialized needs to process the instance string and produce Instance[] 
            StringReader reader = new StringReader(info);
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
                    currentInstance?.Encounters.Add(new Encounter(match.Groups[1].Value, match.Groups[2].Value.Trim()));
                }
                else
                {
                    if (currentInstance != null)
                    {
                        instances.Add(currentInstance);
                    }
                    currentInstance = new Instance(match.Groups[1].Value, match.Groups[2].Value.Trim());
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
        public readonly string enumFriendlyName;
        public readonly string id;
        public List<Encounter> Encounters { get; }
        public Instance(string id, string name)
        {
            this.name = name;
            this.enumFriendlyName = name.ToEnumString();
            this.id = id;
            this.Encounters = new List<Encounter>(12); //i dont think theres any raid with more than 12 encounters.
        }
    }
    public class Encounter
    {
        public readonly string name;
        public readonly string enumFriendlyName;
        public readonly string id;
        public Encounter(string id, string name)
        {
            this.name = name;
            this.enumFriendlyName = name.ToEnumString();
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
                if (c == ',' || c == '\'') continue; //, and ' are omitted
                if (c == ' ' || c == '-') c = '_'; //spaces and - are replaced by _ 
                stringBuilder.Append(c);
            }
            return stringBuilder.ToString();
        }
    }

}
