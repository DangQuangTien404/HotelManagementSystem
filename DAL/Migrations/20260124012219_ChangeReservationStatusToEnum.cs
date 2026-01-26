using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReservationStatusToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add a temporary column to store the enum values
            migrationBuilder.AddColumn<int>(
                name: "StatusTemp",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Convert existing string values to enum integer values
            // Pending = 0, Confirmed = 1, Cancelled = 2, CheckedIn = 3, CheckedOut = 4
            migrationBuilder.Sql(@"
                UPDATE Reservations
                SET StatusTemp = CASE 
                    WHEN Status = 'Pending' THEN 0
                    WHEN Status = 'Confirmed' THEN 1
                    WHEN Status = 'Cancelled' THEN 2
                    WHEN Status = 'CheckedIn' THEN 3
                    WHEN Status = 'CheckedOut' THEN 4
                    ELSE 0
                END
            ");

            // Drop the old Status column
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reservations");

            // Rename the temporary column to Status
            migrationBuilder.RenameColumn(
                name: "StatusTemp",
                table: "Reservations",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add a temporary column to store the string values
            migrationBuilder.AddColumn<string>(
                name: "StatusTemp",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Pending");

            // Convert enum integer values back to strings
            migrationBuilder.Sql(@"
                UPDATE Reservations
                SET StatusTemp = CASE 
                    WHEN Status = 0 THEN 'Pending'
                    WHEN Status = 1 THEN 'Confirmed'
                    WHEN Status = 2 THEN 'Cancelled'
                    WHEN Status = 3 THEN 'CheckedIn'
                    WHEN Status = 4 THEN 'CheckedOut'
                    ELSE 'Pending'
                END
            ");

            // Drop the old Status column
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reservations");

            // Rename the temporary column to Status
            migrationBuilder.RenameColumn(
                name: "StatusTemp",
                table: "Reservations",
                newName: "Status");
        }
    }
}
