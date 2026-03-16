# TeachPortal — Backend API

A RESTful API for the TeachPortal application, built with ASP.NET Core 8 and Entity Framework Core. Handles teacher authentication, student management, and teacher overview data.

## Features

- **JWT authentication** — BCrypt password hashing on signup; signed JWT issued on login with configurable expiry
- **Teacher registration and login** — Full signup/login flow with proper HTTP status codes (201, 401, 409)
- **Student management** — Authenticated teachers can add and retrieve their own students
- **Teacher overview** — Aggregated teacher data including student counts, exposed to authorised users
- **Role-based access control** — Teachers can only access their own student data; Admin role can access any teacher's students
- **Swagger UI** — Interactive API documentation available in Development with Bearer token support

## Tech Stack

| | |
|---|---|
| Framework | ASP.NET Core 8 |
| ORM | Entity Framework Core 9 (SQL Server) |
| Auth | JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer) |
| Password Hashing | BCrypt.Net-Next |
| API Docs | Swashbuckle / Swagger with Annotations |

## Solution Structure

```
TeachPortal.sln
├── TeachPortal/                  # Web API host — controllers, middleware, Program.cs
│   └── Controllers/
│       ├── AuthController.cs     # POST /api/auth/signup, POST /api/auth/login
│       ├── StudentController.cs  # GET/POST /api/students  [Authorize]
│       └── TeacherController.cs  # GET /api/teacher, GET /api/teacher/{id}/students  [Authorize]
├── TeachPortal.Services/         # Business logic
│   ├── AuthService.cs            # Signup, login, JWT generation
│   ├── StudentService.cs         # Create and retrieve students
│   └── TeacherService.cs         # Teacher overview projection
├── TeachPortal.DataStore/        # EF Core DbContext and model configuration
│   └── AppDbContext.cs           # Fluent API — indexes, relationships, cascade delete
└── TechPortal.Models/            # Shared models and service interfaces
    ├── Models/                   # Teacher, Student, LoginRequest, Result<T>, TeacherOverview
    └── Interfaces/               # IAuthService, IStudentService, ITeacherService
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (local or remote)

### Configuration

Add the following to `appsettings.json` (or use `dotnet user-secrets` for local development):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=TeachPortalDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters",
    "Issuer": "TeachPortalAPI",
    "Audience": "TeachPortalClient",
    "ExpiryMinutes": "60"
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:3000" ]
  }
}
```

### Run the API

```bash
git clone https://github.com/nivigot/TeachPortal.git
cd TeachPortal
dotnet restore
dotnet ef database update --project TeachPortal.DataStore --startup-project TeachPortal
dotnet run --project TeachPortal
```

API runs at `https://localhost:7251`. Swagger UI is available at `/swagger` in Development.

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/signup` | None | Register a new teacher |
| POST | `/api/auth/login` | None | Login and receive a JWT |
| POST | `/api/students` | Bearer | Add a student to the authenticated teacher |
| GET | `/api/students` | Bearer | Get all students for the authenticated teacher |
| GET | `/api/teacher` | Bearer | Get all teachers with student counts |
| GET | `/api/teacher/{id}/students` | Bearer | Get students for a specific teacher (self or Admin) |

## Architecture Notes

**Layered architecture** — The solution separates concerns across four projects: the API host, business logic (Services), data access (DataStore), and shared contracts (Models + Interfaces). Controllers depend only on interfaces, not concrete service implementations, enabling easy testing.

**Generic Result pattern** — All service methods return `Result<T>`, which carries `Success`, `Message`, `Data`, and `StatusCode`. Controllers map this directly to the appropriate HTTP response without additional branching logic.

**EF Core configuration** — Relationships, unique indexes, and cascade behaviour are all defined explicitly in `AppDbContext.OnModelCreating` using the Fluent API rather than relying solely on conventions.

## Author

Poongothai Senthurkumar — [GitHub](https://github.com/nivigot)
