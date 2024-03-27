using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FaceApp.Migrations
{
    public partial class AltertableAttendance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttendanceType",
                table: "Attendance",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Attendance",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramId",
                table: "Attendance",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_LocationId",
                table: "Attendance",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_ProgramId",
                table: "Attendance",
                column: "ProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Locations_LocationId",
                table: "Attendance",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Programs_ProgramId",
                table: "Attendance",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "ProgramId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Locations_LocationId",
                table: "Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Programs_ProgramId",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_LocationId",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_ProgramId",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "AttendanceType",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "Attendance");
        }
    }
}
