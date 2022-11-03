namespace CombatlogParser
{
    public static partial class Queries
    {
        public static void Test()
        {
            using var command = DB.CreateCommand();
            if(command != null)
            {
                command.CommandText = "";
                command.ExecuteNonQuery();
            }
        }
    }
}
