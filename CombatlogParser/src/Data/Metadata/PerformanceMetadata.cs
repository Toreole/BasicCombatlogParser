namespace CombatlogParser.src.Data.Metadata
{
    public sealed class PerformanceMetadata
    {
        /// <summary>
        /// unique ID of the performance. autoincrement
        /// </summary>
        public uint performanceUID;
        /// <summary>
        /// GUID of the player
        /// </summary>
        public string playerGUID = "";
        /// <summary>
        /// encounterInfo UID
        /// </summary>
        public uint encounterUID;
        public double dps;
        public double hps;
        public byte roleID;
        public byte specID;
    }
}