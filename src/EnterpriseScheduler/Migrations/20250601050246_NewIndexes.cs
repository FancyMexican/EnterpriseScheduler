using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseScheduler.Migrations
{
    /// <inheritdoc />
    public partial class NewIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Meetings_EndTime",
                table: "Meetings",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_StartTime",
                table: "Meetings",
                column: "StartTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Meetings_EndTime",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_StartTime",
                table: "Meetings");
        }
    }
}
