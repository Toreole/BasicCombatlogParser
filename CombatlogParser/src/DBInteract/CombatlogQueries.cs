using CombatlogParser.Data.Metadata;

namespace CombatlogParser
{
    public static partial class Queries
    {
        /// <summary>
        /// Attempts getting the combatlog metadata for a log based on its ID.
        /// </summary>
        /// <param name="logID"></param>
        /// <returns>null if no such log can be found.</returns>
        public static CombatlogMetadata? GetCombatlogMetadataByID(uint logID)
        {
           
            return null;
        }

        /// <summary>
        /// Checks whether a combatlog has already been imported by confirming whether a log with the same fileName exists in the metadata table.
        /// </summary>
        public static bool CombatlogAlreadyImported(string fileName)
        {
           
            return false;
        }

        /// <summary>
        /// Gets the number of rows in the Combatlog_Metadata table.
        /// </summary>
        public static int GetCombatlogMetadataCount()
        {
            
            return 0; 
        }
    }
}
