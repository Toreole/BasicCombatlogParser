using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CombatlogParser.Migrations
{
	/// <inheritdoc />
	public partial class AddItemLevelSupport : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				name: "ItemLevel",
				table: "Performances",
				type: "INTEGER",
				nullable: false,
				defaultValue: 0);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "ItemLevel",
				table: "Performances");
		}
	}
}
