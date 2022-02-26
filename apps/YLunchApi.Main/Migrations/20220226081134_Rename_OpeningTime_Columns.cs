using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YLunchApi.Main.Migrations
{
    public partial class Rename_OpeningTime_Columns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpenMinutes",
                table: "PlaceOpeningTimes",
                newName: "OffsetInMinutes");

            migrationBuilder.RenameColumn(
                name: "OffsetOpenMinutes",
                table: "PlaceOpeningTimes",
                newName: "DurationInMinutes");

            migrationBuilder.RenameColumn(
                name: "OpenMinutes",
                table: "OrderOpeningTimes",
                newName: "OffsetInMinutes");

            migrationBuilder.RenameColumn(
                name: "OffsetOpenMinutes",
                table: "OrderOpeningTimes",
                newName: "DurationInMinutes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OffsetInMinutes",
                table: "PlaceOpeningTimes",
                newName: "OpenMinutes");

            migrationBuilder.RenameColumn(
                name: "DurationInMinutes",
                table: "PlaceOpeningTimes",
                newName: "OffsetOpenMinutes");

            migrationBuilder.RenameColumn(
                name: "OffsetInMinutes",
                table: "OrderOpeningTimes",
                newName: "OpenMinutes");

            migrationBuilder.RenameColumn(
                name: "DurationInMinutes",
                table: "OrderOpeningTimes",
                newName: "OffsetOpenMinutes");
        }
    }
}
