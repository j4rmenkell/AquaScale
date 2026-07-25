using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaScale.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMeterStubForMirrorJoinTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "meters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mirror_acctmtr_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_meters", x => x.id);
                    table.ForeignKey(
                        name: "fk_meters_mirror_account_meters_mirror_acctmtr_id",
                        column: x => x.mirror_acctmtr_id,
                        principalTable: "mirror_account_meters",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_meters_mirror_acctmtr_id",
                table: "meters",
                column: "mirror_acctmtr_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meters");
        }
    }
}
