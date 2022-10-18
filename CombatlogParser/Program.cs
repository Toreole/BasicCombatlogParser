namespace CombatlogParser
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            SQLitePCL.Batteries.Init();
            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=hello.db Mode=ReadWriteCreate"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                    @"
                        CREATE TABLE IF NOT EXISTS users (
                        id INTEGER PRIMARY KEY,
                        name TEXT
                        );
                    ";
                command.ExecuteNonQuery();

                var insertDataCommand = connection.CreateCommand();
                insertDataCommand.CommandText =
                    @"
                        INSERT INTO users (id, name)
                        VALUES( $varID, $varName);
                    ";
                insertDataCommand.Parameters.AddWithValue("$varName", "Daniel");
                insertDataCommand.Parameters.AddWithValue("$varID", 4);
                insertDataCommand.ExecuteNonQuery();

                var selectCommand = connection.CreateCommand();
                selectCommand.CommandText = "SELECT name FROM users WHERE id = $varID;";
                selectCommand.Parameters.AddWithValue("$varID", 3);
                using (var reader = selectCommand.ExecuteReader())
                {
                    if(reader.Read())
                    {
                        var name = reader.GetString(0);

                        bool correct = name == "Hubert";
                    }
                }

                connection.Close();
            }
            return;
            MainWindow window = new();
            window.ShowDialog();
        }
    }
}