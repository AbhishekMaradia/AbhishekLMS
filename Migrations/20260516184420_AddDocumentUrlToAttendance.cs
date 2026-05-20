using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_SoulCode.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentUrlToAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentUrl",
                table: "Attendance",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentUrl",
                table: "Attendance");
        }
    }
}
