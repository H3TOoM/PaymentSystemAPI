using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionLog_Transactions_TransactionId",
                table: "TransactionLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TransactionLog",
                table: "TransactionLog");

            migrationBuilder.RenameTable(
                name: "TransactionLog",
                newName: "TransactionLogs");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionLog_TransactionId",
                table: "TransactionLogs",
                newName: "IX_TransactionLogs_TransactionId");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceId",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransactionLogs",
                table: "TransactionLogs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReferenceId",
                table: "Transactions",
                column: "ReferenceId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionLogs_Transactions_TransactionId",
                table: "TransactionLogs",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionLogs_Transactions_TransactionId",
                table: "TransactionLogs");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ReferenceId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TransactionLogs",
                table: "TransactionLogs");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "TransactionLogs",
                newName: "TransactionLog");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionLogs_TransactionId",
                table: "TransactionLog",
                newName: "IX_TransactionLog_TransactionId");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceId",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransactionLog",
                table: "TransactionLog",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionLog_Transactions_TransactionId",
                table: "TransactionLog",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
