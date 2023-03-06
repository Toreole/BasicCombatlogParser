using CombatlogParser.Data.Metadata;
using Microsoft.EntityFrameworkCore;

namespace CombatlogParser.DBInteract;
public class CombatlogDBContext : DbContext
{
    public DbSet<CombatlogMetadata> Combatlogs { get; set; }
    public DbSet<EncounterInfoMetadata> Encounters { get; set; }
    public DbSet<PerformanceMetadata> Performances { get; set; }
    public DbSet<PlayerMetadata> Players { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("DataSource=CombatlogMetadata.db; Mode=ReadWriteCreate");
    }

    void Test()
    {
        var entity = new CombatlogMetadata(); //create a blank entity
        Combatlogs.Add(entity); //add it to the context
        this.SaveChanges(); //this updates the DB, and runs some other code to update affected entities
        var storedId = entity.Id; //the entity.Id has been generated in the step before.
    }
}
