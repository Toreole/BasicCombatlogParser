﻿// <auto-generated />
using CombatlogParser.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CombatlogParser.Migrations
{
    [DbContext(typeof(CombatlogDBContext))]
    [Migration("20230326172016_AddItemLevelSupport")]
    partial class AddItemLevelSupport
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.3");

            modelBuilder.Entity("CombatlogParser.Data.Metadata.CombatlogMetadata", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("BuildVersion")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsAdvanced")
                        .HasColumnType("INTEGER");

                    b.Property<long>("MsTimeStamp")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProjectID")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("CombatlogMetadatas");
                });

            modelBuilder.Entity("CombatlogParser.Data.Metadata.EncounterInfoMetadata", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<uint>("CombatlogMetadataId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DifficultyId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("EncounterDurationMS")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EncounterLengthInFile")
                        .HasColumnType("INTEGER");

                    b.Property<long>("EncounterStartIndex")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Success")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WowEncounterId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CombatlogMetadataId");

                    b.ToTable("Encounters");
                });

            modelBuilder.Entity("CombatlogParser.Data.Metadata.PerformanceMetadata", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("Dps")
                        .HasColumnType("REAL");

                    b.Property<uint>("EncounterInfoMetadataId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Hps")
                        .HasColumnType("REAL");

                    b.Property<int>("ItemLevel")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("PlayerMetadataId")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("RoleId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SpecId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WowEncounterId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("EncounterInfoMetadataId");

                    b.HasIndex("PlayerMetadataId");

                    b.ToTable("Performances");
                });

            modelBuilder.Entity("CombatlogParser.Data.Metadata.PlayerMetadata", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClassId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("GUID")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Realm")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("CombatlogParser.Data.Metadata.EncounterInfoMetadata", b =>
                {
                    b.HasOne("CombatlogParser.Data.Metadata.CombatlogMetadata", "CombatlogMetadata")
                        .WithMany("Encounters")
                        .HasForeignKey("CombatlogMetadataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CombatlogMetadata");
                });

            modelBuilder.Entity("CombatlogParser.Data.Metadata.PerformanceMetadata", b =>
                {
                    b.HasOne("CombatlogParser.Data.Metadata.EncounterInfoMetadata", "EncounterInfoMetadata")
                        .WithMany()
                        .HasForeignKey("EncounterInfoMetadataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CombatlogParser.Data.Metadata.PlayerMetadata", "PlayerMetadata")
                        .WithMany()
                        .HasForeignKey("PlayerMetadataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EncounterInfoMetadata");

                    b.Navigation("PlayerMetadata");
                });

            modelBuilder.Entity("CombatlogParser.Data.Metadata.CombatlogMetadata", b =>
                {
                    b.Navigation("Encounters");
                });
#pragma warning restore 612, 618
        }
    }
}
