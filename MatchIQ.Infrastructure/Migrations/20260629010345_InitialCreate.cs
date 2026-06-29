using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MatchIQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pricing_tiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    min_candidates = table.Column<int>(type: "integer", nullable: false),
                    max_candidates = table.Column<int>(type: "integer", nullable: false),
                    price_cop = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pricing_tiers", x => x.Id);
                    table.CheckConstraint("ck_pricing_tiers_max_candidates", "max_candidates >= min_candidates");
                    table.CheckConstraint("ck_pricing_tiers_min_candidates", "min_candidates > 0");
                    table.CheckConstraint("ck_pricing_tiers_price_cop", "price_cop >= 0");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    google_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    picture_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_skills_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "candidate_profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    experience_years = table.Column<int>(type: "integer", nullable: true),
                    seniority = table.Column<string>(type: "text", nullable: true),
                    english_level = table.Column<string>(type: "text", nullable: true),
                    github_link = table.Column<string>(type: "text", nullable: true),
                    profile_photo_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidate_profiles", x => x.Id);
                    table.CheckConstraint("ck_candidate_profiles_experience_years", "experience_years >= 0");
                    table.ForeignKey(
                        name: "FK_candidate_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "company_profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    company_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_company_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "email_verifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    expires_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_verifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_email_verifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    expires_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_password_reset_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    expires_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "candidate_categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    candidate_id = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidate_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_candidate_categories_candidate_profiles_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "candidate_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_candidate_categories_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "candidate_skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    candidate_id = table.Column<int>(type: "integer", nullable: false),
                    skill_id = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidate_skills", x => x.Id);
                    table.CheckConstraint("ck_candidate_skills_level", "level BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_candidate_skills_candidate_profiles_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "candidate_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_candidate_skills_skills_skill_id",
                        column: x => x.skill_id,
                        principalTable: "skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "job_offers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    company_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    salary = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    modality = table.Column<string>(type: "text", nullable: false),
                    min_experience_years = table.Column<int>(type: "integer", nullable: true),
                    required_english_level = table.Column<string>(type: "text", nullable: true),
                    positions_available = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    tier_id = table.Column<int>(type: "integer", nullable: false),
                    candidates_to_test = table.Column<int>(type: "integer", nullable: true),
                    candidates_tested_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "PendingPayment"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    paid_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    test_sent_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_offers", x => x.Id);
                    table.CheckConstraint("ck_job_offers_candidates_tested_count", "candidates_tested_count >= 0");
                    table.CheckConstraint("ck_job_offers_candidates_to_test", "candidates_to_test IS NULL OR candidates_to_test > 0");
                    table.CheckConstraint("ck_job_offers_min_experience_years", "min_experience_years >= 0");
                    table.CheckConstraint("ck_job_offers_positions_available", "positions_available > 0");
                    table.ForeignKey(
                        name: "FK_job_offers_company_profiles_company_id",
                        column: x => x.company_id,
                        principalTable: "company_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_job_offers_pricing_tiers_tier_id",
                        column: x => x.tier_id,
                        principalTable: "pricing_tiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    offer_id = table.Column<int>(type: "integer", nullable: false),
                    candidate_id = table.Column<int>(type: "integer", nullable: false),
                    match_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    adjusted_score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ai_feedback = table.Column<string>(type: "jsonb", nullable: true),
                    stage = table.Column<string>(type: "text", nullable: false, defaultValue: "Matched"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.Id);
                    table.CheckConstraint("ck_matches_adjusted_score", "adjusted_score BETWEEN 0 AND 100");
                    table.CheckConstraint("ck_matches_match_percentage", "match_percentage BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_matches_candidate_profiles_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "candidate_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_matches_job_offers_offer_id",
                        column: x => x.offer_id,
                        principalTable: "job_offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "offer_categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    offer_id = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offer_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_offer_categories_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_offer_categories_job_offers_offer_id",
                        column: x => x.offer_id,
                        principalTable: "job_offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "offer_skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    offer_id = table.Column<int>(type: "integer", nullable: false),
                    skill_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offer_skills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_offer_skills_job_offers_offer_id",
                        column: x => x.offer_id,
                        principalTable: "job_offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_offer_skills_skills_skill_id",
                        column: x => x.skill_id,
                        principalTable: "skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    offer_id = table.Column<int>(type: "integer", nullable: false),
                    tier_id = table.Column<int>(type: "integer", nullable: false),
                    stripe_payment_intent_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    stripe_checkout_session_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    amount_cop = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    paid_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payments_job_offers_offer_id",
                        column: x => x.offer_id,
                        principalTable: "job_offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payments_pricing_tiers_tier_id",
                        column: x => x.tier_id,
                        principalTable: "pricing_tiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    offer_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    time_limit_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tests", x => x.Id);
                    table.CheckConstraint("ck_tests_time_limit_minutes", "time_limit_minutes > 0");
                    table.ForeignKey(
                        name: "FK_tests_job_offers_offer_id",
                        column: x => x.offer_id,
                        principalTable: "job_offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    test_id = table.Column<int>(type: "integer", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    question_type = table.Column<string>(type: "text", nullable: false),
                    question_text = table.Column<string>(type: "text", nullable: false),
                    explanation = table.Column<string>(type: "text", nullable: true),
                    is_gorilla = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    gorilla_hint = table.Column<string>(type: "text", nullable: true),
                    options_json = table.Column<string>(type: "jsonb", nullable: true),
                    correct_answer = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    function_signature = table.Column<string>(type: "text", nullable: true),
                    example_input = table.Column<string>(type: "text", nullable: true),
                    expected_behavior = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_questions", x => x.Id);
                    table.CheckConstraint("ck_test_questions_order_index", "order_index > 0");
                    table.ForeignKey(
                        name: "FK_test_questions_tests_test_id",
                        column: x => x.test_id,
                        principalTable: "tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_submissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    test_id = table.Column<int>(type: "integer", nullable: false),
                    candidate_id = table.Column<int>(type: "integer", nullable: false),
                    answers_json = table.Column<string>(type: "jsonb", nullable: true),
                    score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    feedback = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    started_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ai_evaluated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deadline = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_submissions", x => x.Id);
                    table.CheckConstraint("ck_test_submissions_score", "score BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_test_submissions_candidate_profiles_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "candidate_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_test_submissions_tests_test_id",
                        column: x => x.test_id,
                        principalTable: "tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "question_chat_messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_chat_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_question_chat_messages_test_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "test_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_candidate_categories",
                table: "candidate_categories",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_categories_candidate_id_category_id",
                table: "candidate_categories",
                columns: new[] { "candidate_id", "category_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_candidate_categories_category_id",
                table: "candidate_categories",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_profiles_user_id",
                table: "candidate_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_candidate_skills",
                table: "candidate_skills",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_skills_candidate_id_skill_id",
                table: "candidate_skills",
                columns: new[] { "candidate_id", "skill_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_candidate_skills_skill_id",
                table: "candidate_skills",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_name",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_company_profiles_user_id",
                table: "company_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_email_verifications_user",
                table: "email_verifications",
                columns: new[] { "user_id", "used" });

            migrationBuilder.CreateIndex(
                name: "idx_offers_company_id",
                table: "job_offers",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "idx_offers_test_sent_status",
                table: "job_offers",
                column: "status",
                filter: "status = 'TestSent'");

            migrationBuilder.CreateIndex(
                name: "idx_offers_tier_id",
                table: "job_offers",
                column: "tier_id");

            migrationBuilder.CreateIndex(
                name: "idx_matches_candidate_id",
                table: "matches",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "idx_matches_offer_id",
                table: "matches",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "idx_matches_stage",
                table: "matches",
                columns: new[] { "offer_id", "stage" });

            migrationBuilder.CreateIndex(
                name: "IX_matches_offer_id_candidate_id",
                table: "matches",
                columns: new[] { "offer_id", "candidate_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_offer_categories",
                table: "offer_categories",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "IX_offer_categories_category_id",
                table: "offer_categories",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_offer_categories_offer_id_category_id",
                table: "offer_categories",
                columns: new[] { "offer_id", "category_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_offer_skills",
                table: "offer_skills",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "IX_offer_skills_offer_id_skill_id",
                table: "offer_skills",
                columns: new[] { "offer_id", "skill_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_offer_skills_skill_id",
                table: "offer_skills",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "idx_password_reset_token",
                table: "password_reset_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_user_id",
                table: "password_reset_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_payments_offer_id",
                table: "payments",
                column: "offer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_payments_status",
                table: "payments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_payments_stripe_intent",
                table: "payments",
                column: "stripe_payment_intent_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_stripe_checkout_session_id",
                table: "payments",
                column: "stripe_checkout_session_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_tier_id",
                table: "payments",
                column: "tier_id");

            migrationBuilder.CreateIndex(
                name: "idx_chat_messages_question_id",
                table: "question_chat_messages",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_token",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_skills_category_id",
                table: "skills",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_skills_name_category_id",
                table: "skills",
                columns: new[] { "name", "category_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_questions_test_id",
                table: "test_questions",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_questions_test_id_order_index",
                table: "test_questions",
                columns: new[] { "test_id", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_submissions_candidate_id",
                table: "test_submissions",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "idx_submissions_deadline",
                table: "test_submissions",
                column: "deadline",
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "idx_submissions_test_id",
                table: "test_submissions",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_submissions_test_id_candidate_id",
                table: "test_submissions",
                columns: new[] { "test_id", "candidate_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_tests_offer_id",
                table: "tests",
                column: "offer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_google_id",
                table: "users",
                column: "google_id",
                unique: true,
                filter: "google_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "candidate_categories");

            migrationBuilder.DropTable(
                name: "candidate_skills");

            migrationBuilder.DropTable(
                name: "email_verifications");

            migrationBuilder.DropTable(
                name: "matches");

            migrationBuilder.DropTable(
                name: "offer_categories");

            migrationBuilder.DropTable(
                name: "offer_skills");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "question_chat_messages");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "test_submissions");

            migrationBuilder.DropTable(
                name: "skills");

            migrationBuilder.DropTable(
                name: "test_questions");

            migrationBuilder.DropTable(
                name: "candidate_profiles");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "tests");

            migrationBuilder.DropTable(
                name: "job_offers");

            migrationBuilder.DropTable(
                name: "company_profiles");

            migrationBuilder.DropTable(
                name: "pricing_tiers");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
