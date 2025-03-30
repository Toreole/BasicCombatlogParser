using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CombatlogParser.Migrations
{
    /// <inheritdoc />
    public partial class VersionAndLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomLabel",
                table: "CombatlogMetadatas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LogVersion",
                table: "CombatlogMetadatas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomLabel",
                table: "CombatlogMetadatas");

            migrationBuilder.DropColumn(
                name: "LogVersion",
                table: "CombatlogMetadatas");
        }
    }
}
