using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllChecksOut.Cases.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedUserEmailsToGmailAliases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-aisha-khan",
                column: "Email",
                value: "arty.uptick+aisha-khan@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-amara-singh",
                column: "Email",
                value: "arty.uptick+amara-singh@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-amelia-wright",
                column: "Email",
                value: "arty.uptick+amelia-wright@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-benjamin-foster",
                column: "Email",
                value: "arty.uptick+benjamin-foster@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-ellen-brooks",
                column: "Email",
                value: "arty.uptick+ellen-brooks@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-george-evans",
                column: "Email",
                value: "arty.uptick+george-evans@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-jonathan-price",
                column: "Email",
                value: "arty.uptick+jonathan-price@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-lewis-green",
                column: "Email",
                value: "arty.uptick+lewis-green@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-maya-patel",
                column: "Email",
                value: "arty.uptick+maya-patel@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-michael-reeves",
                column: "Email",
                value: "arty.uptick+michael-reeves@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-nadia-cole",
                column: "Email",
                value: "arty.uptick+nadia-cole@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-owen-clarke",
                column: "Email",
                value: "arty.uptick+owen-clarke@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-peter-walsh",
                column: "Email",
                value: "arty.uptick+peter-walsh@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-priya-shah",
                column: "Email",
                value: "arty.uptick+priya-shah@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-rachel-morgan",
                column: "Email",
                value: "arty.uptick+rachel-morgan@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-sophie-turner",
                column: "Email",
                value: "arty.uptick+sophie-turner@gmail.com");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-aisha-khan",
                column: "Email",
                value: "aisha.khan@northstar-cloud.example");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-amara-singh",
                column: "Email",
                value: "amara.singh@dpaa.example");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-amelia-wright",
                column: "Email",
                value: "amelia.wright@cobalt-workflow.example");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-benjamin-foster",
                column: "Email",
                value: "benjamin.foster@mercury-retail.example");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-ellen-brooks",
                column: "Email",
                value: "ellen.brooks@ledgerfield.example");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-george-evans",
                column: "Email",
                value: "george.evans@sentinel-grc.example");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-jonathan-price",
                column: "Email",
                value: "arty.uptick@gmail.com");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-lewis-green",
                column: "Email",
                value: "lewis.green@cobalt-workflow.example");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-maya-patel",
                column: "Email",
                value: "maya.patel@pinebridge-data.example");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-michael-reeves",
                column: "Email",
                value: "michael.reeves@northstar-cloud.example");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-nadia-cole",
                column: "Email",
                value: "nadia.cole@portfolio.example");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-owen-clarke",
                column: "Email",
                value: "owen.clarke@asteria-identity.example");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-peter-walsh",
                column: "Email",
                value: "peter.walsh@harrington.example");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-priya-shah",
                column: "Email",
                value: "priya.shah@sentinel-grc.example");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-rachel-morgan",
                column: "Email",
                value: "rachel.morgan@harrington.example");

            migrationBuilder.UpdateData(
                schema: "cases",
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: "user-sophie-turner",
                column: "Email",
                value: "sophie.turner@mercury-retail.example");
        }
    }
}
