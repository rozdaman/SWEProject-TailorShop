using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tailor.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Add_Source_Column_To_StockLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "StockLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "StockLogs");
        }
    }
}
