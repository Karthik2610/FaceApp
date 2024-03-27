using FaceApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FaceApp.Data
{
    public class FaceAppDBContext: DbContext
    {
        public FaceAppDBContext(DbContextOptions<FaceAppDBContext> options)
           : base(options)
        {
        }

        public FaceAppDBContext()
        {
        }

        public DbSet<PersonDetails> PersonDetails { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Persons> Persons { get; set; }
        public DbSet<PersonTrainDetails> PersonTrainDetails { get; set; }
        public DbSet<Attendance> Attendance { get; set; }

        public DbSet<CalendarSetting> CalendarSetting { get; set; }
        public DbSet<Programs> Programs { get; set; }
        public DbSet<ProgramCalendarDetails> ProgramCalendarDetails { get; set; }

        public DbSet<Locations> Locations { get; set; }
        public DbSet<LocationProgramDetails> LocationProgramDetails { get; set; }
        public DbSet<UserLocationDetails> UserLocationDetails { get; set; }
        public DbSet<PersonsDetails> PersonsDetails { get; set; }
    }
}
