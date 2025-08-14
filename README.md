**TeachPortal API**

TeachPortal is a .NET 8 Web API for managing teachers and students with JWT-based authentication. It exposes endpoints for signup, login, and simple teacher/student operations. Swagger is enabled for interactive exploration.

<img width="1671" height="770" alt="image" src="https://github.com/user-attachments/assets/7ce006c7-41d1-4ddb-8806-4375998a8b99" />


**Features**

JWT authentication (login/signup)

EF Core with SQL Server

Clean solution split into Models, Services, DataStore, and Web API

Swagger/OpenAPI documentation

CORS policy for a React client on http://localhost:3000

**Tech Stack**

.NET 8, ASP.NET Core Web API

Entity Framework Core 8 (SQL Server provider)

Microsoft.IdentityModel.Tokens, System.IdentityModel.Tokens.Jwt

Swashbuckle.AspNetCore 6.6.x

**Solution Layout**
```text
TeachPortal.sln
│
├─ TeachPortal                  # Web API (controllers, Program.cs, appsettings)
│  ├─ Controllers
│  │   ├─ AuthController.cs
│  │   ├─ StudentController.cs
│  │   └─ TeacherController.cs
│  ├─ appsettings.json
│  └─ appsettings.Development.json
│
├─ TeachPortal.DataStore        # EF Core DbContext and migrations
│  └─ AppDbContext.cs
│
├─ TechPortal.Services          # Business logic and JWT issuance
│  ├─ AuthService.cs
│  ├─ StudentService.cs
│  └─ TeacherService.cs
│
└─ TechPortal.Models            # DTOs, Interfaces, Entities
   ├─ Interfaces
   │   ├─ IAuthService.cs
   │   ├─ IStudentService.cs
   │   └─ ITeacherService.cs
   └─ Models
       ├─ LoginRequest.cs
       ├─ Result.cs
       ├─ Student.cs
       ├─ Teacher.cs
       └─ TeacherOverview.cs
```
**Prerequisites**

.NET SDK 8.x

SQL Server (LocalDB, SQL Express, or full SQL Server)

EF Core tools:

dotnet tool update --global dotnet-ef

**Configuration**

Edit TeachPortal/appsettings.Development.json (or appsettings.json if you prefer one file):

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TeachPortal;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "o8d7KZ0q1T9w2nQe4rYu6xF9pS3tV0mB",
    "Issuer": "TechPortal",
    "Audience": "https://localhost:5001",
    "ExpiryMinutes": 480
  },
  "AllowedHosts": "*"
}


Notes:

For SQL Express: Server=localhost\\SQLEXPRESS;Database=TeachPortal;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;

For full SQL Server with SQL auth: Server=localhost;Database=TeachPortal;User Id=sa;Password=...;Encrypt=True;TrustServerCertificate=True;

TrustServerCertificate=True is fine for local development. For production, install a trusted cert on SQL Server and remove that flag.

Ensure you are running in Development so the Development settings are used:

Properties/launchSettings.json → "ASPNETCORE_ENVIRONMENT": "Development"

**Database**

Create the database schema using EF migrations.

From the TeachPortal project directory (or the solution root with -s):

if you haven't created any migrations yet
dotnet ef migrations add Init -p TeachPortal.DataStore -s TeachPortal

apply migrations to the database
dotnet ef database update -p TeachPortal.DataStore -s TeachPortal

If you prefer automatic application at startup, you can add:

// Program.cs after var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

**Run**

From the TeachPortal project:

dotnet restore
dotnet run


The API will start on the ports in launchSettings.json. Swagger UI will be available at:

https://localhost:<port>/swagger

**CORS**

Program.cs defines a CORS policy named AllowReact allowing http://localhost:3000:

builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowReact", p =>
        p.WithOrigins("http://localhost:3000")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

**Authentication**

POST /api/auth/signup
Create a teacher. Server hashes the password.

POST /api/auth/login
Returns the token

The token contains claims such as sub (teacher id), name, email, and jti. Send it as:

Authorization: Bearer <jwt>

**Notes for a React Client**

Base URL: https://localhost:<api-port>/api

Add Authorization: Bearer <token> header after a successful login.

Keep the CORS origin aligned with your dev client URL (default is http://localhost:3000 in this template).
