using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PLM.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPLMModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateSequence<int>(
                name: "ECONumberSeq",
                schema: "dbo");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Attachments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoMs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoMs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoMs_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BoMComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoMId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoMComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoMComponents_BoMs_BoMId",
                        column: x => x.BoMId,
                        principalTable: "BoMs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoMComponents_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BoMOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoMId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TimeMinutes = table.Column<int>(type: "int", nullable: false),
                    WorkCenter = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoMOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoMOperations_BoMs_BoMId",
                        column: x => x.BoMId,
                        principalTable: "BoMs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ECOs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValueSql: "CONCAT('ECO-', FORMAT(NEXT VALUE FOR dbo.ECONumberSeq, '000000'))"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    BoMId = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsVersionUpdate = table.Column<bool>(type: "bit", nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    ProposedChangesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ECOs_BoMs_BoMId",
                        column: x => x.BoMId,
                        principalTable: "BoMs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ECOs_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ECOLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ECOId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ECOLogs_ECOs_ECOId",
                        column: x => x.ECOId,
                        principalTable: "ECOs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoMComponents_BoMId",
                table: "BoMComponents",
                column: "BoMId");

            migrationBuilder.CreateIndex(
                name: "IX_BoMComponents_ProductId",
                table: "BoMComponents",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BoMOperations_BoMId",
                table: "BoMOperations",
                column: "BoMId");

            migrationBuilder.CreateIndex(
                name: "IX_BoMs_ProductId",
                table: "BoMs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOLogs_ECOId",
                table: "ECOLogs",
                column: "ECOId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOs_BoMId",
                table: "ECOs",
                column: "BoMId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOs_ProductId",
                table: "ECOs",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BoMComponents");

            migrationBuilder.DropTable(
                name: "BoMOperations");

            migrationBuilder.DropTable(
                name: "ECOLogs");

            migrationBuilder.DropTable(
                name: "ECOs");

            migrationBuilder.DropTable(
                name: "BoMs");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropSequence(
                name: "ECONumberSeq",
                schema: "dbo");
        }
    }
}
