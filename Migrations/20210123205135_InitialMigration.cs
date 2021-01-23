using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fork.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JavaSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaxRam = table.Column<int>(type: "INTEGER", nullable: false),
                    JavaPath = table.Column<string>(type: "TEXT", nullable: true),
                    StartupParameters = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JavaSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerVersion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    Build = table.Column<int>(type: "INTEGER", nullable: false),
                    JarLink = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerVersion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SimpleTime",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Hours = table.Column<int>(type: "INTEGER", nullable: false),
                    Minutes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimpleTime", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Networks",
                columns: table => new
                {
                    UID = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    ProxyType = table.Column<int>(type: "INTEGER", nullable: false),
                    JavaSettingsId = table.Column<int>(type: "INTEGER", nullable: true),
                    SyncServers = table.Column<bool>(type: "INTEGER", nullable: false),
                    Initialized = table.Column<bool>(type: "INTEGER", nullable: false),
                    StartWithFork = table.Column<bool>(type: "INTEGER", nullable: false),
                    ServerIconId = table.Column<int>(type: "INTEGER", nullable: false),
                    VersionId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Networks", x => x.UID);
                    table.ForeignKey(
                        name: "FK_Networks_JavaSettings_JavaSettingsId",
                        column: x => x.JavaSettingsId,
                        principalTable: "JavaSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Networks_ServerVersion_VersionId",
                        column: x => x.VersionId,
                        principalTable: "ServerVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestartTime",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestartTime", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestartTime_SimpleTime_TimeId",
                        column: x => x.TimeId,
                        principalTable: "SimpleTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StartTime",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartTime", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StartTime_SimpleTime_TimeId",
                        column: x => x.TimeId,
                        principalTable: "SimpleTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StopTime",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StopTime", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StopTime_SimpleTime_TimeId",
                        column: x => x.TimeId,
                        principalTable: "SimpleTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    UID = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    VersionId = table.Column<int>(type: "INTEGER", nullable: true),
                    JavaSettingsId = table.Column<int>(type: "INTEGER", nullable: true),
                    Initialized = table.Column<bool>(type: "INTEGER", nullable: false),
                    StartWithFork = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoSetSha1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    ServerIconId = table.Column<int>(type: "INTEGER", nullable: false),
                    ResourcePackHashAge = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Restart1Id = table.Column<int>(type: "INTEGER", nullable: true),
                    Restart2Id = table.Column<int>(type: "INTEGER", nullable: true),
                    Restart3Id = table.Column<int>(type: "INTEGER", nullable: true),
                    Restart4Id = table.Column<int>(type: "INTEGER", nullable: true),
                    AutoStop1Id = table.Column<int>(type: "INTEGER", nullable: true),
                    AutoStop2Id = table.Column<int>(type: "INTEGER", nullable: true),
                    AutoStart1Id = table.Column<int>(type: "INTEGER", nullable: true),
                    AutoStart2Id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.UID);
                    table.ForeignKey(
                        name: "FK_Servers_JavaSettings_JavaSettingsId",
                        column: x => x.JavaSettingsId,
                        principalTable: "JavaSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Servers_RestartTime_Restart1Id",
                        column: x => x.Restart1Id,
                        principalTable: "RestartTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Servers_RestartTime_Restart2Id",
                        column: x => x.Restart2Id,
                        principalTable: "RestartTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Servers_RestartTime_Restart3Id",
                        column: x => x.Restart3Id,
                        principalTable: "RestartTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Servers_RestartTime_Restart4Id",
                        column: x => x.Restart4Id,
                        principalTable: "RestartTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Servers_ServerVersion_VersionId",
                        column: x => x.VersionId,
                        principalTable: "ServerVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Servers_StartTime_AutoStart1Id",
                        column: x => x.AutoStart1Id,
                        principalTable: "StartTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Servers_StartTime_AutoStart2Id",
                        column: x => x.AutoStart2Id,
                        principalTable: "StartTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Servers_StopTime_AutoStop1Id",
                        column: x => x.AutoStop1Id,
                        principalTable: "StopTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Servers_StopTime_AutoStop2Id",
                        column: x => x.AutoStop2Id,
                        principalTable: "StopTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Networks_JavaSettingsId",
                table: "Networks",
                column: "JavaSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Networks_VersionId",
                table: "Networks",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_RestartTime_TimeId",
                table: "RestartTime",
                column: "TimeId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_AutoStart1Id",
                table: "Servers",
                column: "AutoStart1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_AutoStart2Id",
                table: "Servers",
                column: "AutoStart2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_AutoStop1Id",
                table: "Servers",
                column: "AutoStop1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_AutoStop2Id",
                table: "Servers",
                column: "AutoStop2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_JavaSettingsId",
                table: "Servers",
                column: "JavaSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_Restart1Id",
                table: "Servers",
                column: "Restart1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_Restart2Id",
                table: "Servers",
                column: "Restart2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_Restart3Id",
                table: "Servers",
                column: "Restart3Id");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_Restart4Id",
                table: "Servers",
                column: "Restart4Id");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_VersionId",
                table: "Servers",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_StartTime_TimeId",
                table: "StartTime",
                column: "TimeId");

            migrationBuilder.CreateIndex(
                name: "IX_StopTime_TimeId",
                table: "StopTime",
                column: "TimeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Networks");

            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropTable(
                name: "JavaSettings");

            migrationBuilder.DropTable(
                name: "RestartTime");

            migrationBuilder.DropTable(
                name: "ServerVersion");

            migrationBuilder.DropTable(
                name: "StartTime");

            migrationBuilder.DropTable(
                name: "StopTime");

            migrationBuilder.DropTable(
                name: "SimpleTime");
        }
    }
}
