using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FaceApp.Migrations
{
    public partial class AddTraingingInfoPersonTrainDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "PersonTrainDetails",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoOfTrainings",
                table: "PersonTrainDetails",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "PersonTrainDetails");

            migrationBuilder.DropColumn(
                name: "NoOfTrainings",
                table: "PersonTrainDetails");
        }
    }
}
