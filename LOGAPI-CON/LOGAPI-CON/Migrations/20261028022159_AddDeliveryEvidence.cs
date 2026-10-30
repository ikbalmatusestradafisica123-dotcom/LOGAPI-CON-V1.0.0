using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOGAPI_CON.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DELIVERYEVIDENCES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    EvidenceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DELIVERYEVIDENCES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DELIVERYEVIDENCES_DELIVERYORDER_OrderId",
                        column: x => x.OrderId,
                        principalTable: "DELIVERYORDER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DELIVERYEVIDENCES_OrderId",
                table: "DELIVERYEVIDENCES",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DELIVERYEVIDENCES");
        }
    }
}
