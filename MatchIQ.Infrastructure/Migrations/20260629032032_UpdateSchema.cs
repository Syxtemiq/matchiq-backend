using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MatchIQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "stripe_payment_intent_id",
                table: "payments",
                newName: "payment_transaction_id");

            migrationBuilder.RenameColumn(
                name: "stripe_checkout_session_id",
                table: "payments",
                newName: "payment_checkout_id");

            migrationBuilder.RenameIndex(
                name: "IX_payments_stripe_checkout_session_id",
                table: "payments",
                newName: "IX_payments_payment_checkout_id");

            migrationBuilder.RenameIndex(
                name: "idx_payments_stripe_intent",
                table: "payments",
                newName: "idx_payments_transaction_id");

            migrationBuilder.AlterColumn<string>(
                name: "role",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "cedula",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "email_verified",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "full_name",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "test_submissions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<string>(
                name: "question_type",
                table: "test_questions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "role",
                table: "question_chat_messages",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<string>(
                name: "stage",
                table: "matches",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Matched");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "job_offers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "PendingPayment");

            migrationBuilder.AlterColumn<string>(
                name: "required_english_level",
                table: "job_offers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "modality",
                table: "job_offers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "TestDeadlineDays",
                table: "job_offers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "seniority",
                table: "candidate_profiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "english_level",
                table: "candidate_profiles",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "linkedin_url",
                table: "candidate_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "proctoring_sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    session_id = table.Column<string>(type: "text", nullable: false),
                    usuario_id = table.Column<string>(type: "text", nullable: false),
                    submission_id = table.Column<int>(type: "integer", nullable: true),
                    inicio = table.Column<DateTime>(type: "timestamp", nullable: false),
                    fin = table.Column<DateTime>(type: "timestamp", nullable: true),
                    total_frames_procesados = table.Column<int>(type: "integer", nullable: true),
                    integrity_score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    integrity_summary = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proctoring_sessions", x => x.Id);
                    table.UniqueConstraint("AK_proctoring_sessions_session_id", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_proctoring_sessions_test_submissions_submission_id",
                        column: x => x.submission_id,
                        principalTable: "test_submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proctoring_events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    session_id = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    detalle = table.Column<string>(type: "text", nullable: true),
                    evidencia = table.Column<string>(type: "text", nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proctoring_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proctoring_events_proctoring_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "proctoring_sessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_cedula",
                table: "users",
                column: "cedula",
                unique: true,
                filter: "cedula IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_proctoring_events_session",
                table: "proctoring_events",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "idx_proctoring_sessions_submission",
                table: "proctoring_sessions",
                column: "submission_id");

            migrationBuilder.CreateIndex(
                name: "idx_proctoring_sessions_usuario",
                table: "proctoring_sessions",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_proctoring_sessions_session_id",
                table: "proctoring_sessions",
                column: "session_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "proctoring_events");

            migrationBuilder.DropTable(
                name: "proctoring_sessions");

            migrationBuilder.DropIndex(
                name: "IX_users_cedula",
                table: "users");

            migrationBuilder.DropColumn(
                name: "cedula",
                table: "users");

            migrationBuilder.DropColumn(
                name: "email_verified",
                table: "users");

            migrationBuilder.DropColumn(
                name: "full_name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "TestDeadlineDays",
                table: "job_offers");

            migrationBuilder.DropColumn(
                name: "linkedin_url",
                table: "candidate_profiles");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "payment_transaction_id",
                table: "payments",
                newName: "stripe_payment_intent_id");

            migrationBuilder.RenameColumn(
                name: "payment_checkout_id",
                table: "payments",
                newName: "stripe_checkout_session_id");

            migrationBuilder.RenameIndex(
                name: "IX_payments_payment_checkout_id",
                table: "payments",
                newName: "IX_payments_stripe_checkout_session_id");

            migrationBuilder.RenameIndex(
                name: "idx_payments_transaction_id",
                table: "payments",
                newName: "idx_payments_stripe_intent");

            migrationBuilder.AlterColumn<string>(
                name: "role",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "test_submissions",
                type: "text",
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "question_type",
                table: "test_questions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "role",
                table: "question_chat_messages",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "payments",
                type: "text",
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "stage",
                table: "matches",
                type: "text",
                nullable: false,
                defaultValue: "Matched",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "job_offers",
                type: "text",
                nullable: false,
                defaultValue: "PendingPayment",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "required_english_level",
                table: "job_offers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "modality",
                table: "job_offers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "seniority",
                table: "candidate_profiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "english_level",
                table: "candidate_profiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);
        }
    }
}
