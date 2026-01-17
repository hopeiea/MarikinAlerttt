using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MarikinAlert.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyRelayFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AssignedNodeName",
                table: "AdminUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AdminUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "AdminUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AdminUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AgencyRelays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReportId = table.Column<int>(type: "INTEGER", nullable: false),
                    AgencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    RelayedBy = table.Column<string>(type: "TEXT", nullable: false),
                    RelayedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyRelays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GovernmentAgencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Acronym = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    EmergencyHotline = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    HandlesCategories = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernmentAgencies", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AdminUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FullName", "Password", "Role" },
                values: new object[] { "", "", "Dispatcher" });

            migrationBuilder.InsertData(
                table: "GovernmentAgencies",
                columns: new[] { "Id", "Acronym", "Address", "Description", "Email", "EmergencyHotline", "HandlesCategories", "IsActive", "Name", "Priority" },
                values: new object[,]
                {
                    { 1, "BFP", "BFP National Headquarters, Quezon City", "Philippines national fire fighting agency", "bfp@fire.gov.ph", "116", "Fire", true, "Bureau of Fire Protection", 1 },
                    { 2, "PNP", "PNP National Headquarters, Camp Crame", "National police force for emergency response", "hotline@pnp.gov.ph", "117", "BuildingCollapse,Infrastructure", true, "Philippine National Police", 1 },
                    { 3, "PRC", "PRC National Headquarters, Manila", "Emergency medical services and disaster response", "info@redcross.org.ph", "143", "Medical", true, "Philippine Red Cross", 1 },
                    { 4, "MMDA", "MMDA Main Office, EDSA, Quezon City", "Metro Manila traffic and emergency coordination", "mmda@mmda.gov.ph", "136", "Infrastructure,Logistics", true, "Metro Manila Development Authority", 2 },
                    { 5, "OCD", "OCD National Headquarters, Quezon City", "National disaster risk reduction and management", "ocd@ocd.gov.ph", "911", "Fire,BuildingCollapse,Medical,Logistics,Infrastructure", true, "Office of Civil Defense", 1 },
                    { 6, "DPWH", "DPWH Central Office, Port Area, Manila", "Infrastructure and public works emergency response", "info@dpwh.gov.ph", "165-02", "Infrastructure,BuildingCollapse", true, "Department of Public Works and Highways", 2 },
                    { 7, "CHO", "Marikina City Health Office, Marikina City", "Local health department for medical emergencies", "health@marikina.gov.ph", "8888", "Medical", true, "City Health Office - Marikina", 2 },
                    { 8, "CEO", "Marikina City Engineering Office, Marikina City", "Local engineering office for infrastructure emergencies", "engineering@marikina.gov.ph", "8888", "Infrastructure,BuildingCollapse", true, "City Engineering Office - Marikina", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Category",
                table: "Reports",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Category_Priority",
                table: "Reports",
                columns: new[] { "Category", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Priority",
                table: "Reports",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Priority_Timestamp",
                table: "Reports",
                columns: new[] { "Priority", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Status",
                table: "Reports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Timestamp",
                table: "Reports",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedReports_ArchivedTimestamp",
                table: "ArchivedReports",
                column: "ArchivedTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedReports_OriginalReportId",
                table: "ArchivedReports",
                column: "OriginalReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedReports_Status",
                table: "ArchivedReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Username",
                table: "AdminUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgencyRelays_AgencyId",
                table: "AgencyRelays",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyRelays_RelayedAt",
                table: "AgencyRelays",
                column: "RelayedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyRelays_ReportId",
                table: "AgencyRelays",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyRelays_ReportId_AgencyId",
                table: "AgencyRelays",
                columns: new[] { "ReportId", "AgencyId" });

            migrationBuilder.CreateIndex(
                name: "IX_AgencyRelays_Status",
                table: "AgencyRelays",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_GovernmentAgencies_Acronym",
                table: "GovernmentAgencies",
                column: "Acronym");

            migrationBuilder.CreateIndex(
                name: "IX_GovernmentAgencies_IsActive",
                table: "GovernmentAgencies",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyRelays");

            migrationBuilder.DropTable(
                name: "GovernmentAgencies");

            migrationBuilder.DropIndex(
                name: "IX_Reports_Category",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_Category_Priority",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_Priority",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_Priority_Timestamp",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_Status",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_Timestamp",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_ArchivedReports_ArchivedTimestamp",
                table: "ArchivedReports");

            migrationBuilder.DropIndex(
                name: "IX_ArchivedReports_OriginalReportId",
                table: "ArchivedReports");

            migrationBuilder.DropIndex(
                name: "IX_ArchivedReports_Status",
                table: "ArchivedReports");

            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_Username",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AdminUsers");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedNodeName",
                table: "AdminUsers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
