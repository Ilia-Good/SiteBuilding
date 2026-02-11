using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiteBuilder.Migrations
{
    public partial class AddUserFormEndpoint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FormEndpoint",
                table: "Users",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormEndpoint",
                table: "Users");
        }
    }
}
