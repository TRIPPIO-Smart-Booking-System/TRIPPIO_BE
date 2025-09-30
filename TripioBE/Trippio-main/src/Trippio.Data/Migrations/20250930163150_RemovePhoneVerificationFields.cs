using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trippio.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovePhoneVerificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPhoneVerified",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PhoneOtp",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PhoneOtpExpiry",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPhoneVerified",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhoneOtp",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PhoneOtpExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }
    }
}
