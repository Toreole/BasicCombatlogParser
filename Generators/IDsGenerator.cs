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
        readonly Regex idNameRegex = new Regex("([0-9]+): ([A-Za-z,' -]+)\n?");
        readonly string instanceInfo = @"
# current tier should always be at the top
# raid instances start at the start of the line. 
# their encounters are indented by 2. 
# ': ' seperates id from name.
# enum names are generated based on the full name in here.
# DRAGONFLIGHT
2569: Aberrus, the Shadowed Crucible
  2688: Kazzara, the Hellforged
  2687: The Amalgamation Chamber
  2693: The Forgotten Experiments
  2682: Assault of the Zaqali
  2680: Rashok, the Elder
  2689: The Vigilant Steward, Zskarn
  2683: Magmorax
  2684: Echo of Neltharion
  2685: Scalecommander Sarkareth
2522: Vault of the Incarnates
  2587: Eranog
  2639: Terros
  2590: The Primal Council
  2592: Sennarth, The Cold Breath
  2635: Dathea, Ascended
  2605: Kurog Grimtotem
  2614: Broodkeeper Diurna
  2607: Raszageth the Storm-Eater
# SHADOWLANDS...
2481: Sepulcher of the First Ones
  2512: Vigilant Guardian
  2540: Dausegne, the Fallen Oracle
  2553: Artificer Xy'Mox, again
  2544: Prototype Pantheon
  2542: Skolex, the Insatiable Ravener
  2529: Halondrus, the Reclaimer
  2539: Lihuvim, Principal Architect
  2546: Anduin Wrynn
  2543: Lords of Dread
  2549: Rygelon
  2537: The Jailer
2450: Sanctum of Domination
  2423: The Tarragrue
  2433: The Eye of the Jailer
  2429: The Nine
  2432: Remnant of Ner'zhul
  2434: Soulrender Dormazain
  2430: Painsmith Raznal
  2436: Guardian of the First Ones
  2431: Fatescribe Roh-Kalo
  2435: Sylvanas Windrunner
2296: Castle Nathria
  2398: Shriekwing
  2418: Huntsman Altimor
  2383: Hungering Destroyer
  2402: Sun King's Salvation
  2405: Artificer Xy'Mox
  2406: Lady Inerva Darkvein
  2412: The Council of Blood
  2399: Sludgefist
  2417: Stone Legion Generals
  2407: Sire Denathrius";

        private List<Instance> instances;

        const string InstanceIdTypeName = "InstanceId";
        const string EncounterIdTypeName = "EncounterId";

        //TODO: figure out a way to easily configure the IDs for encounters, instances, and their connections.
        //maybe a json-esque file format, that is located within the project, but how would you get it reliably?
        public void Execute(GeneratorExecutionContext context)
        {
            ParseInstanceInfo();
            StringBuilder instanceIdBuilder = new StringBuilder("    UNKNOWN = 0,\n");
            StringBuilder encounterIdBuilder = new StringBuilder("    UNKNOWN = 0,\n");
            encounterIdBuilder.Append("    All_Bosses = -1,\n");
            for (int i = 0; i < instances.Count; i++)
            {
                var inst = instances[i];
                instanceIdBuilder.Append($"    {inst.enumFriendlyName} = {inst.id}");
                instanceIdBuilder.Append(i == instances.Count -1 ? "" : ",\n");

                encounterIdBuilder.Append($"    // {inst.name}\n");
                foreach(var encounter in inst.Encounters)
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
            foreach(var inst in instances)
            {
                foreach(var encounter in inst.Encounters)
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
                    builder.Append($"            {EncounterIdTypeName}.{encounter.enumFriendlyName}{(i == inst.Encounters.Count-1 ? "\n" : " or\n")}");
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
