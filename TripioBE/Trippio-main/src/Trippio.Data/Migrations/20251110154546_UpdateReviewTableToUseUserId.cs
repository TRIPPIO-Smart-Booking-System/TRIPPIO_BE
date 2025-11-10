using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trippio.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReviewTableToUseUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Customers_CustomerId",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Reviews",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_OrderId_CustomerId",
                table: "Reviews",
                newName: "IX_Reviews_OrderId_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_CustomerId",
                table: "Reviews",
                newName: "IX_Reviews_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_UserId",
                table: "Reviews",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_UserId",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Reviews",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                newName: "IX_Reviews_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_OrderId_UserId",
                table: "Reviews",
                newName: "IX_Reviews_OrderId_CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Customers_CustomerId",
                table: "Reviews",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
