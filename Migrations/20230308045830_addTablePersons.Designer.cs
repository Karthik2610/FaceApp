﻿// <auto-generated />
using FaceApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace FaceApp.Migrations
{
    [DbContext(typeof(FaceAppDBContext))]
    [Migration("20230308045830_addTablePersons")]
    partial class addTablePersons
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("FaceApp.Models.PersonDetails", b =>
                {
                    b.Property<Guid>("PersonId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("PersonName");

                    b.HasKey("PersonId");

                    b.ToTable("PersonDetails");
                });

            modelBuilder.Entity("FaceApp.Models.Persons", b =>
                {
                    b.Property<int>("PersonId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address1");

                    b.Property<string>("Address2");

                    b.Property<string>("City");

                    b.Property<int>("CratedBy");

                    b.Property<DateTime>("CratedDate");

                    b.Property<DateTime?>("DOB");

                    b.Property<string>("FirstName");

                    b.Property<bool>("IsActive");

                    b.Property<string>("LastName");

                    b.Property<string>("MiddleInitial");

                    b.Property<int?>("ModifiedBy");

                    b.Property<DateTime?>("ModifiedDate");

                    b.Property<string>("PersonId1");

                    b.Property<string>("PersonId2");

                    b.Property<string>("PersonId3");

                    b.Property<string>("State");

                    b.Property<string>("Zip");

                    b.HasKey("PersonId");

                    b.ToTable("Persons");
                });

            modelBuilder.Entity("FaceApp.Models.Users", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CratedBy");

                    b.Property<DateTime>("CratedDate");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsAdmin");

                    b.Property<bool>("IsForgotPassword");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("MiddleInitial")
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("ModifiedBy");

                    b.Property<DateTime?>("ModifiedDate");

                    b.Property<string>("Password");

                    b.Property<string>("Salt");

                    b.Property<string>("SecurityGroup")
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("UserEmpId");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
