using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccess.Migrations
{
    public partial class AddStringValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AreaStr",
                table: "Ads",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SizeStr",
                table: "Ads",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaStr",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "SizeStr",
                table: "Ads");
        }
    }
}
