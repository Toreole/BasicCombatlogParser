namespace CombatlogParser
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            //1. establish SQLite connection
            //2. Ensure that the required tables for metaData are in place.
            //3. Read config file, set defaults if it doesnt already exist.
            //4. Open the MainWindow to start the application.
            SQLitePCL.Batteries.Init();
            //the "arguments" for the connection string are seperated with ;
            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=hello.db; Mode=ReadWriteCreate"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                //creates a table if one of the same name does not exist yet.
                //id is an integer, and used as the primary key (needs to be unique) and is automatically incremented.
                //name is just a plaintext string.
                command.CommandText =
                    @"
                        CREATE TABLE IF NOT EXISTS users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT
                        );
                    ";
                command.ExecuteNonQuery();

                var insertDataCommand = connection.CreateCommand();
                //inserting one row (an additional entry) into the users table. 
                //only value specified is the name, because id is autoincrement.
                insertDataCommand.CommandText =
                    @"
                        INSERT INTO users (name)
                        VALUES($varName);
                    ";
                var nameParam = insertDataCommand.Parameters.AddWithValue("$varName", "Flora"); //parameters can be easily replaced in the string
                string[] names = { "Flora", "Bertha", "Hubert", "Daniel", "Michael", "Yeeter", "Emko", "Job", "Bracketman"};
                foreach(var n in names)
                {
                    nameParam.Value = n;
                    insertDataCommand.ExecuteNonQuery();
                }

                var selectCommand = connection.CreateCommand();
                selectCommand.CommandText = "SELECT name FROM users WHERE id = $varID;";
                int searchID = 1;
                var idParam = selectCommand.Parameters.AddWithValue("$varID", searchID);
                while(true)
                {
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read()) //read the results in order that they were selected in. this case: only name. true = there is a value there.
                        {
                            var name = reader.GetString(0);

                            bool correct = name == "Hubert";
                        }
                        else break;
                    }
                    searchID++;
                    idParam.Value = searchID;
                }

                connection.Close();
            }
            return;
            MainWindow window = new();
            window.ShowDialog();
        }
    }
}