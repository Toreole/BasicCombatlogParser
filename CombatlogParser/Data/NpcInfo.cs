namespace CombatlogParser.Data;

public class NpcInfo : CombatlogUnit
{
	public uint NpcId { get; init; }
	public List<string> InstanceGuids { get; } = [];

	public NpcInfo(uint npcId, string name, string firstInstanceGuid)
	{
		NpcId = npcId;
		Name = name;
		GUID = firstInstanceGuid;
		InstanceGuids =
		[
			firstInstanceGuid
		];
	}
}
