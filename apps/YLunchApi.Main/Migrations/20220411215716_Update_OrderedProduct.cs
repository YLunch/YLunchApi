using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YLunchApi.Main.Migrations
{
    public partial class Update_OrderedProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDateTime",
                table: "OrderedProducts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductType",
                table: "OrderedProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpirationDateTime",
                table: "OrderedProducts");

            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "OrderedProducts");
        }
    }
}
