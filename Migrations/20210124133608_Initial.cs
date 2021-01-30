using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fork.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "JavaSettings",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaxRam = table.Column<int>("INTEGER", nullable: false),
                    JavaPath = table.Column<string>("TEXT", nullable: true),
                    StartupParameters = table.Column<string>("TEXT", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_JavaSettings", x => x.Id); });

            migrationBuilder.CreateTable(
                "ServerVersion",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>("INTEGER", nullable: false),
                    Version = table.Column<string>("TEXT", nullable: true),
                    Build = table.Column<int>("INTEGER", nullable: false),
                    JarLink = table.Column<string>("TEXT", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_ServerVersion", x => x.Id); });

            migrationBuilder.CreateTable(
                "SimpleTime",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Hours = table.Column<int>("INTEGER", nullable: false),
                    Minutes = table.Column<int>("INTEGER", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_SimpleTime", x => x.Id); });

            migrationBuilder.CreateTable(
                "Networks",
                table => new
                {
                    UID = table.Column<string>("TEXT", nullable: false),
                    Name = table.Column<string>("TEXT", nullable: true),
                    ProxyType = table.Column<int>("INTEGER", nullable: false),
                    JavaSettingsId = table.Column<int>("INTEGER", nullable: true),
                    SyncServers = table.Column<bool>("INTEGER", nullable: false),
                    Initialized = table.Column<bool>("INTEGER", nullable: false),
                    StartWithFork = table.Column<bool>("INTEGER", nullable: false),
                    ServerIconId = table.Column<int>("INTEGER", nullable: false),
                    VersionId = table.Column<int>("INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Networks", x => x.UID);
                    table.ForeignKey(
                        "FK_Networks_JavaSettings_JavaSettingsId",
                        x => x.JavaSettingsId,
                        "JavaSettings",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_Networks_ServerVersion_VersionId",
                        x => x.VersionId,
                        "ServerVersion",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "RestartTime",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Enabled = table.Column<bool>("INTEGER", nullable: false),
                    TimeId = table.Column<int>("INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestartTime", x => x.Id);
                    table.ForeignKey(
                        "FK_RestartTime_SimpleTime_TimeId",
                        x => x.TimeId,
                        "SimpleTime",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "StartTime",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Enabled = table.Column<bool>("INTEGER", nullable: false),
                    TimeId = table.Column<int>("INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartTime", x => x.Id);
                    table.ForeignKey(
                        "FK_StartTime_SimpleTime_TimeId",
                        x => x.TimeId,
                        "SimpleTime",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "StopTime",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Enabled = table.Column<bool>("INTEGER", nullable: false),
                    TimeId = table.Column<int>("INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StopTime", x => x.Id);
                    table.ForeignKey(
                        "FK_StopTime_SimpleTime_TimeId",
                        x => x.TimeId,
                        "SimpleTime",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "Servers",
                table => new
                {
                    UID = table.Column<string>("TEXT", nullable: false),
                    Name = table.Column<string>("TEXT", nullable: true),
                    VersionId = table.Column<int>("INTEGER", nullable: true),
                    JavaSettingsId = table.Column<int>("INTEGER", nullable: true),
                    Initialized = table.Column<bool>("INTEGER", nullable: false),
                    StartWithFork = table.Column<bool>("INTEGER", nullable: false),
                    AutoSetSha1 = table.Column<bool>("INTEGER", nullable: false),
                    ServerIconId = table.Column<int>("INTEGER", nullable: false),
                    ResourcePackHashAge = table.Column<DateTime>("TEXT", nullable: false),
                    Restart1Id = table.Column<int>("INTEGER", nullable: true),
                    Restart2Id = table.Column<int>("INTEGER", nullable: true),
                    Restart3Id = table.Column<int>("INTEGER", nullable: true),
                    Restart4Id = table.Column<int>("INTEGER", nullable: true),
                    AutoStop1Id = table.Column<int>("INTEGER", nullable: true),
                    AutoStop2Id = table.Column<int>("INTEGER", nullable: true),
                    AutoStart1Id = table.Column<int>("INTEGER", nullable: true),
                    AutoStart2Id = table.Column<int>("INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.UID);
                    table.ForeignKey(
                        "FK_Servers_JavaSettings_JavaSettingsId",
                        x => x.JavaSettingsId,
                        "JavaSettings",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_Servers_RestartTime_Restart1Id",
                        x => x.Restart1Id,
                        "RestartTime",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_Servers_RestartTime_Restart2Id",
                        x => x.Restart2Id,
                        "RestartTime",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_Servers_RestartTime_Restart3Id",
                        x => x.Restart3Id,
                        "RestartTime",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_Servers_RestartTime_Restart4Id",
                        x => x.Restart4Id,
                        "RestartTime",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_Servers_ServerVersion_VersionId",
                        x => x.VersionId,
                        "ServerVersion",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_Servers_StartTime_AutoStart1Id",
                        x => x.AutoStart1Id,
                        "StartTime",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_Servers_StartTime_AutoStart2Id",
                        x => x.AutoStart2Id,
                        "StartTime",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_Servers_StopTime_AutoStop1Id",
                        x => x.AutoStop1Id,
                        "StopTime",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_Servers_StopTime_AutoStop2Id",
                        x => x.AutoStop2Id,
                        "StopTime",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                "IX_Networks_JavaSettingsId",
                "Networks",
                "JavaSettingsId");

            migrationBuilder.CreateIndex(
                "IX_Networks_VersionId",
                "Networks",
                "VersionId");

            migrationBuilder.CreateIndex(
                "IX_RestartTime_TimeId",
                "RestartTime",
                "TimeId");

            migrationBuilder.CreateIndex(
                "IX_Servers_AutoStart1Id",
                "Servers",
                "AutoStart1Id");

            migrationBuilder.CreateIndex(
                "IX_Servers_AutoStart2Id",
                "Servers",
                "AutoStart2Id");

            migrationBuilder.CreateIndex(
                "IX_Servers_AutoStop1Id",
                "Servers",
                "AutoStop1Id");

            migrationBuilder.CreateIndex(
                "IX_Servers_AutoStop2Id",
                "Servers",
                "AutoStop2Id");

            migrationBuilder.CreateIndex(
                "IX_Servers_JavaSettingsId",
                "Servers",
                "JavaSettingsId");

            migrationBuilder.CreateIndex(
                "IX_Servers_Restart1Id",
                "Servers",
                "Restart1Id");

            migrationBuilder.CreateIndex(
                "IX_Servers_Restart2Id",
                "Servers",
                "Restart2Id");

            migrationBuilder.CreateIndex(
                "IX_Servers_Restart3Id",
                "Servers",
                "Restart3Id");

            migrationBuilder.CreateIndex(
                "IX_Servers_Restart4Id",
                "Servers",
                "Restart4Id");

            migrationBuilder.CreateIndex(
                "IX_Servers_VersionId",
                "Servers",
                "VersionId");

            migrationBuilder.CreateIndex(
                "IX_StartTime_TimeId",
                "StartTime",
                "TimeId");

            migrationBuilder.CreateIndex(
                "IX_StopTime_TimeId",
                "StopTime",
                "TimeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Networks");

            migrationBuilder.DropTable(
                "Servers");

            migrationBuilder.DropTable(
                "JavaSettings");

            migrationBuilder.DropTable(
                "RestartTime");

            migrationBuilder.DropTable(
                "ServerVersion");

            migrationBuilder.DropTable(
                "StartTime");

            migrationBuilder.DropTable(
                "StopTime");

            migrationBuilder.DropTable(
                "SimpleTime");
        }
    }
}