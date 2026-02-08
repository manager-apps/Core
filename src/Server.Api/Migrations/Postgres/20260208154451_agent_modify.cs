using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Api.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class agent_modify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecretKeyHash",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "SecretKeySalt",
                table: "Agents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "SecretKeyHash",
                table: "Agents",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "SecretKeySalt",
                table: "Agents",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
