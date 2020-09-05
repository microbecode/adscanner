using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccess.Migrations
{
    public partial class AddLands : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lands",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteId = table.Column<string>(nullable: true),
                    SiteUrl = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    Region = table.Column<string>(nullable: true),
                    Village = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    SizeStr = table.Column<string>(nullable: true),
                    Size = table.Column<decimal>(nullable: true),
                    Usage = table.Column<string>(nullable: true),
                    LandNumber = table.Column<string>(nullable: true),
                    PriceStr = table.Column<string>(nullable: true),
                    Price = table.Column<decimal>(nullable: true),
                    PriceType = table.Column<int>(nullable: true),
                    FirstSeen = table.Column<DateTime>(nullable: true),
                    NotSeenAnymore = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lands", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lands_NotSeenAnymore",
                table: "Lands",
                column: "NotSeenAnymore");

            migrationBuilder.CreateIndex(
                name: "IX_Lands_PriceStr",
                table: "Lands",
                column: "PriceStr");

            migrationBuilder.CreateIndex(
                name: "IX_Lands_SiteId",
                table: "Lands",
                column: "SiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lands");
        }
    }
}
