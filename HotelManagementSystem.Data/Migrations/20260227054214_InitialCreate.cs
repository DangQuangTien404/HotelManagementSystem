using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HotelServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoomNumber = table.Column<string>(type: "TEXT", nullable: false),
                    RoomType = table.Column<string>(type: "TEXT", nullable: false),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    IdentityNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoomId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedTo = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Deadline = table.Column<DateTime>(type: "datetime", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceTasks_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceTasks_Users_AssignedTo",
                        column: x => x.AssignedTo,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceTasks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderId = table.Column<int>(type: "INTEGER", nullable: true),
                    SenderName = table.Column<string>(type: "TEXT", nullable: false),
                    SenderType = table.Column<string>(type: "TEXT", nullable: false),
                    RecipientType = table.Column<string>(type: "TEXT", nullable: false),
                    RecipientId = table.Column<int>(type: "INTEGER", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    IsAnnouncement = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoomCleanings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoomId = table.Column<int>(type: "INTEGER", nullable: false),
                    CleanedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    CleaningDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomCleanings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomCleanings_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomCleanings_Users_CleanedBy",
                        column: x => x.CleanedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Staffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<string>(type: "TEXT", nullable: false),
                    Shift = table.Column<string>(type: "TEXT", nullable: false),
                    HireDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Staffs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    RoomId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReservedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    CheckInDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReservedByNavigationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_Users_ReservedByNavigationId",
                        column: x => x.ReservedByNavigationId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CheckInOuts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReservationId = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckInBy = table.Column<int>(type: "INTEGER", nullable: true),
                    CheckOutBy = table.Column<int>(type: "INTEGER", nullable: true),
                    CheckInTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CheckOutTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckInOuts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckInOuts_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CheckInOuts_Users_CheckInBy",
                        column: x => x.CheckInBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CheckInOuts_Users_CheckOutBy",
                        column: x => x.CheckOutBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReservationServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReservationId = table.Column<int>(type: "INTEGER", nullable: false),
                    HotelServiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AddedBy = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationServices_HotelServices_HotelServiceId",
                        column: x => x.HotelServiceId,
                        principalTable: "HotelServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReservationServices_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckInOuts_CheckInBy",
                table: "CheckInOuts",
                column: "CheckInBy");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInOuts_CheckOutBy",
                table: "CheckInOuts",
                column: "CheckOutBy");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInOuts_ReservationId",
                table: "CheckInOuts",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId",
                table: "Customers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_AssignedTo",
                table: "MaintenanceTasks",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_RoomId",
                table: "MaintenanceTasks",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_UserId",
                table: "MaintenanceTasks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SenderId",
                table: "Notifications",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CustomerId",
                table: "Reservations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservedByNavigationId",
                table: "Reservations",
                column: "ReservedByNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_RoomId",
                table: "Reservations",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationServices_HotelServiceId",
                table: "ReservationServices",
                column: "HotelServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationServices_ReservationId",
                table: "ReservationServices",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomCleanings_CleanedBy",
                table: "RoomCleanings",
                column: "CleanedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RoomCleanings_RoomId",
                table: "RoomCleanings",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_UserId",
                table: "Staffs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckInOuts");

            migrationBuilder.DropTable(
                name: "MaintenanceTasks");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ReservationServices");

            migrationBuilder.DropTable(
                name: "RoomCleanings");

            migrationBuilder.DropTable(
                name: "Staffs");

            migrationBuilder.DropTable(
                name: "HotelServices");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
