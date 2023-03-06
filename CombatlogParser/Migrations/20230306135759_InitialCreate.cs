using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CombatlogParser.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CombatlogMetadatas",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    IsAdvanced = table.Column<bool>(type: "INTEGER", nullable: false),
                    MsTimeStamp = table.Column<long>(type: "INTEGER", nullable: false),
                    BuildVersion = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombatlogMetadatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GUID = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Realm = table.Column<string>(type: "TEXT", nullable: false),
                    ClassId = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Encounters",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EncounterStartIndex = table.Column<long>(type: "INTEGER", nullable: false),
                    WowEncounterId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    DifficultyId = table.Column<int>(type: "INTEGER", nullable: false),
                    EncounterLengthInFile = table.Column<int>(type: "INTEGER", nullable: false),
                    CombatlogMetadataId = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Encounters_CombatlogMetadatas_CombatlogMetadataId",
                        column: x => x.CombatlogMetadataId,
                        principalTable: "CombatlogMetadatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Performances",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Dps = table.Column<double>(type: "REAL", nullable: false),
                    Hps = table.Column<double>(type: "REAL", nullable: false),
                    RoleId = table.Column<byte>(type: "INTEGER", nullable: false),
                    SpecId = table.Column<byte>(type: "INTEGER", nullable: false),
                    WowEncounterId = table.Column<uint>(type: "INTEGER", nullable: false),
                    EncounterInfoMetadataId = table.Column<uint>(type: "INTEGER", nullable: false),
                    PlayerMetadataId = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Performances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Performances_Encounters_EncounterInfoMetadataId",
                        column: x => x.EncounterInfoMetadataId,
                        principalTable: "Encounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Performances_Players_PlayerMetadataId",
                        column: x => x.PlayerMetadataId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_CombatlogMetadataId",
                table: "Encounters",
                column: "CombatlogMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_Performances_EncounterInfoMetadataId",
                table: "Performances",
                column: "EncounterInfoMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_Performances_PlayerMetadataId",
                table: "Performances",
                column: "PlayerMetadataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Performances");

            migrationBuilder.DropTable(
                name: "Encounters");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "CombatlogMetadatas");
        }
    }
}
