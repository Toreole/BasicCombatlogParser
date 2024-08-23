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
}
