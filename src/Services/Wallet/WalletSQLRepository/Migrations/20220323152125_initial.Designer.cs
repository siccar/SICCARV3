﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WalletService.SQLRepository;

#nullable disable

namespace WalletService.SQLRepository.Migrations
{
    [DbContext(typeof(WalletContext))]
    [Migration("20220323152125_initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Siccar.Platform.TransactionMetaData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("ActionId")
                        .HasColumnType("int");

                    b.Property<string>("BlueprintId")
                        .HasColumnType("longtext");

                    b.Property<string>("InstanceId")
                        .HasColumnType("longtext");

                    b.Property<int>("NextActionId")
                        .HasColumnType("int");

                    b.Property<string>("RegisterId")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("TransactionType")
                        .HasColumnType("int");

                    b.Property<string>("_trackingDataJson")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("TransactionMetaData");
                });

            modelBuilder.Entity("Siccar.Platform.WalletAddress", b =>
                {
                    b.Property<string>("Address")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("DerivationPath")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("WalletId")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("Address");

                    b.HasIndex("WalletId");

                    b.ToTable("Addresses");
                });

            modelBuilder.Entity("WalletService.Core.Models.Wallet", b =>
                {
                    b.Property<string>("Address")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("varchar(120)");

                    b.Property<string>("Owner")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("varchar(40)");

                    b.Property<string>("PrivateKey")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Tenant")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("varchar(40)");

                    b.HasKey("Address");

                    b.HasIndex("Owner");

                    b.ToTable("Wallets");
                });

            modelBuilder.Entity("WalletService.Core.Models.WalletAccess", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AccessType")
                        .HasColumnType("int");

                    b.Property<DateTime>("AssignedTime")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Tenant")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("WalletId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("Subject");

                    b.HasIndex("WalletId");

                    b.ToTable("Delegates");
                });

            modelBuilder.Entity("WalletService.Core.Models.WalletTransaction", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<int>("MetaDataId")
                        .HasColumnType("int");

                    b.Property<string>("PreviousId")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ReceivedAddress")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("Sender")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.Property<string>("TransactionId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("WalletId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<bool>("isConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("isSendingWallet")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("isSpent")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.HasIndex("MetaDataId");

                    b.HasIndex("WalletId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("Siccar.Platform.WalletAddress", b =>
                {
                    b.HasOne("WalletService.Core.Models.Wallet", null)
                        .WithMany("Addresses")
                        .HasForeignKey("WalletId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WalletService.Core.Models.WalletAccess", b =>
                {
                    b.HasOne("WalletService.Core.Models.Wallet", null)
                        .WithMany("Delegates")
                        .HasForeignKey("WalletId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WalletService.Core.Models.WalletTransaction", b =>
                {
                    b.HasOne("Siccar.Platform.TransactionMetaData", "MetaData")
                        .WithMany()
                        .HasForeignKey("MetaDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WalletService.Core.Models.Wallet", null)
                        .WithMany("Transactions")
                        .HasForeignKey("WalletId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MetaData");
                });

            modelBuilder.Entity("WalletService.Core.Models.Wallet", b =>
                {
                    b.Navigation("Addresses");

                    b.Navigation("Delegates");

                    b.Navigation("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
