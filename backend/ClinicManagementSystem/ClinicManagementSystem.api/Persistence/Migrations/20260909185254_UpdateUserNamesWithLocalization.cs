using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagementSystem.api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserNamesWithLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename existing columns to _EN versions
            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "AspNetUsers",
                newName: "FirstName_EN");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "AspNetUsers",
                newName: "LastName_EN");

            // Add new _AR columns
            migrationBuilder.AddColumn<string>(
                name: "FirstName_AR",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName_AR",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove _AR columns
            migrationBuilder.DropColumn(
                name: "FirstName_AR",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName_AR",
                table: "AspNetUsers");

            // Rename _EN columns back to original names
            migrationBuilder.RenameColumn(
                name: "FirstName_EN",
                table: "AspNetUsers",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "LastName_EN",
                table: "AspNetUsers",
                newName: "LastName");
        }
    }
}
