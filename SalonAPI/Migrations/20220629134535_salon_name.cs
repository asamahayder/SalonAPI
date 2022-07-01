using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalonAPI.Migrations
{
    public partial class salon_name : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalonChainName",
                table: "Owners");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Salons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Salons");

            migrationBuilder.AddColumn<string>(
                name: "SalonChainName",
                table: "Owners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
