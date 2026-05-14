using System;
using Domain.Event;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class DM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "domain");

            migrationBuilder.CreateTable(
                name: "events",
                schema: "domain",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grain_id = table.Column<long>(type: "bigint", nullable: false),
                    e_tag = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<DomainEventBase>(type: "jsonb", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.event_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_events_grain_id_e_tag",
                schema: "domain",
                table: "events",
                columns: new[] { "grain_id", "e_tag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_events_timestamp",
                schema: "domain",
                table: "events",
                column: "timestamp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_events_type",
                schema: "domain",
                table: "events",
                column: "type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "events",
                schema: "domain");
        }
    }
}
