using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagement.Migrations
{
    /// <inheritdoc />
    public partial class ApprovalsUserMatrixs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalsUserMatrixs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false),
                    WorkFlowUserGroupId = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalsUserMatrixs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalsUserMatrixs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalsUserMatrixs_SystemCodeDetails_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "SystemCodeDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalsUserMatrixs_WorkFlowUserGroups_WorkFlowUserGroupId",
                        column: x => x.WorkFlowUserGroupId,
                        principalTable: "WorkFlowUserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalsUserMatrixs_DocumentTypeId",
                table: "ApprovalsUserMatrixs",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalsUserMatrixs_UserId",
                table: "ApprovalsUserMatrixs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalsUserMatrixs_WorkFlowUserGroupId",
                table: "ApprovalsUserMatrixs",
                column: "WorkFlowUserGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalsUserMatrixs");
        }
    }
}
