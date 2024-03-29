﻿using System.Data.Entity;
using DayEasy.EntityFramework;
using DayEasy.Web.Wiki.Models;

namespace DayEasy.Web.Wiki.Contracts.Services
{
    public class WikiDbContext : CodeFirstDbContext
    {
        public WikiDbContext()
            : base("default")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("main");
            Database.SetInitializer<WikiDbContext>(null);
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<WikiGroup> Groups { get; set; }
        public virtual DbSet<Models.Wiki> Wikis { get; set; }
        public virtual DbSet<WikiVersion> Versions { get; set; }
        public virtual DbSet<WikiDetail> Details { get; set; }
    }
}