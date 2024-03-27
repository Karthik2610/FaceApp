using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FaceApp.Migrations
{
    public partial class AddFoeignKeyReferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProgramCalendarDetails_CalendarId",
                table: "ProgramCalendarDetails",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramCalendarDetails_ProgramId",
                table: "ProgramCalendarDetails",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationProgramDetails_LocationId",
                table: "LocationProgramDetails",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationProgramDetails_ProgramId",
                table: "LocationProgramDetails",
                column: "ProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationProgramDetails_Locations_LocationId",
                table: "LocationProgramDetails",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationProgramDetails_Programs_ProgramId",
                table: "LocationProgramDetails",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "ProgramId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramCalendarDetails_CalendarSetting_CalendarId",
                table: "ProgramCalendarDetails",
                column: "CalendarId",
                principalTable: "CalendarSetting",
                principalColumn: "CalendarId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramCalendarDetails_Programs_ProgramId",
                table: "ProgramCalendarDetails",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "ProgramId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationProgramDetails_Locations_LocationId",
                table: "LocationProgramDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationProgramDetails_Programs_ProgramId",
                table: "LocationProgramDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramCalendarDetails_CalendarSetting_CalendarId",
                table: "ProgramCalendarDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramCalendarDetails_Programs_ProgramId",
                table: "ProgramCalendarDetails");

            migrationBuilder.DropIndex(
                name: "IX_ProgramCalendarDetails_CalendarId",
                table: "ProgramCalendarDetails");

            migrationBuilder.DropIndex(
                name: "IX_ProgramCalendarDetails_ProgramId",
                table: "ProgramCalendarDetails");

            migrationBuilder.DropIndex(
                name: "IX_LocationProgramDetails_LocationId",
                table: "LocationProgramDetails");

            migrationBuilder.DropIndex(
                name: "IX_LocationProgramDetails_ProgramId",
                table: "LocationProgramDetails");
        }
    }
}
