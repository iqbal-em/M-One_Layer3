using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace M_One_Layer3.Migrations
{
    public partial class AddWidthHeightToBiometricTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "BiometricTemplates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "BiometricTemplates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "BiometricTemplates");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "BiometricTemplates");
        }
    }
}
