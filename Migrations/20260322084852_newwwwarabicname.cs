using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientEcommerce.API.Migrations
{
    /// <inheritdoc />
    public partial class newwwwarabicname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameArabic",
                table: "Products",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameArabic",
                table: "Products");
        }
    }
}
