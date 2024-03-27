using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FaceApp.Migrations
{
    public partial class UpdatetablePersons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CratedDate",
                table: "Persons",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "CratedBy",
                table: "Persons",
                newName: "CreatedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Persons",
                newName: "CratedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Persons",
                newName: "CratedBy");
        }
    }
}
