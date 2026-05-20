using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_SoulCode.Migrations
{
    /// <inheritdoc />
    public partial class AddThumbUrlToAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbUrl",
                table: "Attendance",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbUrl",
                table: "Attendance");
        }
    }
}
