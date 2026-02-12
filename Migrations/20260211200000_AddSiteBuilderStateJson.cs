using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiteBuilder.Migrations
{
    public partial class AddSiteBuilderStateJson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuilderStateJson",
                table: "Sites",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuilderStateJson",
                table: "Sites");
        }
    }
}
