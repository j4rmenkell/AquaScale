using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaScale.Api.Migrations
{
    /// <inheritdoc />
    public partial class MeterReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_meters_properties_property_id",
                table: "meters");

            migrationBuilder.AlterColumn<Guid>(
                name: "property_id",
                table: "meters",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "meters",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "date_marked_non_operational",
                table: "meters",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "meter_status",
                table: "meters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "qr_code",
                table: "meters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "utility_type",
                table: "meters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "meter_readings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    captured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gps_lat = table.Column<decimal>(type: "numeric", nullable: true),
                    gps_lng = table.Column<decimal>(type: "numeric", nullable: true),
                    seconds_since_last_capture = table.Column<int>(type: "integer", nullable: true),
                    photo_url = table.Column<string>(type: "text", nullable: true),
                    qr_scanned_code = table.Column<string>(type: "text", nullable: true),
                    ocr_reading_value = table.Column<decimal>(type: "numeric", nullable: true),
                    confidence_score = table.Column<decimal>(type: "numeric", nullable: true),
                    previous_reading = table.Column<decimal>(type: "numeric", nullable: true),
                    recapture_count = table.Column<int>(type: "integer", nullable: false),
                    is_duplicate_flag = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_meter_readings", x => x.id);
                    table.ForeignKey(
                        name: "fk_meter_readings_meters_meter_id",
                        column: x => x.meter_id,
                        principalTable: "meters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_meter_readings_profiles_field_worker_id",
                        column: x => x.field_worker_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_meter_readings_profiles_reviewed_by",
                        column: x => x.reviewed_by,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_meter_readings_field_worker_id",
                table: "meter_readings",
                column: "field_worker_id");

            migrationBuilder.CreateIndex(
                name: "ix_meter_readings_meter_id",
                table: "meter_readings",
                column: "meter_id");

            migrationBuilder.CreateIndex(
                name: "ix_meter_readings_reviewed_by",
                table: "meter_readings",
                column: "reviewed_by");

            migrationBuilder.AddForeignKey(
                name: "fk_meters_properties_property_id",
                table: "meters",
                column: "property_id",
                principalTable: "properties",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_meters_properties_property_id",
                table: "meters");

            migrationBuilder.DropTable(
                name: "meter_readings");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "meters");

            migrationBuilder.DropColumn(
                name: "date_marked_non_operational",
                table: "meters");

            migrationBuilder.DropColumn(
                name: "meter_status",
                table: "meters");

            migrationBuilder.DropColumn(
                name: "qr_code",
                table: "meters");

            migrationBuilder.DropColumn(
                name: "utility_type",
                table: "meters");

            migrationBuilder.AlterColumn<Guid>(
                name: "property_id",
                table: "meters",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "fk_meters_properties_property_id",
                table: "meters",
                column: "property_id",
                principalTable: "properties",
                principalColumn: "id");
        }
    }
}
