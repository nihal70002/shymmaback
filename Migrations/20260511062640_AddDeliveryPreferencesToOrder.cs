using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientEcommerce.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryPreferencesToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryInstructions",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreferredDeliveryDate",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredDeliveryTime",
                table: "Orders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryInstructions",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PreferredDeliveryDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PreferredDeliveryTime",
                table: "Orders");
        }
    }
}
