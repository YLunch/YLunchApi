﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using YnovEat.Infrastructure.Database;

namespace YnovEat.Api.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.6");

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("RoleId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Value")
                        .HasColumnType("longtext");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("RestaurantRestaurantCategory", b =>
                {
                    b.Property<string>("CategoriesId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("RestaurantsId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("CategoriesId", "RestaurantsId");

                    b.HasIndex("RestaurantsId");

                    b.ToTable("RestaurantRestaurantCategory");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.CustomerAggregate.Cart", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime?>("FulfilmentDatetime")
                        .HasColumnType("datetime(6)");

                    b.HasKey("UserId");

                    b.ToTable("Carts");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.CustomerAggregate.Customer", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("CustomerFamily")
                        .HasColumnType("int");

                    b.HasKey("UserId");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.CustomerAggregate.CustomerProduct", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("CartUserId")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("OrderId")
                        .HasColumnType("varchar(255)");

                    b.Property<double>("Price")
                        .HasColumnType("double");

                    b.Property<string>("RestaurantId")
                        .HasColumnType("longtext");

                    b.Property<string>("RestaurantProductId")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("CartUserId");

                    b.HasIndex("OrderId");

                    b.ToTable("CustomerProducts");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.ClosingDate", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("ClosingDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("RestaurantId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("RestaurantId");

                    b.ToTable("ClosingDates");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.DayOpeningTimes", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("DayOfWeek")
                        .HasColumnType("int");

                    b.Property<string>("RestaurantId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("RestaurantId");

                    b.ToTable("DaysOpeningTimes");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.OpeningTime", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("DayOpeningTimesId")
                        .HasColumnType("varchar(255)");

                    b.Property<int?>("EndOrderTimeInMinutes")
                        .HasColumnType("int");

                    b.Property<int>("EndTimeInMinutes")
                        .HasColumnType("int");

                    b.Property<int?>("StartOrderTimeInMinutes")
                        .HasColumnType("int");

                    b.Property<int>("StartTimeInMinutes")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DayOpeningTimesId");

                    b.ToTable("OpeningTimes");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.Order", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime?>("AcceptationDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("CustomerComment")
                        .HasColumnType("longtext");

                    b.Property<string>("CustomerId")
                        .HasColumnType("varchar(255)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("RestaurantComment")
                        .HasColumnType("longtext");

                    b.Property<string>("RestaurantId")
                        .HasColumnType("varchar(255)");

                    b.Property<double>("TotalPrice")
                        .HasColumnType("double");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("RestaurantId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.OrderStatus", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("OrderId")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.ToTable("OrderStatus");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.Restaurant", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("AddressExtraInformation")
                        .HasColumnType("longtext");

                    b.Property<string>("Base64Image")
                        .HasColumnType("longtext");

                    b.Property<string>("Base64Logo")
                        .HasColumnType("longtext");

                    b.Property<string>("City")
                        .HasColumnType("longtext");

                    b.Property<string>("Country")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("EmailConfirmationDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsEmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsOpen")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsPublished")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<string>("OwnerId")
                        .HasColumnType("longtext");

                    b.Property<string>("OwnerUserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<string>("StreetName")
                        .HasColumnType("longtext");

                    b.Property<string>("StreetNumber")
                        .HasColumnType("longtext");

                    b.Property<string>("ZipCode")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("OwnerUserId");

                    b.ToTable("Restaurants");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantCategory", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("RestaurantCategories");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantProduct", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("ExpirationDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Image")
                        .HasColumnType("longtext");

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<double>("Price")
                        .HasColumnType("double");

                    b.Property<int>("ProductFamily")
                        .HasColumnType("int");

                    b.Property<int?>("Quantity")
                        .HasColumnType("int");

                    b.Property<string>("RestaurantId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("RestaurantId");

                    b.ToTable("RestaurantProducts");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantProductTag", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("RestaurantProductId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("RestaurantProductId");

                    b.ToTable("RestaurantProductTags");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantUser", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("RestaurantId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("UserId");

                    b.HasIndex("RestaurantId");

                    b.ToTable("RestaurantUsers");

                    b.HasDiscriminator<string>("Discriminator").HasValue("RestaurantUser");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.UserAggregate.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("ConfirmationDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<DateTime?>("EmailConfirmationDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("IsAccountActivated")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("PhoneNumberConfirmationDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("longtext");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantAdmin", b =>
                {
                    b.HasBaseType("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantUser");

                    b.HasDiscriminator().HasValue("RestaurantAdmin");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantEmployee", b =>
                {
                    b.HasBaseType("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantUser");

                    b.HasDiscriminator().HasValue("RestaurantEmployee");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantOwner", b =>
                {
                    b.HasBaseType("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantUser");

                    b.HasDiscriminator().HasValue("RestaurantOwner");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.UserAggregate.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.UserAggregate.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("YnovEat.Domain.ModelsAggregate.UserAggregate.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.UserAggregate.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RestaurantRestaurantCategory", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantCategory", null)
                        .WithMany()
                        .HasForeignKey("CategoriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.Restaurant", null)
                        .WithMany()
                        .HasForeignKey("RestaurantsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.CustomerAggregate.Cart", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.CustomerAggregate.Customer", "Customer")
                        .WithOne("Cart")
                        .HasForeignKey("YnovEat.Domain.ModelsAggregate.CustomerAggregate.Cart", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.CustomerAggregate.Customer", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.UserAggregate.User", "User")
                        .WithOne("Customer")
                        .HasForeignKey("YnovEat.Domain.ModelsAggregate.CustomerAggregate.Customer", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.CustomerAggregate.CustomerProduct", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.CustomerAggregate.Cart", null)
                        .WithMany("Products")
                        .HasForeignKey("CartUserId");

                    b.HasOne("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.Order", "Order")
                        .WithMany("CustomerProducts")
                        .HasForeignKey("OrderId");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.ClosingDate", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.Restaurant", "Restaurant")
                        .WithMany("ClosingDates")
                        .HasForeignKey("RestaurantId");

                    b.Navigation("Restaurant");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.DayOpeningTimes", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.Restaurant", "Restaurant")
                        .WithMany("WeekOpeningTimes")
                        .HasForeignKey("RestaurantId");

                    b.Navigation("Restaurant");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.OpeningTime", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.DayOpeningTimes", "DayOpeningTimes")
                        .WithMany("OpeningTimes")
                        .HasForeignKey("DayOpeningTimesId")
                        .OnDelete(DeleteBehavior.ClientCascade);

                    b.Navigation("DayOpeningTimes");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.Order", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.CustomerAggregate.Customer", "Customer")
                        .WithMany("Orders")
                        .HasForeignKey("CustomerId");

                    b.HasOne("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.Restaurant", "Restaurant")
                        .WithMany("Orders")
                        .HasForeignKey("RestaurantId");

                    b.Navigation("Customer");

                    b.Navigation("Restaurant");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.OrderStatus", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.Order", "Order")
                        .WithMany("OrderStatuses")
                        .HasForeignKey("OrderId");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.Restaurant", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantOwner", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerUserId");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantProduct", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.Restaurant", "Restaurant")
                        .WithMany("RestaurantProducts")
                        .HasForeignKey("RestaurantId");

                    b.Navigation("Restaurant");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantProductTag", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantProduct", null)
                        .WithMany("RestaurantProductTags")
                        .HasForeignKey("RestaurantProductId");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantUser", b =>
                {
                    b.HasOne("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.Restaurant", "Restaurant")
                        .WithMany("RestaurantUsers")
                        .HasForeignKey("RestaurantId");

                    b.HasOne("YnovEat.Domain.ModelsAggregate.UserAggregate.User", "User")
                        .WithOne("RestaurantUser")
                        .HasForeignKey("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantUser", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Restaurant");

                    b.Navigation("User");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.CustomerAggregate.Cart", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.CustomerAggregate.Customer", b =>
                {
                    b.Navigation("Cart");

                    b.Navigation("Orders");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.DayOpeningTimes", b =>
                {
                    b.Navigation("OpeningTimes");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.Order", b =>
                {
                    b.Navigation("CustomerProducts");

                    b.Navigation("OrderStatuses");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.Restaurant", b =>
                {
                    b.Navigation("ClosingDates");

                    b.Navigation("Orders");

                    b.Navigation("RestaurantProducts");

                    b.Navigation("RestaurantUsers");

                    b.Navigation("WeekOpeningTimes");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.RestaurantAggregate.RestaurantProduct", b =>
                {
                    b.Navigation("RestaurantProductTags");
                });

            modelBuilder.Entity("YnovEat.Domain.ModelsAggregate.UserAggregate.User", b =>
                {
                    b.Navigation("Customer");

                    b.Navigation("RestaurantUser");
                });
#pragma warning restore 612, 618
        }
    }
}
