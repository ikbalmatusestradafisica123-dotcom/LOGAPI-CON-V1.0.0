using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOGAPI_CON.Migrations
{
    /// <inheritdoc />
    public partial class AddPartialDeliveries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PARTIALDELIVERIES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderedWeight = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    DeliveredWeight = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    PendingWeight = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RescheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegisteredByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RegisteredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PARTIALDELIVERIES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PARTIALDELIVERIES_DELIVERYORDER_OrderId",
                        column: x => x.OrderId,
                        principalTable: "DELIVERYORDER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PARTIALDELIVERIES_OrderId",
                table: "PARTIALDELIVERIES",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PARTIALDELIVERIES");
        }
    }
}
