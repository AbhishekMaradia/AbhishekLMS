using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_SoulCode.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToCourseDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Modules");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CourseDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "CourseDocuments");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Permissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Modules",
                type: "int",
                nullable: true);
        }
    }
}
