namespace CombatlogParser.Data
{
    public class DamageSummary
    {
        public string SourceName { get; set; } = string.Empty;
        public long TotalDamage { get; set; }
        public float DPS { get; set; }
    }
}
