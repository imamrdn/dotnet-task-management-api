using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskOwnerSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerEmailSnapshot",
                table: "tasks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerNameSnapshot",
                table: "tasks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE tasks AS task
                SET
                    "OwnerNameSnapshot" = "user"."Name",
                    "OwnerEmailSnapshot" = "user"."Email"
                FROM users AS "user"
                WHERE task."UserId" = "user"."Id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerEmailSnapshot",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "OwnerNameSnapshot",
                table: "tasks");
        }
    }
}
