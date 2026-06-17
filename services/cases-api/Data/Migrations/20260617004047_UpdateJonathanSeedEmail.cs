using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllChecksOut.Cases.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJonathanSeedEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-jonathan-price",
                column: "Email",
                value: "arty.uptick@gmail.com");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-jonathan-price",
                column: "Email",
                value: "jonathan.price@dpaa.example");
        }
    }
}
