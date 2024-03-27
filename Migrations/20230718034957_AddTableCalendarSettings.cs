using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FaceApp.Migrations
{
    public partial class AddTableCalendarSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarSetting",
                columns: table => new
                {
                    CalendarId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CalendarName = table.Column<string>(nullable: true),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    FridayEndTime = table.Column<DateTime>(nullable: true),
                    FridayIsActive = table.Column<bool>(nullable: false),
                    FridayStartTime = table.Column<DateTime>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    MondayEndTime = table.Column<DateTime>(nullable: true),
                    MondayIsActive = table.Column<bool>(nullable: false),
                    MondayStartTime = table.Column<DateTime>(nullable: true),
                    SaturdayEndTime = table.Column<DateTime>(nullable: true),
                    SaturdayIsActive = table.Column<bool>(nullable: false),
                    SaturdayStartTime = table.Column<DateTime>(nullable: true),
                    SundayEndTime = table.Column<DateTime>(nullable: true),
                    SundayIsActive = table.Column<bool>(nullable: false),
                    SundayStartTime = table.Column<DateTime>(nullable: true),
                    ThursdayEndTime = table.Column<DateTime>(nullable: true),
                    ThursdayIsActive = table.Column<bool>(nullable: false),
                    ThursdayStartTime = table.Column<DateTime>(nullable: true),
                    TuesdayEndTime = table.Column<DateTime>(nullable: true),
                    TuesdayIsActive = table.Column<bool>(nullable: false),
                    TuesdayStartTime = table.Column<DateTime>(nullable: true),
                    WednesdayEndTime = table.Column<DateTime>(nullable: true),
                    WednesdayIsActive = table.Column<bool>(nullable: false),
                    WednesdayStartTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarSetting", x => x.CalendarId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarSetting");
        }
    }
}
