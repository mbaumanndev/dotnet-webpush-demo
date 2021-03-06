﻿// <auto-generated />
using System;
using MBaumann.WebPush.WebUi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MBaumann.WebPush.WebUi.Migrations
{
    [DbContext(typeof(WebPushDbContext))]
    [Migration("20180925071229_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-preview1-35029");

            modelBuilder.Entity("MBaumann.WebPush.WebUi.Entities.Device", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Auth");

                    b.Property<string>("Endpoint");

                    b.Property<string>("P256DH");

                    b.HasKey("Id");

                    b.ToTable("Devices");
                });
#pragma warning restore 612, 618
        }
    }
}
