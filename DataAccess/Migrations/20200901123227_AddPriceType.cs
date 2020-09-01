using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccess.Migrations
{
    public partial class AddPriceType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ads",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteId = table.Column<string>(nullable: true),
                    SiteUrl = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    Region = table.Column<string>(nullable: true),
                    Village = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Size = table.Column<decimal>(nullable: false),
                    Floors = table.Column<int>(nullable: false),
                    Rooms = table.Column<int>(nullable: false),
                    Area = table.Column<decimal>(nullable: false),
                    Commodities = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    PriceStr = table.Column<string>(nullable: true),
                    Price = table.Column<decimal>(nullable: false),
                    PriceType = table.Column<int>(nullable: false),
                    FirstSeen = table.Column<DateTime>(nullable: false),
                    NotSeenAnymore = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ads", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ads");
        }
    }
}
