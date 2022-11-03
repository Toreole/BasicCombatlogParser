namespace CombatlogParser.Data.MetaData
{
    public sealed class CombatlogMetadata
    {
        /// <summary>
        /// unique ID (incrementing int) for easier referencing from other data. <br />
        /// This is set by the DB. changes will not affect this.
        /// </summary>
        public uint uID;
        /// <summary>
        /// The name of the file of the combatlog. Must be UNIQUE and can be used as the PRIMARY KEY
        /// </summary>
        public string fileName = "";
        /// <summary>
        /// Whether the log is advanced
        /// </summary>
        public bool isAdvanced;
        /// <summary>
        /// the unix milliseconds timestamp for the start of the log.
        /// </summary>
        public long msTimeStamp;
        /// <summary>
        /// The version of the build thats responsible for this log.
        /// </summary>
        public string buildVersion ="";
        /// <summary>
        /// the project ID
        /// </summary>
        public WowProjectID projectID;
    }
}
