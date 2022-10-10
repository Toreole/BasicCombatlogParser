namespace CombatlogParser.Data
{
    /// <summary>
    /// Represents an entire Combatlog file.
    /// </summary>
    public class Combatlog
    {

        //These values can be found at the start of the file.
        public string LogStartTimestamp { get; set; } = "00/00 00:00:00.000";
        public int CombatlogVersion { get; set; } = 19;
        public bool AdvancedLogEnabled { get; set; } = false;
        public string BuildVersion { get; set; } = "9.2.7";
        public int ProjectID { get; set; } = 1;
    }
}
