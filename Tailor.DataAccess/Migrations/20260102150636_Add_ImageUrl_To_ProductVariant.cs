using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tailor.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Add_ImageUrl_To_ProductVariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProductVariant",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ProductVariant");
        }
    }
}
