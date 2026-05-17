using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchLearn.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                schema: "public",
                table: "Enrollments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompletedLessonsCount",
                schema: "public",
                table: "Enrollments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastAccessedLessonId",
                schema: "public",
                table: "Enrollments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgressPercent",
                schema: "public",
                table: "Enrollments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLessonsCount",
                schema: "public",
                table: "Enrollments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                schema: "public",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "CompletedLessonsCount",
                schema: "public",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "LastAccessedLessonId",
                schema: "public",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "ProgressPercent",
                schema: "public",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "TotalLessonsCount",
                schema: "public",
                table: "Enrollments");
        }
    }
}
