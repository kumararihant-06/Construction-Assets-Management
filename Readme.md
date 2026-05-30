# Construction Asset Management API

A REST API for tracking heavy equipment, job sites, and equipment-to-site assignments across a construction operation. Built as a single ASP.NET Core 10 project using minimal APIs.

## Features

- JWT-based authentication with three roles (Admin / Manager / Operator)
- Full CRUD for equipment, job sites, and assignments
- Double-booking detection — equipment cannot be assigned to two open sites at once
- Soft-return on assignment delete (history is preserved)
- Maintenance-due query (equipment due within 7 days)
- Global exception handler returning RFC 7807 ProblemDetails with correlation IDs on 500s
- Swagger UI with JWT Authorize button

## Tech stack

| Layer | Choice |
|---|---|
| Runtime | .NET 10 |
| Web | ASP.NET Core 10, Minimal APIs |
| Data | EF Core 10 + SQLite (`construction.db`) |
| Identity | ASP.NET Identity, `IdentityUser<Guid>` |
| Auth | JWT Bearer |
| Validation | FluentValidation |
| OpenAPI | Swashbuckle 6.9.0 |

## Architecture
```
Endpoints  →  Services  →  AppDbContext (EF Core)
↓
Typed exceptions (NotFound / Conflict / Forbidden)
↓
ExceptionHandlingMiddleware  →  ProblemDetails JSON
```

Endpoints are thin dispatchers. All business logic — FK existence checks, double-booking detection, status transitions — lives in `Services/`. Services throw typed exceptions; the middleware maps them to HTTP status codes.

## Role matrix

| Capability | Admin | Manager | Operator |
|---|:-:|:-:|:-:|
| Register new users | ✅ | ❌ | ❌ |
| Login | ✅ | ✅ | ✅ |
| List / view equipment, sites, assignments | ✅ | ✅ | ✅ |
| Create / update equipment | ✅ | ✅ | ❌ |
| Create / update job sites | ✅ | ✅ | ❌ |
| Create assignment / soft-return assignment | ✅ | ✅ | ❌ |
| View maintenance-due equipment | ✅ | ✅ | ❌ |
| Patch equipment status | ✅ | ✅ | ✅ |
| Delete equipment | ✅ | ❌ | ❌ |
| Delete job site | ✅ | ❌ | ❌ |

## Endpoint matrix

All routes prefixed `/api/v1`. Auth column indicates the policy required.

### Auth
| Method | Route | Auth | Purpose |
|---|---|---|---|
| POST | `/auth/login` | Public | Exchange credentials for JWT |
| POST | `/auth/register` | Admin | Create a new user with a role |

### Equipment
| Method | Route | Auth | Purpose |
|---|---|---|---|
| GET | `/equipments` | Any authed | List all equipment |
| GET | `/equipments/{id}` | Any authed | Get one |
| POST | `/equipments` | Manager+ | Create |
| PUT | `/equipments/{id}` | Manager+ | Update |
| DELETE | `/equipments/{id}` | Admin | Delete |
| GET | `/equipments/available` | Any authed | Only `Available` equipment |
| GET | `/equipments/maintenance-due` | Manager+ | Due within 7 days |
| PATCH | `/equipments/{id}/status` | Any authed | Flip status |

### Job sites
| Method | Route | Auth | Purpose |
|---|---|---|---|
| GET | `/jobsites` | Any authed | List |
| GET | `/jobsites/{id}` | Any authed | Get one |
| POST | `/jobsites` | Manager+ | Create |
| PUT | `/jobsites/{id}` | Manager+ | Update |
| DELETE | `/jobsites/{id}` | Admin | Delete |

### Assignments
| Method | Route | Auth | Purpose |
|---|---|---|---|
| GET | `/assignments` | Any authed | List with equipment + site names |
| POST | `/assignments` | Manager+ | Create (rejects double-booking with 409) |
| DELETE | `/assignments/{id}` | Manager+ | Soft return (sets `ReturnDate`, frees equipment) |

## Error responses

All errors come back as `application/problem+json`:

| Status | Trigger |
|---|---|
| 400 | FluentValidation failure on request body |
| 401 | Missing / invalid JWT |
| 403 | Policy not satisfied, or `ForbiddenException` from service |
| 404 | `NotFoundException` (missing equipment, site, assignment, or FK target) |
| 409 | `ConflictException` (double-booking, already-returned assignment, duplicate user) |
| 500 | Unhandled — includes `correlationId` in body and `X-Correlation-Id` header |

## Running locally

Prerequisites: .NET 10 SDK.

```bash
git clone <repo-url>
cd ConstructionAssetAPI

# Restore + build
dotnet restore
dotnet build

# Apply migrations (creates construction.db)
dotnet ef database update

# Run
dotnet run
```

The DB is auto-seeded on startup with three roles and a bootstrap admin.

### Bootstrap credentials
```
Email:    admin@local.com
Password: Admin@123
```
Login once, copy the `accessToken` from the response, then in Swagger UI click **Authorize** and paste it (no `Bearer ` prefix needed).

### Swagger

Once running, open: <http://localhost:5000/swagger> (port may differ — check console output or `launchSettings.json`).

## Project layout
```
ConstructionAssetAPI/
├── Auth/              JWT settings + token generator
├── Data/              AppDbContext + DbSeeder
├── DTOs/              (reserved for future shared DTOs)
├── Endpoints/         Minimal-API route groups + input/output records
├── Entities/          Equipment, JobSite, Assignment, ApplicationUser
├── Enums/             EquipmentStatus, UserRole
├── Exceptions/        NotFound / Conflict / Forbidden
├── Middleware/        ExceptionHandlingMiddleware
├── Migrations/        EF Core migrations
├── Services/          Business logic (Auth, Equipment, JobSite, Assignment)
├── Validators/        FluentValidation rules
└── Program.cs
```