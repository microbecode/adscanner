using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccess.Migrations
{
    public partial class Indexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SiteId",
                table: "Ads",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PriceStr",
                table: "Ads",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ads_NotSeenAnymore",
                table: "Ads",
                column: "NotSeenAnymore");

            migrationBuilder.CreateIndex(
                name: "IX_Ads_PriceStr",
                table: "Ads",
                column: "PriceStr");

            migrationBuilder.CreateIndex(
                name: "IX_Ads_SiteId",
                table: "Ads",
                column: "SiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ads_NotSeenAnymore",
                table: "Ads");

            migrationBuilder.DropIndex(
                name: "IX_Ads_PriceStr",
                table: "Ads");

            migrationBuilder.DropIndex(
                name: "IX_Ads_SiteId",
                table: "Ads");

            migrationBuilder.AlterColumn<string>(
                name: "SiteId",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PriceStr",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
