using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProperEnumsToRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add temporary string columns
            migrationBuilder.AddColumn<string>(
                name: "RoomType_New",
                table: "Rooms",
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: "Single");

            migrationBuilder.AddColumn<string>(
                name: "Status_New",
                table: "Rooms",
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: "Available");

            // Migrate RoomType data from int to string
            migrationBuilder.Sql(@"
                UPDATE Rooms SET RoomType_New = 
                    CASE RoomType
                        WHEN 0 THEN 'Single'
                        WHEN 1 THEN 'Double'
                        WHEN 2 THEN 'Suite'
                        WHEN 3 THEN 'Deluxe'
                        WHEN 4 THEN 'Family'
                        ELSE 'Single'
                    END
            ");

            // Migrate Status data from int to string
            migrationBuilder.Sql(@"
                UPDATE Rooms SET Status_New = 
                    CASE Status
                        WHEN 0 THEN 'Available'
                        WHEN 1 THEN 'Occupied'
                        WHEN 2 THEN 'Maintenance'
                        WHEN 3 THEN 'Reserved'
                        WHEN 4 THEN 'Cleaning'
                        ELSE 'Available'
                    END
            ");

            // Drop old int columns
            migrationBuilder.DropColumn(
                name: "RoomType",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Rooms");

            // Rename new columns to original names
            migrationBuilder.RenameColumn(
                name: "RoomType_New",
                table: "Rooms",
                newName: "RoomType");

            migrationBuilder.RenameColumn(
                name: "Status_New",
                table: "Rooms",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add temporary int columns
            migrationBuilder.AddColumn<int>(
                name: "RoomType_Old",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status_Old",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Migrate RoomType data from string back to int
            migrationBuilder.Sql(@"
                UPDATE Rooms SET RoomType_Old = 
                    CASE RoomType
                        WHEN 'Single' THEN 0
                        WHEN 'Double' THEN 1
                        WHEN 'Suite' THEN 2
                        WHEN 'Deluxe' THEN 3
                        WHEN 'Family' THEN 4
                        ELSE 0
                    END
            ");

            // Migrate Status data from string back to int
            migrationBuilder.Sql(@"
                UPDATE Rooms SET Status_Old = 
                    CASE Status
                        WHEN 'Available' THEN 0
                        WHEN 'Occupied' THEN 1
                        WHEN 'Maintenance' THEN 2
                        WHEN 'Reserved' THEN 3
                        WHEN 'Cleaning' THEN 4
                        ELSE 0
                    END
            ");

            // Drop string columns
            migrationBuilder.DropColumn(
                name: "RoomType",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Rooms");

            // Rename old columns to original names
            migrationBuilder.RenameColumn(
                name: "RoomType_Old",
                table: "Rooms",
                newName: "RoomType");

            migrationBuilder.RenameColumn(
                name: "Status_Old",
                table: "Rooms",
                newName: "Status");
        }
    }
}
