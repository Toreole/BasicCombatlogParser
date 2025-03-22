using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CombatlogParser.Migrations
{
	/// <inheritdoc />
	public partial class UpdateEncounterInfoMeta : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<long>(
				name: "EncounterDurationMS",
				table: "Encounters",
				type: "INTEGER",
				nullable: false,
				defaultValue: 0L);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "EncounterDurationMS",
				table: "Encounters");
		}
	}
}
