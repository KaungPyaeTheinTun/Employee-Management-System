using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagement.Migrations
{
    /// <inheritdoc />
    public partial class FixedAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FixedAssets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    SerialNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ResponsibleEmployeeId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixedAssets_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FixedAssets_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FixedAssets_Employees_ResponsibleEmployeeId",
                        column: x => x.ResponsibleEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FixedAssets_SystemCodeDetails_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "SystemCodeDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FixedAssets_SystemCodeDetails_LocationId",
                        column: x => x.LocationId,
                        principalTable: "SystemCodeDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FixedAssets_SystemCodeDetails_StatusId",
                        column: x => x.StatusId,
                        principalTable: "SystemCodeDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_CategoryId",
                table: "FixedAssets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_CreatedById",
                table: "FixedAssets",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_LocationId",
                table: "FixedAssets",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_ModifiedById",
                table: "FixedAssets",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_ResponsibleEmployeeId",
                table: "FixedAssets",
                column: "ResponsibleEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_StatusId",
                table: "FixedAssets",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FixedAssets");
        }
    }
}
