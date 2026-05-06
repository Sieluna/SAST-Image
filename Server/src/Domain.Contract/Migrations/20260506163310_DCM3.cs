using Domain.Event;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class DCM3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DomainEventBase>(
                name: "value",
                schema: "event",
                table: "events",
                type: "json",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "value",
                schema: "event",
                table: "events",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(DomainEventBase),
                oldType: "json");
        }
    }
}
