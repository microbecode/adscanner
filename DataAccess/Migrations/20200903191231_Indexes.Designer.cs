﻿// <auto-generated />
using System;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DataAccess.Migrations
{
    [DbContext(typeof(ScannerContext))]
    [Migration("20200903191231_Indexes")]
    partial class Indexes
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DataAccess.Models.Ad", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("Area")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("AreaStr")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Commodities")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("FirstSeen")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Floors")
                        .HasColumnType("int");

                    b.Property<DateTime?>("NotSeenAnymore")
                        .HasColumnType("datetime2");

                    b.Property<decimal?>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("PriceStr")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("PriceType")
                        .HasColumnType("int");

                    b.Property<string>("Region")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Rooms")
                        .HasColumnType("int");

                    b.Property<string>("SiteId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("SiteUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("Size")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("SizeStr")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Village")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("NotSeenAnymore");

                    b.HasIndex("PriceStr");

                    b.HasIndex("SiteId");

                    b.ToTable("Ads");
                });
#pragma warning restore 612, 618
        }
    }
}
