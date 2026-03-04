using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace M_One_Layer3.Migrations
{
    public partial class UpdateDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QualityScore",
                table: "BiometricTemplates");

            migrationBuilder.AlterColumn<byte[]>(
                name: "TemplateBase64",
                table: "BiometricTemplates",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "FingerIndex",
                table: "BiometricTemplates",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FingerIndex",
                table: "BiometricTemplates");

            migrationBuilder.AlterColumn<string>(
                name: "TemplateBase64",
                table: "BiometricTemplates",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AddColumn<int>(
                name: "QualityScore",
                table: "BiometricTemplates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
