using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaScale.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSubdivisionLegacyCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LegacyCode",
                table: "Subdivisions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LegacyCode",
                table: "Subdivisions");
        }
    }
}
