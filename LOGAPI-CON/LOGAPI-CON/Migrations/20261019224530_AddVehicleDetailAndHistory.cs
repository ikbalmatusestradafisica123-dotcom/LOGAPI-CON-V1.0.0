using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOGAPI_CON.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleDetailAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "VEHICLES",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LicensePlate",
                table: "VEHICLES",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "VEHICLES",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "VEHICLEHISTORY",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangeByUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VEHICLEHISTORY", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VEHICLEHISTORY_VEHICLES_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "VEHICLES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VEHICLEHISTORY_ChangedAt",
                table: "VEHICLEHISTORY",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VEHICLEHISTORY_VehicleId",
                table: "VEHICLEHISTORY",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VEHICLEHISTORY");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "VEHICLES");

            migrationBuilder.DropColumn(
                name: "LicensePlate",
                table: "VEHICLES");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "VEHICLES");
        }
    }
}
