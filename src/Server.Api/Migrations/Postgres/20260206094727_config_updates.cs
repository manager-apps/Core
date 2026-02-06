using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Api.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class config_updates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SynchronizationExitIntervalSeconds",
                table: "Configs",
                newName: "IterationDelaySeconds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IterationDelaySeconds",
                table: "Configs",
                newName: "SynchronizationExitIntervalSeconds");
        }
    }
}
