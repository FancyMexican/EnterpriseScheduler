using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseScheduler.Migrations
{
    /// <inheritdoc />
    public partial class TableNamesUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingParticipants_Meetings_MeetingsId",
                table: "MeetingParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingParticipants_Users_ParticipantsId",
                table: "MeetingParticipants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MeetingParticipants",
                table: "MeetingParticipants");

            migrationBuilder.RenameTable(
                name: "MeetingParticipants",
                newName: "MeetingUser");

            migrationBuilder.RenameIndex(
                name: "IX_MeetingParticipants_ParticipantsId",
                table: "MeetingUser",
                newName: "IX_MeetingUser_ParticipantsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MeetingUser",
                table: "MeetingUser",
                columns: new[] { "MeetingsId", "ParticipantsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingUser_Meetings_MeetingsId",
                table: "MeetingUser",
                column: "MeetingsId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingUser_Users_ParticipantsId",
                table: "MeetingUser",
                column: "ParticipantsId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingUser_Meetings_MeetingsId",
                table: "MeetingUser");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingUser_Users_ParticipantsId",
                table: "MeetingUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MeetingUser",
                table: "MeetingUser");

            migrationBuilder.RenameTable(
                name: "MeetingUser",
                newName: "MeetingParticipants");

            migrationBuilder.RenameIndex(
                name: "IX_MeetingUser_ParticipantsId",
                table: "MeetingParticipants",
                newName: "IX_MeetingParticipants_ParticipantsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MeetingParticipants",
                table: "MeetingParticipants",
                columns: new[] { "MeetingsId", "ParticipantsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingParticipants_Meetings_MeetingsId",
                table: "MeetingParticipants",
                column: "MeetingsId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingParticipants_Users_ParticipantsId",
                table: "MeetingParticipants",
                column: "ParticipantsId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
