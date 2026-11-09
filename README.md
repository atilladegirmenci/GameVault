# 🎮 GameVault

**GameVault** is an open-source game library management Web API built with **ASP.NET Core and .NET 10**. It allows users to manage their personal game collections, track their gaming progress, write reviews, and create custom game lists using data from the [IGDB Video Games Database API](https://api-docs.igdb.com/).

This project is both a functional backend application and an ongoing learning project focused on modern .NET development, RESTful API design, database management, authentication, and maintainable software architecture.

I'm open to **contributions, suggestions, feedback, and collaboration**. Whether you want to report a bug, suggest a feature, improve the codebase, or contribute directly, you're welcome to get involved!

## ✨ Features

### 🔐 Authentication and User Management
- User registration and login with JWT-based authentication.
- Email verification with time-limited, single-use verification links.
- Password hashing using BCrypt.
- Refresh token management with hashed token storage.
- User account management.

### 🎮 Game Library Management
- Integration with IGDB to retrieve video game information through its Apicalypse query API.
- Dynamic Twitch OAuth client-credentials access tokens for IGDB requests.
- Search and retrieve game details.
- Manage a personal game library.
- Track game statuses, including playing, completed, and planned-to-play games.

### 📚 Custom Game Lists
- Create, update, and delete custom game lists.
- Add games to and remove games from lists.
- Retrieve user lists and view the games they contain.
- Enforce list ownership and prevent duplicate game entries.

### ⭐ Game Reviews
- Create and delete game reviews.
- Retrieve reviews for a specific game.
- Retrieve reviews written by a particular user.
- Paginated review endpoints.
- Input validation and ownership checks.

### 🛠️ Backend Engineering
- RESTful API architecture using ASP.NET Core.
- Repository Pattern and Unit of Work for data access.
- Entity Framework Core with code-first migrations.
- Structured application logging with Serilog.
- Centralized exception handling and custom exceptions.
- DTO-based request and response models.
- Swagger/OpenAPI documentation.
- Unit testing with xUnit, Moq, and FluentAssertions.

## 🧰 Tech Stack

| Technology | Purpose |
|---|---|
| C# / .NET 10 | Backend development |
| ASP.NET Core Web API | REST API |
| Entity Framework Core | ORM and database management |
| SQL Server | Relational database |
| JWT | Authentication |
| BCrypt.Net-Next | Password hashing |
| Serilog | Application logging |
| IGDB API | Video game data |
| Twitch OAuth | IGDB access-token authentication |
| Swagger / OpenAPI | API documentation |
| xUnit, Moq, FluentAssertions | Unit testing |

## 🏗️ Architecture

The application follows a layered architecture to separate HTTP handling, business logic, and data access.

- **Controllers:** Handle HTTP requests and responses.
- **Services:** Implement application and business logic.
- **Repositories:** Encapsulate data access operations.
- **Unit of Work:** Coordinates repository operations and database persistence.
- **DTOs:** Define API request and response contracts.
- **Entities:** Represent the application's domain models.
- **Data:** Contains `AppDbContext` and Entity Framework Core configurations.

This structure aims to keep the codebase maintainable, testable, and easier to extend as new features are introduced.

## 📂 Project Structure

```text
GameVault/
├── GameDBWebApi/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Entities/
│   ├── Data/
│   ├── Repositories/
│   ├── Services/
│   └── ...
└── GameDBWebApi.Tests/
```

*Note: Folder and namespace names may vary slightly in the current implementation.*

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server
- A Twitch developer application with an IGDB `ClientId` and `ClientSecret`
- Visual Studio, Rider, or VS Code (optional)

### 1. Clone the repository

```bash
git clone https://github.com/atilladegirmenci/GameVault.git
cd GameVault
```

### 2. Configure application settings

Configure your database connection string, JWT settings, and IGDB/Twitch credentials using User Secrets, environment variables, or local development configuration.

Required settings include:

- `ConnectionStrings:DefaultConnection`
- `JwtSettings:SecretKey`
- `JwtSettings:Issuer`
- `JwtSettings:Audience`
- `IGDB:ClientId`
- `IGDB:ClientSecret`
- `EmailSettings:SmtpHost`
- `EmailSettings:SmtpPort`
- `EmailSettings:Username`
- `EmailSettings:Password`
- `EmailSettings:FromAddress`
- `EmailSettings:FromName`
- `EmailSettings:VerificationUrl`
- `EmailSettings:VerificationTokenLifetimeMinutes`

Configure SMTP credentials with User Secrets, environment variables, or a managed secret store.
**Do not commit real API keys, JWT secrets, SMTP passwords, or other credentials to source control.**

For local development, User Secrets can be configured with:

```bash
dotnet user-secrets set "IGDB:ClientId" "<your-twitch-client-id>" --project GameDBWebApi
dotnet user-secrets set "IGDB:ClientSecret" "<your-twitch-client-secret>" --project GameDBWebApi
```

The application requests Twitch access tokens dynamically and caches them until they are
close to expiration. The token is then sent with the `Client-ID` header and bearer
authorization required by IGDB.

The active catalog implementation is `IGDBApiService`, registered behind
`IGameCatalogService`. `RawgApiService` remains available as an alternative adapter for
future provider switching, but it is not the active catalog provider.

### 3. Apply database migrations

From the solution directory, run:

```bash
dotnet ef database update --project GameDBWebApi
```

Ensure the required EF Core tooling and database configuration are available. If your project uses a separate startup project, specify it with `--startup-project`.

### 4. Run the API

```bash
dotnet run --project GameDBWebApi
```

Open the Swagger UI at the URL printed in the terminal, usually:

```text
https://localhost:<port>/swagger
```

Use Swagger to explore and test the available endpoints.

## 🧪 Running Tests

Run the test suite from the solution directory:

```bash
dotnet test
```

The test project uses xUnit, Moq, and FluentAssertions to support automated testing of application behavior.

## 🗺️ Roadmap

GameVault is actively evolving, and there are several areas where the codebase can be improved further.

Potential and planned improvements include:

- Expanding unit and integration test coverage.
- Reviewing DTO and entity validation rules.
- Reviewing transaction boundaries and concurrency handling.
- Auditing database indexes and query performance.
- Improving IGDB API resilience with retries, circuit breakers, and caching.
- Enhancing structured logging with correlation IDs and security audit events.
- Centralizing error messages and improving consistency.
- Exploring Docker support and containerization.

Progress and proposed improvements are tracked through [GitHub Issues](https://github.com/atilladegirmenci/GameVault/issues).

## 🤝 Contributing

Contributions and constructive feedback are welcome!

If you're interested in improving GameVault, you can:

- **Report bugs** by opening an issue.
- **Suggest features** or improvements.
- **Discuss architectural decisions** and share technical feedback.
- **Submit pull requests** for bug fixes, features, documentation, or tests.

For larger changes, consider opening an issue first so we can discuss the proposed approach before implementation.

Whether you're interested in .NET development, backend architecture, database design, testing, or open-source collaboration, your input is appreciated.

## 📄 License

Please check the repository for license information. If no license has been added, the project's reuse and contribution terms should be clarified before others use or distribute the code.

---

**Built with C#, ASP.NET Core, and a passion for backend development.**
