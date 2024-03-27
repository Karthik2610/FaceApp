using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FaceApp.Migrations
{
    public partial class addTablePersonsDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonsDetails",
                columns: table => new
                {
                    PersonsDetailsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CalendarId = table.Column<int>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    LocationId = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    PersonId = table.Column<int>(nullable: false),
                    ProgramId = table.Column<int>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonsDetails", x => x.PersonsDetailsId);
                    table.ForeignKey(
                        name: "FK_PersonsDetails_CalendarSetting_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "CalendarSetting",
                        principalColumn: "CalendarId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonsDetails_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonsDetails_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonsDetails_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "ProgramId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonsDetails_CalendarId",
                table: "PersonsDetails",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonsDetails_LocationId",
                table: "PersonsDetails",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonsDetails_PersonId",
                table: "PersonsDetails",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonsDetails_ProgramId",
                table: "PersonsDetails",
                column: "ProgramId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonsDetails");
        }
    }
}
