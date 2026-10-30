using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOGAPI_CON.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServerSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DELIVERYORDER",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Customer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsPriority = table.Column<bool>(type: "bit", nullable: false),
                    PriorityReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DELIVERYORDER", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VEHICLES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Capacity = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VEHICLES", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ORDERSTATUSHISTORIES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PreviousStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TicketReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDERSTATUSHISTORIES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ORDERSTATUSHISTORIES_DELIVERYORDER_OrderId",
                        column: x => x.OrderId,
                        principalTable: "DELIVERYORDER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DELIVERYORDER_ScheduledDate",
                table: "DELIVERYORDER",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_DELIVERYORDER_Status",
                table: "DELIVERYORDER",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERSTATUSHISTORIES_ChangedAt",
                table: "ORDERSTATUSHISTORIES",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERSTATUSHISTORIES_OrderId",
                table: "ORDERSTATUSHISTORIES",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_VEHICLES_IsAvailable",
                table: "VEHICLES",
                column: "IsAvailable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ORDERSTATUSHISTORIES");

            migrationBuilder.DropTable(
                name: "VEHICLES");

            migrationBuilder.DropTable(
                name: "DELIVERYORDER");
        }
    }
}
