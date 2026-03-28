using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientEcommerce.API.Migrations
{
    /// <inheritdoc />
    public partial class variantcreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Class",
                table: "ProductVariants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "ProductVariants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "ProductVariants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Style",
                table: "ProductVariants",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Class",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Style",
                table: "ProductVariants");
        }
    }
}
