using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaScale.Api.Migrations
{
    /// <inheritdoc />
    public partial class PropertyAndMeterMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "property_id",
                table: "meters",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "subdivisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    geojson_boundary = table.Column<string>(type: "text", nullable: true),
                    mobile_data_provider = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subdivisions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "properties",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subdivision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    block = table.Column<string>(type: "text", nullable: true),
                    lot = table.Column<string>(type: "text", nullable: true),
                    comp_pbl = table.Column<string>(type: "text", nullable: true),
                    mirror_account_no = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_properties", x => x.id);
                    table.ForeignKey(
                        name: "fk_properties_subdivisions_subdivision_id",
                        column: x => x.subdivision_id,
                        principalTable: "subdivisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_meters_property_id",
                table: "meters",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "ix_properties_subdivision_id",
                table: "properties",
                column: "subdivision_id");

            migrationBuilder.AddForeignKey(
                name: "fk_meters_properties_property_id",
                table: "meters",
                column: "property_id",
                principalTable: "properties",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_meters_properties_property_id",
                table: "meters");

            migrationBuilder.DropTable(
                name: "properties");

            migrationBuilder.DropTable(
                name: "subdivisions");

            migrationBuilder.DropIndex(
                name: "ix_meters_property_id",
                table: "meters");

            migrationBuilder.DropColumn(
                name: "property_id",
                table: "meters");
        }
    }
}
