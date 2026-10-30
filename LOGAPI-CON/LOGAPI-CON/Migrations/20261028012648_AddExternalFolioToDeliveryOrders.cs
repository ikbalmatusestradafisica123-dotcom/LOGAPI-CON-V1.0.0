using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOGAPI_CON.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalFolioToDeliveryOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalFolio",
                table: "DELIVERYORDER",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DELIVERYORDER_ExternalFolio",
                table: "DELIVERYORDER",
                column: "ExternalFolio",
                unique: true,
                filter: "[ExternalFolio] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DELIVERYORDER_ExternalFolio",
                table: "DELIVERYORDER");

            migrationBuilder.DropColumn(
                name: "ExternalFolio",
                table: "DELIVERYORDER");
        }
    }
}
