using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSubscriptionTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubscriptionTier",
                table: "users",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "basic");

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionTierSource",
                table: "users",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "default");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionTier",
                table: "users");

            migrationBuilder.DropColumn(
                name: "SubscriptionTierSource",
                table: "users");
        }
    }
}
