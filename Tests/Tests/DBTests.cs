using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatlogParser.Tests
{
    public class DBTests
    {
        [Test]
        public void TestDBTableStringCreation()
        {
            DB.DBTable t = new("tableName", new DB.DBColumn[] {
                new("id", "INTEGER PRIMARY KEY"),
                new("name", "TEXT")
            });

            Assert.That(t.ToTableCreationString(), Is.EqualTo("tableName ( id INTEGER PRIMARY KEY, name TEXT )"));
        }
    }
}
