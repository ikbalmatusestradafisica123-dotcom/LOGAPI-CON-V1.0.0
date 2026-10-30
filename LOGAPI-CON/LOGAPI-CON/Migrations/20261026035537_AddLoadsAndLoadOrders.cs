using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOGAPI_CON.Migrations
{
    /// <inheritdoc />
    public partial class AddLoadsAndLoadOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LOADS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    RouteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TotalWeight = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOADS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LOADS_VEHICLES_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "VEHICLES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LOADORDERS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoadId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    AssignedWeight = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOADORDERS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LOADORDERS_DELIVERYORDER_OrderId",
                        column: x => x.OrderId,
                        principalTable: "DELIVERYORDER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LOADORDERS_LOADS_LoadId",
                        column: x => x.LoadId,
                        principalTable: "LOADS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LOADORDERS_LoadId_OrderId",
                table: "LOADORDERS",
                columns: new[] { "LoadId", "OrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LOADORDERS_OrderId",
                table: "LOADORDERS",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_LOADS_VehicleId_RouteDate",
                table: "LOADS",
                columns: new[] { "VehicleId", "RouteDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LOADORDERS");

            migrationBuilder.DropTable(
                name: "LOADS");
        }
    }
}
