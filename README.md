
# MatchIQ

> **Intelligent B2B matching platform** connecting Colombian tech companies with senior software developers — powered by a proprietary scoring algorithm, AI-generated technical assessments, and real-time behavioral proctoring.

---

## Table of Contents

1. [Overview](#overview)
2. [Key Features](#key-features)
3. [Architecture](#architecture)
4. [Tech Stack](#tech-stack)
5. [Project Structure](#project-structure)
6. [Database Design](#database-design)
7. [Core Business Logic](#core-business-logic)
8. [API Reference](#api-reference)
9. [Authentication & Authorization](#authentication--authorization)
10. [Payment Integration](#payment-integration)
11. [AI Integration](#ai-integration)
12. [Proctoring System](#proctoring-system)
13. [Email Notifications](#email-notifications)
14. [Background Jobs](#background-jobs)
15. [Market Analytics](#market-analytics)
16. [Excel Reports](#excel-reports)
17. [Rate Limiting](#rate-limiting)
18. [Getting Started](#getting-started)
19. [Configuration Reference](#configuration-reference)
20. [API Response Contract](#api-response-contract)

---

## Overview

MatchIQ is a B2B SaaS platform built for the Colombian technology market. It solves a specific problem: **companies spend weeks screening developers who look good on paper but don't actually fit the role**. MatchIQ replaces that process with a three-stage pipeline:

1. **Score** — A weighted SQL algorithm matches every registered candidate against a published job offer across four dimensions: skills (50 %), categories (20 %), experience (20 %), and English level (10 %). The AI then enriches the top candidates with a qualitative fit score.
2. **Test** — The company selects candidates from the ranked list and sends them an AI-generated technical assessment tailored to the specific offer. The test has a configurable time limit and is evaluated autonomously by GPT-4o-mini.
3. **Decide** — The company reviews scores, AI feedback, and proctoring integrity reports before selecting or rejecting candidates.

### What makes MatchIQ different from LinkedIn or Indeed

| Dimension | LinkedIn / Indeed | MatchIQ |
|---|---|---|
| Matching | Keyword search | Weighted algorithm + AI qualitative score |
| Assessment | External, manual | AI-generated per offer, auto-evaluated |
| Pricing | Monthly subscription | One-time payment per offer |
| Integrity | None | Behavioral proctoring with AI integrity score |
| Market | Global | Colombian tech market (Wompi / PSE) |

---

## Key Features

### For companies
- Create job offers with AI-assisted parsing (skills and categories extracted from free-text description)
- Pay once per offer via Wompi (credit card or PSE — Colombian payment gateway)
- View a ranked list of matching candidates with percentage scores and AI qualitative insights
- Re-evaluate the ranking at any time to include newly registered candidates
- Generate and customize AI-powered technical tests per offer
- Edit test questions via an AI chat interface
- Send tests in bulk to selected candidates with configurable deadlines
- Review submissions with AI-evaluated scores and detailed feedback
- Download proctoring integrity reports per candidate
- Access aggregated metrics via a company dashboard
- Export offer and candidate pipeline data to Excel

### For candidates
- Register via email + verification code, or via Google OAuth (one click)
- Complete a structured profile: skills (rated 1–5), categories, seniority, English level, experience
- Automatically matched against every open offer on profile creation or update
- Receive test invitations by email with a deep link
- Complete a timed assessment with multiple-choice and code challenges
- View personal score and AI-generated feedback after evaluation
- Access skill demand intelligence: see which skills the market needs most, which combinations companies request together, and how their profile compares

### For administrators
- Manage all users: activate, deactivate, delete
- Create new admin accounts
- View platform-wide statistics (revenue, conversion rates, match counts)
- Export a full platform report to Excel (3 sheets: summary, companies, payments)

---

## Architecture

MatchIQ follows **Clean Architecture** organized into four layers with strict dependency inversion. Inner layers define interfaces; outer layers implement them.

```
┌─────────────────────────────────────────────────────────┐
│                      MatchIQ.API                        │
│  Controllers · Middlewares · BackgroundServices ·       │
│  Program.cs · Swagger · Rate Limiting · CORS            │
├─────────────────────────────────────────────────────────┤
│                 MatchIQ.Application                     │
│  Services (business logic) · DTOs · Interfaces          │
│  Modules: Auth · Candidate · Company · Offers ·         │
│  Matching · Tests · Admin · Analytics                   │
├─────────────────────────────────────────────────────────┤
│                 MatchIQ.Infrastructure                  │
│  EF Core (AppDbContext) · Repositories ·                │
│  OpenAI · MailKit · Wompi · JWT · Google OAuth ·        │
│  ClosedXML (Excel)                                      │
├─────────────────────────────────────────────────────────┤
│                   MatchIQ.Domain                        │
│  Entities · Enums (no dependencies on other layers)     │
└─────────────────────────────────────────────────────────┘
```

**Dependency rule:** `API` → `Application` ← `Infrastructure`, `Domain` ← all layers.

### Key architectural decisions

- **No ENUM types in PostgreSQL.** All domain-state fields are `VARCHAR`. Conversion to C# enums is handled by EF Core value converters, keeping migrations simpler and the DB portable.
- **PL/pgSQL for matching.** The scoring algorithm lives in two PostgreSQL functions (`get_candidate_matches`, `get_full_offer_ranking`) to leverage set-based operations at scale. Everything else uses LINQ.
- **Repository pattern only for raw SQL.** `MatchRepository` and `TestRepository` exist solely to encapsulate `Database.SqlQuery<T>()` calls. All LINQ queries run directly via `IAppDbContext` in service classes.
- **AI is best-effort.** Every OpenAI call is wrapped in a try/catch. If the AI fails, the operation succeeds without enrichment. The daily job retries failed evaluations automatically.

---

## Tech Stack

| Layer | Technology | Version | Purpose |
|---|---|---|---|
| Runtime | .NET / C# | 10 / 13 | Backend API |
| ORM | Entity Framework Core | latest | Data access + migrations |
| Database | PostgreSQL | 15+ | Primary data store + PL/pgSQL matching |
| EF Driver | Npgsql | latest | PostgreSQL adapter for EF Core |
| AI | OpenAI GPT-4o-mini | via Betalgo SDK | Test generation, evaluation, parsing, fit score |
| Payments | Wompi | REST API | Colombian payments (credit card + PSE) |
| Auth | JWT HS256 | `System.IdentityModel.Tokens.Jwt` | Access + refresh token auth |
| OAuth | Google Identity | `Google.Apis.Auth` | Google Sign-In for candidates |
| Email | MailKit | latest | Transactional emails via SMTP |
| Reports | ClosedXML | latest | Excel (.xlsx) generation |
| API Docs | Swagger / Swashbuckle | latest | Interactive API documentation |
| Naming | EFCore.NamingConventions | latest | Auto snake_case column mapping |
| Frontend | Flutter Web | — | (separate repository) |

---

## Project Structure

```
MatchIQ/
├── MatchIQ.sln
│
├── MatchIQ.Domain/
│   └── Entities/               # Plain C# entity classes (no logic)
│       ├── User.cs
│       ├── CandidateProfile.cs
│       ├── CompanyProfile.cs
│       ├── JobOffer.cs
│       ├── Match.cs
│       ├── Skill.cs, Category.cs
│       ├── OfferSkill.cs, CandidateSkill.cs
│       ├── Test.cs, TestQuestion.cs, TestSubmission.cs
│       ├── ProctoringSession.cs, ProctoringEvent.cs
│       └── ...
│   └── Enums/                  # UserRole, OfferStatus, MatchStage, etc.
│
├── MatchIQ.Application/
│   ├── Common/
│   │   ├── Interfaces/         # IAppDbContext, IJwtService, IAIService, etc.
│   │   └── Dtos/               # Shared DTOs (ApiResponse)
│   └── Modules/
│       ├── Auth/               # AuthService + DTOs
│       ├── Candidate/          # CandidateService + DTOs
│       ├── Company/            # CompanyService + DTOs
│       ├── Offers/             # OffersService + DTOs
│       ├── Matching/           # MatchingService + DTOs
│       ├── Tests/              # TestService, TestEditorService, ProctoringService + DTOs
│       ├── Admin/              # AdminService + DTOs
│       └── Analytics/          # MarketService + DTOs
│
├── MatchIQ.Infrastructure/
│   ├── Persistence/
│   │   ├── AppDbContext.cs     # EF Core context + all entity configurations
│   │   └── Repositories/      # MatchRepository, TestRepository, JobOfferRepository
│   ├── AI/                    # OpenAIService, OfferParserService
│   ├── Auth/                  # JwtService, PasswordHasher, GoogleTokenValidator
│   ├── Email/                 # MailKitEmailService
│   ├── Payments/              # WompiService, StripeService
│   └── Reports/               # ReportService (ClosedXML)
│
├── MatchIQ.API/
│   ├── Controllers/           # One controller per module
│   ├── Middlewares/           # ErrorHandlingMiddleware, CurrentUserMiddleware
│   ├── BackgroundServices/    # DailyJobsService
│   ├── Services/              # CurrentUserService
│   ├── Common/                # ApiResponse<T> helper
│   └── Program.cs             # DI registration, middleware pipeline, Swagger
│
├── DBContext.md               # Full PostgreSQL schema (tables, functions, triggers, seed)
├── API_REFERENCE.md           # Complete API reference for frontend
├── ANALYTICS_FRONTEND.md      # Analytics endpoints guide for frontend
├── COMPANY_FRONTEND.md        # Company-specific endpoints guide
├── ADMIN_FRONTEND.md          # Admin endpoints guide
├── PROCTORING_FRONTEND.md     # Proctoring endpoints guide
├── matchiq_db_complete.sql    # Full DB creation script
├── add_test_deadline_days.sql # ALTER TABLE migration for deadline column
├── add_proctoring_tables.sql  # ALTER TABLE migration for proctoring tables
├── fix_sql_case.sql           # Enum case fix migration
└── fix_null_experience_years.sql
```

---

## Database Design

PostgreSQL 15+ with all domain-state fields stored as `VARCHAR` (no native ENUM types). EF Core handles enum conversion via value converters.

### Entity-Relationship overview

```
users ──────────────┬── candidate_profiles ──┬── candidate_skills ──── skills ──── categories
                    │                        └── candidate_categories ── categories
                    └── company_profiles ──── job_offers ──┬── offer_skills ──── skills
                                                           ├── offer_categories ── categories
                                                           ├── matches ──── candidate_profiles
                                                           ├── payments
                                                           └── tests ──── test_questions
                                                                     └── test_submissions ──── proctoring_sessions
                                                                                           └── proctoring_events
```

### Core tables

| Table | Description |
|---|---|
| `users` | All users — candidates, companies, admins. Google OAuth users have `password_hash = NULL` |
| `candidate_profiles` | Extended profile data for candidates (seniority, English, experience) |
| `company_profiles` | Company name linked to a user account |
| `skills` | Catalog of skills (e.g. React, Docker, Python) — each belongs to one category |
| `categories` | FrontEnd · BackEnd · FullStack · DevOps · QA · UX/UI · Databases |
| `candidate_skills` | Pivot: candidate ↔ skill with proficiency level (1–5) |
| `candidate_categories` | Pivot: candidate ↔ category |
| `job_offers` | Offers created by companies with status lifecycle and pricing tier |
| `offer_skills` | Pivot: offer ↔ required skills |
| `offer_categories` | Pivot: offer ↔ required categories |
| `pricing_tiers` | Starter (1 candidate / $89K COP) to Avanzado (8–15 / $599K COP) |
| `payments` | One payment per offer — updated via Wompi webhook |
| `matches` | Scoring results: one row per (offer, candidate) pair |
| `tests` | 1:1 with `job_offers` — the AI-generated test |
| `test_questions` | Individual questions (MultipleChoice or CodeChallenge) |
| `test_submissions` | Candidate attempt: answers, score, AI feedback, deadline |
| `proctoring_sessions` | Behavioral monitoring session per submission |
| `proctoring_events` | Individual events captured during proctoring (face, tab switch, etc.) |

### Offer status lifecycle

```
PendingPayment ──(payment confirmed)──► Open ──(test sent)──► TestSent ──(all decided)──► Completed
                                          │
                                          ├──(manual cancel)──► Cancelled
                                          └──(3 months elapsed)──► Expired
```

### PL/pgSQL functions

| Function | Description |
|---|---|
| `get_candidate_matches(offer_id)` | Incremental matching — new candidates only |
| `get_full_offer_ranking(offer_id)` | Full UPSERT of all candidates against an offer |
| `trigger_rematch_open_offers()` | Auto-triggered on `candidate_profiles` INSERT/UPDATE |
| `expire_stale_offers()` | Marks `Open` offers as `Expired` after 3 months |
| `expire_stale_submissions()` | Marks `Pending` submissions as `Expired` after deadline |

---

## Core Business Logic

### Matching algorithm

The scoring formula runs entirely in PostgreSQL for performance:

```
match_percentage = (skills_score) + (category_score) + (experience_score) + (english_score)

skills_score     = (matched_skills / total_offer_skills) × 50
category_score   = (matched_categories / total_offer_categories) × 20
experience_score = min(candidate_years / required_years, 1) × 20
english_score    = (candidate_english_rank / required_english_rank) × 10
```

Each dimension is capped at its weight. Missing requirements on the offer side default to full score (non-penalized). The formula is implemented twice: as an incremental query (`get_candidate_matches`) and as a full UPSERT (`get_full_offer_ranking`) used for re-evaluation.

### AI enrichment (top 3 candidates)

After the SQL score runs, the top 3 candidates without an existing `adjusted_score` are sent to GPT-4o-mini for qualitative evaluation. The AI returns a `FitScore` (0–10) and a written insight. The final score is:

```
adjusted_score = 0.9 × match_percentage + fit_score
```

`fit_score` is on a 0–10 scale, so its maximum contribution is 10 points. The `adjusted_score` is capped at 100.

On re-evaluation, candidates who already have `ai_feedback` recalculate `adjusted_score` without a new AI call. Only new top-3 candidates trigger a GPT request, minimizing API costs.

### Test lifecycle

```
Company creates offer
       │
       ▼
AI generates test (POST /api/tests/{offerId}/generate)
  • 1 CodeChallenge + N MultipleChoice questions
  • Questions are tailored to the offer's skills and categories
  • Company can edit questions via AI chat before sending
       │
       ▼
Company sends test to selected candidates (POST /api/matching/send-test)
  • Each candidate receives an email with a deep link
  • A TestSubmission row is created with a deadline
       │
       ▼
Candidate starts test (POST /api/tests/{offerId}/candidate/start)
  • Timer begins (time_limit_minutes)
       │
       ▼
Candidate submits answers (POST /api/tests/{testId}/submit)
  • Answers persisted before calling AI (prevents data loss on timeout)
       │
       ▼
AI evaluates submission (async, with 3 automatic retries + exponential backoff)
  • Score (0–100) + detailed feedback per question
  • DailyJobsService retries any evaluation that failed
```

---

## API Reference

All responses follow a unified contract (see [API Response Contract](#api-response-contract)).

**Base URL (development):** `http://localhost:5000`

### `/api/auth` — Public

| Method | Path | Description |
|---|---|---|
| POST | `/register` | Register with email and password |
| POST | `/verify-email` | Verify 6-digit email code |
| POST | `/resend-verification` | Resend verification code |
| POST | `/login` | Login — returns access token + refresh token |
| POST | `/refresh` | Exchange refresh token for new access token |
| POST | `/forgot-password` | Send password reset email |
| POST | `/reset-password` | Reset password with token |
| POST | `/google` | Login or register via Google ID token |
| POST | `/logout` | Revoke refresh token (requires auth) |

### `/api/candidate` — Candidate

| Method | Path | Description |
|---|---|---|
| GET | `/profile` | Get own candidate profile |
| PUT | `/profile` | Update profile (skills, categories, seniority, English, etc.) |

### `/api/company` — Company

| Method | Path | Description |
|---|---|---|
| GET | `/profile` | Get own company profile |
| PUT | `/profile` | Update company name |
| GET | `/dashboard` | Aggregated metrics (offers, matches, test stats) |
| GET | `/report` | Download Excel report (.xlsx) — 2 sheets |

### `/api/offers` — Company

| Method | Path | Description |
|---|---|---|
| POST | `/parse-description` | AI extracts skills/categories from free-text description |
| GET | `/tiers` | List pricing tiers |
| POST | `/` | Create a new job offer |
| GET | `/` | List own offers |
| GET | `/{id}` | Get offer detail |
| PUT | `/{id}` | Update offer |
| PATCH | `/{id}/cancel` | Cancel offer (returns 409 if candidates are in progress) |
| POST | `/{id}/force-cancel` | Force cancel regardless of candidate status |

### `/api/matching` — Company

| Method | Path | Description |
|---|---|---|
| POST | `/{offerId}/run` | Run incremental matching (new candidates only) |
| GET | `/{offerId}` | Get ranked candidate list for offer |
| POST | `/send-test` | Send test to selected candidates |
| POST | `/{offerId}/reevaluate` | Re-run full ranking including all candidates |
| POST | `/{matchId}/select` | Select a candidate |
| POST | `/{matchId}/reject` | Reject a candidate |

### `/api/tests` — Company

| Method | Path | Description |
|---|---|---|
| POST | `/{offerId}/generate` | Generate AI test for offer |
| POST | `/{offerId}/regenerate` | Regenerate test (overwrites existing) |
| GET | `/{offerId}` | Get full test with all questions |
| GET | `/questions/{questionId}/chat` | Get chat history for a question |
| POST | `/questions/{questionId}/chat` | Send message to AI to edit a question |
| GET | `/submissions/{matchId}` | Get a candidate's submission result |
| GET | `/proctoring/{matchId}` | Get proctoring integrity report for a candidate |

### `/api/tests` — Candidate

| Method | Path | Description |
|---|---|---|
| GET | `/candidate` | List own test invitations |
| GET | `/{offerId}/candidate/preview` | Preview test metadata (no questions yet) |
| POST | `/{offerId}/candidate/start` | Start test — begins timer |
| POST | `/{testId}/submit` | Submit answers |
| GET | `/{testId}/result` | Get own score and AI feedback |

### `/api/payments` — Mixed

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/create-checkout` | Company | Create Wompi payment link for an offer |
| POST | `/webhook` | Public | Wompi webhook receiver (SHA-256 verified) |

### `/api/catalog` — Public

| Method | Path | Description |
|---|---|---|
| GET | `/categories` | List all skill categories |
| GET | `/categories/{id}/skills` | List skills within a category |

### `/api/analytics` — Mixed

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/market` | Public | Top demand skills, top supply skills, top skill combinations |
| GET | `/market/my-insights` | Candidate | Same data annotated with candidate's own skill presence and gaps |

### `/api/admin` — Admin

| Method | Path | Description |
|---|---|---|
| POST | `/users` | Create admin account |
| GET | `/users` | List all users (filter by role, active status) |
| GET | `/users/{id}` | Get user by ID |
| PATCH | `/users/{id}/toggle-status` | Activate or deactivate a user |
| DELETE | `/users/{id}` | Delete user |
| GET | `/stats` | Platform-wide statistics |
| GET | `/report` | Download admin Excel report (.xlsx) — 3 sheets |

---

## Authentication & Authorization

### Token strategy

| Token | Lifetime | Storage |
|---|---|---|
| Access token (JWT HS256) | 60 minutes | Client memory — never persisted |
| Refresh token | 7 days | `refresh_tokens` table |

On every request, the access token is validated for signature, issuer, audience, and expiry. When it expires, the client calls `POST /api/auth/refresh` with the refresh token to get a new pair.

Logout revokes the refresh token immediately. Revoking all sessions is done by deleting all rows for a user from `refresh_tokens`.

### Roles

| Role | Description |
|---|---|
| `Candidate` | Registered developers — access to their own profile, tests, and analytics |
| `Company` | Tech companies — access to offers, matching, tests, and dashboard |
| `Admin` | Platform administrators — access to all user management and statistics |

Each role is enforced via `[Authorize(Roles = "...")]` on controllers or individual actions.

### Google OAuth flow

1. Frontend obtains a Google ID token using the Google Sign-In SDK.
2. Frontend sends the ID token to `POST /api/auth/google`.
3. Backend validates the token with `Google.Apis.Auth` (verifies audience and signature).
4. If the email already exists, it links the Google account. If not, it registers a new user.
5. Backend returns the same JWT pair as a regular login.

### Password reset flow

```
POST /forgot-password (email) → sends 6-character token by email
POST /reset-password (token + new password) → token consumed, password updated
```

Reset tokens expire after 15 minutes and are single-use.

---

## Payment Integration

MatchIQ uses **Wompi** as its payment gateway, supporting both credit cards and PSE (Colombian bank transfer), which makes it viable for any Colombian company regardless of card access.

### Payment flow

```
1. Company creates an offer (status: PendingPayment)
2. POST /api/payments/create-checkout?offerId=X
   → WompiService creates a payment link (idempotent — same link if called again)
   → Returns checkout URL
3. Company is redirected to Wompi's hosted payment page
4. Wompi processes payment and fires a webhook
5. POST /api/payments/webhook [AllowAnonymous]
   → Backend verifies SHA-256 signature using EventsSecret
   → On success: offer status → Open, matching runs automatically
   → On failure: payment status → Failed
```

The webhook uses Wompi's `events_secret` to compute and verify a SHA-256 HMAC signature, preventing unauthorized webhook calls.

---

## AI Integration

All AI calls use **GPT-4o-mini** via the Betalgo OpenAI SDK.

### AI operations

| Operation | Trigger | Model input | Model output |
|---|---|---|---|
| **Offer parsing** | `POST /offers/parse-description` | Raw job description text | Structured skills and categories |
| **Test generation** | `POST /tests/{offerId}/generate` | Offer skills, categories, title | Test title, N questions (MC + code challenge) |
| **Question editing** | `POST /tests/questions/{id}/chat` | Chat history + user instruction | Updated question fields |
| **Candidate fit score** | After matching top 3 | Candidate profile + offer details | FitScore (0–10) + qualitative insight |
| **Submission evaluation** | After `submit` | Questions + candidate answers | Score (0–100) + per-question feedback |

### Reliability measures

- Every AI call is wrapped in try/catch — partial failure does not break the main operation.
- Test evaluation has **3 automatic retries with exponential backoff** at submission time.
- The `DailyJobsService` runs `RetryPendingEvaluationsAsync()` every 24 hours to catch any evaluations that failed after all retries.
- AI-generated JSON responses are stripped of markdown code fences before deserialization.

---

## Proctoring System

The proctoring module provides behavioral integrity monitoring during test sessions. It operates independently of the test submission pipeline and adds an integrity layer for companies reviewing candidates.

### What it captures

- **Face presence detection** — periodic frame analysis to verify the candidate is in front of the camera
- **Tab switch / focus loss events** — browser visibility API events
- **Multiple faces detected** — potential external assistance
- **Window blur / minimize events**

### Data model

```
ProctoringSession (1) ──► (many) ProctoringEvent
     │
     └── integrity_score: 0–100 (computed by AI from all events)
     └── integrity_summary: AI-generated written assessment
```

### Company access

Companies can request the proctoring report for any candidate who has completed a test:

```
GET /api/tests/proctoring/{matchId}
Authorization: Bearer <company-token>
```

Returns the session's `integrityScore`, `integritySummary`, and the full list of events with timestamps and details.

---

## Email Notifications

All transactional emails are sent via **MailKit** over SMTP (Gmail by default). The system sends the following emails:

| Trigger | Recipient | Subject |
|---|---|---|
| Registration | Candidate / Company | Email verification code |
| Forgot password | Any user | Password reset link |
| Test invitation | Candidate | Invitation to present technical assessment |
| Test evaluated — selected | Candidate | Congratulations — you have been selected |
| Test evaluated — rejected | Candidate | Application update |

Emails are sent asynchronously and non-blocking. A failure in email delivery does not roll back the triggering operation.

---

## Background Jobs

`DailyJobsService` is an `IHostedService` that runs every 24 hours using `PeriodicTimer`. It also runs once immediately on application startup.

### Jobs executed on each cycle

| Job | What it does |
|---|---|
| `expire_stale_offers()` | Marks `Open` offers as `Expired` if `expires_at ≤ NOW()` (3-month window from payment) |
| `expire_stale_submissions()` | Marks `Pending` submissions as `Expired` if `deadline ≤ NOW()` |
| `RetryPendingEvaluationsAsync()` | Finds submissions with `status = Pending` and `submitted_at IS NOT NULL` and retries AI evaluation |

Each job is isolated in its own try/catch so a failure in one does not prevent the others from running. All outcomes are logged.

---

## Market Analytics

Two endpoints provide skill intelligence derived from live platform data.

### `GET /api/analytics/market` — Public

Returns market-wide statistics with no authentication required:

- **`topDemand`** — Top 10 skills ordered by number of distinct offers requesting them
- **`topSupply`** — Top 10 skills ordered by number of distinct candidates who have them
- **`topCombinations`** — Top 10 skill pairs that appear together most frequently in the same offer (co-occurrence analysis)

### `GET /api/analytics/market/my-insights` — Candidate

Returns the same market data annotated with the candidate's own profile:

- Each demand skill includes `candidateHasSkill` (bool) and `candidateLevel` (1–5 or null)
- Each combination includes `candidateHasA`, `candidateHasB`, and `candidateHasBoth`
- `skillsInDemand` — candidate's skills that appear in the top demand list (strengths)
- `skillGaps` — top demand skills the candidate does not have, ordered by market frequency (learning priorities)

---

## Excel Reports

### Company report (`GET /api/company/report`)

Two-sheet `.xlsx` file:

| Sheet | Content |
|---|---|
| My Offers | All company offers with status, payment date, tier, salary, modality |
| Candidate Pipeline | All matches with candidate info, score, stage, and test result |

### Admin report (`GET /api/admin/report`)

Three-sheet `.xlsx` file:

| Sheet | Content |
|---|---|
| Platform Summary | Total users, offers, matches, revenue, conversion rates |
| Companies | All company accounts with offer counts and payment totals |
| Payments | All completed payments with amounts, dates, and linked offers |

Both reports are generated by `ReportService` using **ClosedXML** and returned as `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` with appropriate `Content-Disposition` headers.

---

## Rate Limiting

In-memory rate limiting using ASP.NET Core's built-in `RateLimiter`:

| Policy | Limit | Applied to |
|---|---|---|
| `auth-strict` | 5 requests / minute per IP | `register`, `login`, `forgot-password`, `reset-password` |
| `auth-general` | 15 requests / minute per IP | `verify-email`, `refresh`, `google` |
| `payment` | 5 requests / 5 minutes per userId or IP | `create-checkout` |

When a limit is exceeded the API returns `HTTP 429 Too Many Requests`. The client should wait at least 60 seconds before retrying a rate-limited endpoint.

---

## Getting Started

### Prerequisites

| Requirement | Version |
|---|---|
| .NET SDK | 10.0+ |
| PostgreSQL | 15+ |
| OpenAI API Key | GPT-4o-mini access required |
| Wompi account | Sandbox keys for development |
| SMTP credentials | Gmail app password or equivalent |
| Google Cloud project | OAuth 2.0 Client ID for Web |

### 1. Clone the repository

```bash
git clone <repository-url>
cd MatchIQ
```

### 2. Create the database

Connect to your PostgreSQL instance and run the full schema script:

```bash
psql -U postgres -d your_database_name -f matchiq_db_complete.sql
```

Then apply the incremental migrations in order:

```bash
psql -U postgres -d your_database_name -f fix_sql_case.sql
psql -U postgres -d your_database_name -f add_test_deadline_days.sql
psql -U postgres -d your_database_name -f add_proctoring_tables.sql
psql -U postgres -d your_database_name -f fix_null_experience_years.sql
```

### 3. Configure the application

Copy the template below into `MatchIQ.API/appsettings.Development.json` and fill in your values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=matchiq;Username=postgres;Password=your_password"
  },
  "Jwt": {
    "Key": "your-256-bit-secret-key-at-least-32-chars",
    "Issuer": "MatchIQ",
    "Audience": "MatchIQ",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "gpt-4o-mini"
  },
  "Google": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-google-client-secret"
  },
  "Wompi": {
    "PublicKey": "pub_test_...",
    "PrivateKey": "prv_test_...",
    "EventsSecret": "test_events_...",
    "BaseUrl": "https://sandbox.wompi.co/v1",
    "RedirectUrl": "http://localhost:YOUR_FLUTTER_PORT/payment-result"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromName": "MatchIQ"
  },
  "App": {
    "FrontendUrl": "http://localhost:YOUR_FLUTTER_PORT"
  }
}
```

> **Gmail setup:** Enable 2-factor authentication on your Google account, then generate an App Password at [myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords). Use that 16-character password as `Email.Password`.

### 4. Run the API

```bash
cd MatchIQ.API
dotnet run
```

The API starts on `http://localhost:5000`. Swagger UI is available at the root: `http://localhost:5000/`.

### 5. Create the first admin user

Use Swagger or any HTTP client to call:

```http
POST /api/auth/register
Content-Type: application/json

{
  "fullName": "Admin Name",
  "email": "admin@matchiq.co",
  "password": "StrongPassword123!",
  "role": "Admin"
}
```

> Alternatively use `POST /api/admin/users` with an existing admin token once the platform has at least one admin.

---

## Configuration Reference

| Key | Type | Required | Description |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | string | ✅ | PostgreSQL connection string |
| `Jwt:Key` | string | ✅ | HS256 signing key — minimum 32 characters |
| `Jwt:Issuer` | string | ✅ | JWT issuer claim (must match between API restarts) |
| `Jwt:Audience` | string | ✅ | JWT audience claim |
| `Jwt:AccessTokenExpirationMinutes` | int | ✅ | Access token lifetime in minutes |
| `Jwt:RefreshTokenExpirationDays` | int | ✅ | Refresh token lifetime in days |
| `OpenAI:ApiKey` | string | ✅ | OpenAI API key with GPT-4o-mini access |
| `OpenAI:Model` | string | ✅ | Model identifier (e.g. `gpt-4o-mini`) |
| `Google:ClientId` | string | ✅ | Google OAuth 2.0 client ID |
| `Google:ClientSecret` | string | ✅ | Google OAuth 2.0 client secret |
| `Wompi:PublicKey` | string | ✅ | Wompi public key (sandbox or production) |
| `Wompi:PrivateKey` | string | ✅ | Wompi private key |
| `Wompi:EventsSecret` | string | ✅ | Wompi events secret for webhook signature verification |
| `Wompi:BaseUrl` | string | ✅ | `https://sandbox.wompi.co/v1` (sandbox) or `https://production.wompi.co/v1` |
| `Wompi:RedirectUrl` | string | ✅ | URL Wompi redirects to after payment (Flutter frontend) |
| `Email:SmtpHost` | string | ✅ | SMTP server hostname |
| `Email:SmtpPort` | int | ✅ | SMTP port (typically 587 for TLS) |
| `Email:Username` | string | ✅ | SMTP authentication username |
| `Email:Password` | string | ✅ | SMTP authentication password or app password |
| `Email:FromName` | string | ✅ | Display name for outgoing emails |
| `App:FrontendUrl` | string | ✅ | Base URL of the Flutter frontend (used in email links) |

---

## API Response Contract

Every endpoint — including errors — returns the same JSON envelope:

```json
{ "success": true,  "data": { ... }, "message": null    }
{ "success": true,  "data": null,    "message": "Done." }
{ "success": false, "data": null,    "message": "Descriptive error message." }
```

### HTTP status codes

| Code | When |
|---|---|
| `200` | Success |
| `400` | Invalid input, business rule violation, or invalid state |
| `401` | Missing token, expired token, or insufficient role |
| `404` | Resource not found |
| `409` | State conflict requiring user confirmation before proceeding |
| `429` | Rate limit exceeded — wait before retrying |
| `500` | Unhandled server error |

Exception-to-status mapping is handled centrally by `ErrorHandlingMiddleware`:

| Exception type | HTTP status |
|---|---|
| `InvalidOperationException` | 400 |
| `UnauthorizedAccessException` | 401 |
| `KeyNotFoundException` | 404 |
| `NotImplementedException` | 501 |
| Any other | 500 |

---

## Frontend Documentation

Detailed implementation guides for the Flutter frontend are available in the following files:

| File | Contents |
|---|---|
| `API_REFERENCE.md` | Complete API reference: all endpoints, request/response shapes, enum values |
| `ANALYTICS_FRONTEND.md` | Market analytics endpoints: field-by-field explanation, Flutter implementation flow |
| `COMPANY_FRONTEND.md` | Company-specific endpoints: dashboard metrics, offer management, matching |
| `ADMIN_FRONTEND.md` | Admin panel endpoints: user management, platform stats |
| `PROCTORING_FRONTEND.md` | Proctoring integration: session lifecycle, event types, report structure |

---

*MatchIQ — Built with .NET 10, PostgreSQL 15, and GPT-4o-mini.*