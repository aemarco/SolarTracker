﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SolarTracker.Database;

#nullable disable

namespace SolarTracker.Migrations
{
    [DbContext(typeof(SolarContext))]
    [Migration("20230423115314_SunInfoAddedSecondsOfDay")]
    partial class SunInfoAddedSecondsOfDay
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.15");

            modelBuilder.Entity("SolarTracker.Models.Db.KeyValueInfo", b =>
                {
                    b.Property<string>("Key")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.ToTable("KeyValueInfos", (string)null);
                });

            modelBuilder.Entity("SolarTracker.Models.Db.SunInfo", b =>
                {
                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.Property<float>("Latitude")
                        .HasColumnType("REAL");

                    b.Property<float>("Longitude")
                        .HasColumnType("REAL");

                    b.Property<float>("Altitude")
                        .HasColumnType("REAL");

                    b.Property<float>("Azimuth")
                        .HasColumnType("REAL");

                    b.Property<double>("SecondsOfDay")
                        .HasColumnType("REAL");

                    b.Property<TimeOnly>("Sunrise")
                        .HasColumnType("TEXT");

                    b.Property<TimeOnly>("Sunset")
                        .HasColumnType("TEXT");

                    b.HasKey("Timestamp", "Latitude", "Longitude");

                    b.ToTable("SolarInfos", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
