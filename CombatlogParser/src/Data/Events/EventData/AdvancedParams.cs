using static CombatlogParser.ParsingUtil;

namespace CombatlogParser.Data.Events;

public class AdvancedParams
{
    public readonly string infoGUID;
    public readonly string ownerGUID;
    public readonly int currentHP;
    public readonly int maxHP;
    public readonly int attackPower;
    public readonly int spellPower;
    public readonly int armor;
    public readonly int absorb;
    public readonly PowerType[] powerType;
    public readonly int[] currentPower;
    public readonly int[] maxPower;
    public readonly int[] powerCost;
    public readonly float positionX;
    public readonly float positionY;
    public readonly int uiMapID;
    public readonly float facing;
    public readonly int level;

    public AdvancedParams(string data, ref int dataIndex)
    {
        infoGUID = string.Intern(NextSubstring(data, ref dataIndex));
        ownerGUID = string.Intern(NextSubstring(data, ref dataIndex));
        currentHP = int.Parse(NextSubstring(data, ref dataIndex));
        maxHP = int.Parse(NextSubstring(data, ref dataIndex));
        attackPower = int.Parse(NextSubstring(data, ref dataIndex));
        spellPower = int.Parse(NextSubstring(data, ref dataIndex));
        armor = int.Parse(NextSubstring(data, ref dataIndex));
        absorb = int.Parse(NextSubstring(data, ref dataIndex));

        powerType = AllPowerTypesIn(NextSubstring(data, ref dataIndex));
        currentPower = AllIntsIn(NextSubstring(data, ref dataIndex));
        maxPower = AllIntsIn(NextSubstring(data, ref dataIndex));
        powerCost = AllIntsIn(NextSubstring(data, ref dataIndex));
        positionX = float.Parse(NextSubstring(data, ref dataIndex), FloatNumberFormat);
        positionY = float.Parse(NextSubstring(data, ref dataIndex), FloatNumberFormat);
        uiMapID = int.Parse(NextSubstring(data, ref dataIndex));
        facing = float.Parse(NextSubstring(data, ref dataIndex), FloatNumberFormat);
        level = int.Parse(NextSubstring(data, ref dataIndex));
    }
}