using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchLearn.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameDurationSecondsToMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DurationSeconds",
                schema: "public",
                table: "Lessons",
                newName: "DurationMinutes");

            // Convert existing seconds to minutes (round down)
            migrationBuilder.Sql(
                "UPDATE public.\"Lessons\" SET \"DurationMinutes\" = \"DurationMinutes\" / 60");

            // Make the column nullable
            migrationBuilder.AlterColumn<int>(
                name: "DurationMinutes",
                schema: "public",
                table: "Lessons",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: false);

            // Set rows that were 0 to NULL (no duration specified)
            migrationBuilder.Sql(
                "UPDATE public.\"Lessons\" SET \"DurationMinutes\" = NULL WHERE \"DurationMinutes\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DurationMinutes",
                schema: "public",
                table: "Lessons",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            // Convert minutes back to seconds
            migrationBuilder.Sql(
                "UPDATE public.\"Lessons\" SET \"DurationMinutes\" = \"DurationMinutes\" * 60");

            migrationBuilder.RenameColumn(
                name: "DurationMinutes",
                schema: "public",
                table: "Lessons",
                newName: "DurationSeconds");
        }
    }
}
