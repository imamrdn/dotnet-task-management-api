using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveTaskIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tasks_UserId_Id",
                table: "tasks");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_UserId_IsDeleted_Id",
                table: "tasks",
                columns: new[] { "UserId", "IsDeleted", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tasks_UserId_IsDeleted_Id",
                table: "tasks");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_UserId_Id",
                table: "tasks",
                columns: new[] { "UserId", "Id" });
        }
    }
}
