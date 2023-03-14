namespace CombatlogParser.Data
{
    public class NpcInfo
    {
        public uint NpcId { get; init; }
        public string Name { get; init; } = string.Empty;
        public List<string> InstanceGuids { get; } = new List<string>();

        public NpcInfo(uint npcId, string name, string firstInstanceGuid)
        {
            NpcId = npcId;
            Name = name;
            InstanceGuids = new()
            {
                firstInstanceGuid
            };
        }
    }
}
