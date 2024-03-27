using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FaceApp.Migrations
{
    public partial class createAdminUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
               INSERT INTO Users(Email,IsAdmin,FirstName,LastName,Password,Salt,CratedDate,CratedBy) values('admin@faceapp.com',1,'FaceApp','admin','VUdhZn5kYlU9FL/SNFpyMD5J7q1wDsPGZH+I2t1p6bs=','EY$YbDkD',GETDATE(),1);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
