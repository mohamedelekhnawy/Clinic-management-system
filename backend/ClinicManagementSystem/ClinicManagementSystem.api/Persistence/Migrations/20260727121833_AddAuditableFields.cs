using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagementSystem.api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditableFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete all data from tables
            migrationBuilder.Sql("DELETE FROM Doctor");
            migrationBuilder.Sql("DELETE FROM Clinics");

            // Reset identity seeds
            migrationBuilder.Sql("DBCC CHECKIDENT ('Doctor', RESEED, 0)");
            migrationBuilder.Sql("DBCC CHECKIDENT ('Clinics', RESEED, 0)");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Clinics");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Doctor",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Clinics",
                newName: "CreatedOn");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Doctor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Doctor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Doctor",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Clinics",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Clinics");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Doctor",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Clinics",
                newName: "UpdatedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Doctor",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Clinics",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
