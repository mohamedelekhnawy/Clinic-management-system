using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagementSystem.api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToProfileBasedArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create Profiles table FIRST (before dropping any columns)
            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName_En = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName_Ar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName_En = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName_Ar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_Email",
                table: "Profiles",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_Phone",
                table: "Profiles",
                column: "Phone",
                unique: true);

            // Step 2: Add ProfileId columns to all tables (nullable initially)
            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "Doctor",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "Assistants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            // Step 3: DATA MIGRATION - Migrate existing Doctor records to Profiles
            migrationBuilder.Sql(@"
                INSERT INTO Profiles (FirstName_En, FirstName_Ar, LastName_En, LastName_Ar, Phone, Email, IsActive, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
                SELECT 
                    ISNULL(FirstName_En, ''), 
                    ISNULL(FirstName_Ar, ''), 
                    ISNULL(LastName_En, ''), 
                    ISNULL(LastName_Ar, ''), 
                    ISNULL(Phone, ''), 
                    ISNULL(Email, ''), 
                    1, 
                    CreatedOn, 
                    CreatedBy, 
                    UpdatedOn, 
                    UpdatedBy
                FROM Doctor;

                UPDATE Doctor
                SET ProfileId = (
                    SELECT p.Id 
                    FROM Profiles p
                    WHERE p.Email = Doctor.Email 
                    AND p.Phone = Doctor.Phone
                    AND p.FirstName_En = Doctor.FirstName_En
                    AND p.CreatedOn = Doctor.CreatedOn
                );
            ");

            // Step 4: DATA MIGRATION - Migrate existing Assistant records to Profiles
            migrationBuilder.Sql(@"
                INSERT INTO Profiles (FirstName_En, FirstName_Ar, LastName_En, LastName_Ar, Phone, Email, IsActive, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
                SELECT 
                    ISNULL(FirstName_En, ''), 
                    ISNULL(FirstName_Ar, ''), 
                    ISNULL(LastName_En, ''), 
                    ISNULL(LastName_Ar, ''), 
                    ISNULL(Phone, ''), 
                    ISNULL(Email, ''), 
                    IsActive, 
                    CreatedOn, 
                    CreatedBy, 
                    UpdatedOn, 
                    UpdatedBy
                FROM Assistants;

                UPDATE Assistants
                SET ProfileId = (
                    SELECT p.Id 
                    FROM Profiles p
                    WHERE p.Email = Assistants.Email 
                    AND p.Phone = Assistants.Phone
                    AND p.FirstName_En = Assistants.FirstName_En
                    AND p.CreatedOn = Assistants.CreatedOn
                );
            ");

            // Step 5: DATA MIGRATION - Migrate existing Patient records to Profiles
            migrationBuilder.Sql(@"
                INSERT INTO Profiles (FirstName_En, FirstName_Ar, LastName_En, LastName_Ar, Phone, Email, IsActive, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
                SELECT 
                    ISNULL(FirstName_En, ''), 
                    ISNULL(FirstName_Ar, ''), 
                    ISNULL(LastName_En, ''), 
                    ISNULL(LastName_Ar, ''), 
                    ISNULL(Phone, ''), 
                    ISNULL(Email, ''), 
                    IsActive, 
                    CreatedOn, 
                    CreatedBy, 
                    UpdatedOn, 
                    UpdatedBy
                FROM Patients;

                UPDATE Patients
                SET ProfileId = (
                    SELECT p.Id 
                    FROM Profiles p
                    WHERE p.Phone = Patients.Phone
                    AND p.FirstName_En = Patients.FirstName_En
                    AND p.CreatedOn = Patients.CreatedOn
                );
            ");

            // Step 6: Make ProfileId NOT NULL for role tables (now that data is migrated)
            migrationBuilder.AlterColumn<int>(
                name: "ProfileId",
                table: "Patients",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProfileId",
                table: "Doctor",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProfileId",
                table: "Assistants",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Step 7: Drop old indexes
            migrationBuilder.DropIndex(
                name: "IX_Patients_Email",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_Phone",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Assistants_Email",
                table: "Assistants");

            migrationBuilder.DropIndex(
                name: "IX_Assistants_Phone",
                table: "Assistants");

            // Step 8: Drop old columns from all tables
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "FirstName_Ar",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "FirstName_En",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "LastName_Ar",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "LastName_En",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "FirstName_Ar",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "FirstName_En",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "LastName_Ar",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "LastName_En",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Assistants");

            migrationBuilder.DropColumn(
                name: "FirstName_Ar",
                table: "Assistants");

            migrationBuilder.DropColumn(
                name: "FirstName_En",
                table: "Assistants");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Assistants");

            migrationBuilder.DropColumn(
                name: "LastName_Ar",
                table: "Assistants");

            migrationBuilder.DropColumn(
                name: "LastName_En",
                table: "Assistants");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Assistants");

            migrationBuilder.DropColumn(
                name: "FirstName_AR",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FirstName_EN",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName_AR",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName_EN",
                table: "AspNetUsers");

            // Step 9: Create indexes and foreign keys
            migrationBuilder.CreateIndex(
                name: "IX_Patients_ProfileId",
                table: "Patients",
                column: "ProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_ProfileId",
                table: "Doctor",
                column: "ProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assistants_ProfileId",
                table: "Assistants",
                column: "ProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ProfileId",
                table: "AspNetUsers",
                column: "ProfileId",
                unique: true,
                filter: "[ProfileId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Profiles_ProfileId",
                table: "AspNetUsers",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assistants_Profiles_ProfileId",
                table: "Assistants",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctor_Profiles_ProfileId",
                table: "Doctor",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Profiles_ProfileId",
                table: "Patients",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Profiles_ProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Assistants_Profiles_ProfileId",
                table: "Assistants");

            migrationBuilder.DropForeignKey(
                name: "FK_Doctor_Profiles_ProfileId",
                table: "Doctor");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Profiles_ProfileId",
                table: "Patients");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_Patients_ProfileId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Doctor_ProfileId",
                table: "Doctor");

            migrationBuilder.DropIndex(
                name: "IX_Assistants_ProfileId",
                table: "Assistants");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "Assistants");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Patients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName_Ar",
                table: "Patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName_En",
                table: "Patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName_Ar",
                table: "Patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName_En",
                table: "Patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Patients",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Doctor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName_Ar",
                table: "Doctor",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName_En",
                table: "Doctor",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName_Ar",
                table: "Doctor",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName_En",
                table: "Doctor",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Doctor",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Assistants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName_Ar",
                table: "Assistants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName_En",
                table: "Assistants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Assistants",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName_Ar",
                table: "Assistants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName_En",
                table: "Assistants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Assistants",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName_AR",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName_EN",
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
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName_EN",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Email",
                table: "Patients",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Phone",
                table: "Patients",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assistants_Email",
                table: "Assistants",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assistants_Phone",
                table: "Assistants",
                column: "Phone",
                unique: true);
        }
    }
}
