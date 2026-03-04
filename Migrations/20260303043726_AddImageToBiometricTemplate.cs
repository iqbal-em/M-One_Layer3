using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace M_One_Layer3.Migrations
{
    public partial class AddImageToBiometricTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "BiometricTemplates",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "BiometricTemplates");
        }
    }
}
