#  GameVault

GameVault is an ASP.NET Core Web API built with .NET 10. It provides a backend for managing personal game libraries, custom game lists, and player statuses while serving as a learning project focused on modern backend development practices.

⚠️ This project is currently on hold.

I originally built this project to practice ASP.NET Core Web API development and backend architecture. Due to other commitments, development is currently paused.

The project is open-source, and contributions, suggestions, and feedback are welcome.


## 🚀 Features

- **User Authentication**: Secure JWT-based authentication (Login, Register).
- **Game Management**: Integration with game databases to manage games, statuses, and custom lists. (currently using RAWG)
- **Repository Pattern & Unit of Work**: Clean separation of concerns for data access.
- **Entity Framework Core**: Code-first database management with EF Core.
- **Logging**: Configured with **Serilog** for robust application logging.
- **API Documentation**: Built-in Swagger/OpenAPI support for easy testing and documentation.

## 📌 Roadmap

- ✅ Structural Architecture (Repository Pattern & Unit of Work)
- ✅ Basic User Authentication (JWT, Login, Register)
- ✅ Database Schema & Code-First Migrations (using EF Core)
- ✅ Basic CRUD Operations (User accounts, basic Game logic)
- ✅ Unit Testing Infrastructure Setup
- ⏳ Complete Unit Testing Suite (Increase coverage for Services & Controllers)
- ⏳ Custom Game Lists Management (Creation, editing, and game assignment for lists)
- ⏳ User Profile & Status Tracking (Finished, Playing, Backlog tracking)
- ⏳ Refresh Token Authentication
- ⏳ Integration Tests setup
- ⏳ Docker Support & Containerization

## 🏗️ Architecture & Technologies

- **Framework**: .NET 10 / ASP.NET Core Web API
- **Database**: Entity Framework Core (SQL)
- **Authentication**: JWT (JSON Web Tokens)
- **Logging**: Serilog
- **Testing**: xUnit, Moq, FluentAssertions (`GameDBWebApi.Tests`)

## 📂 Project Structure

- `GameDBWebApi/`
  - `Controllers/`: Handles incoming HTTP requests and responses (`AuthController`, `GameController`, `UserController`).
  - `DTOs/`: Data Transfer Objects used to shape data appropriately for requests/responses.
  - `Entities/`: Database domain models (`User`, `Game`, `UserGame`, `UserList`).
  - `Data/`: `AppDbContext` and core EF Core configurations.
  - `Repositories/`: Data access logic (`UserRepository`, `UnitOfWork`).
  - `Services/`: Business logic layer (e.g., `AuthService`).
  - `GameDBWebApi.Tests/`: xUnit test project for unit testing services and controllers.

## 🤝 Contributing

Contributions are welcome.

If you'd like to improve the project, feel free to fork the repository, open an issue, or submit a pull request.

## ⚙️ Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- Visual Studio 2026 or later, Rider, or VS Code
- SQL Server (or any EF Core compatible database depending on your connection string)

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone https://github.com/atilladegirmenci/GameVault.git
   cd GameVault
   ```

2. **Configure AppSettings**
   Ensure your `appsettings.json` or `appsettings.Development.json` has the appropriate configurations for:
   - `ConnectionStrings:DefaultConnection`
   - `JwtSettings:SecretKey`, `Issuer`, and `Audience`

3. **Apply Database Migrations**
   Open your terminal in the `GameDBWebApi` project folder and update your database:
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**
   Start the API project:
   ```bash
   dotnet run --project GameDBWebApi
   ```
   Navigate to Swagger UI to test the endpoints: `https://localhost:<port>/swagger`

## 🧪 Testing

The solution includes a test project `GameDBWebApi.Tests` using xUnit and Moq.
To run the test suite, use the following command:
```bash
dotnet test
```

## 📝 Logging

The application logs errors, warnings, and system information to the file system (e.g., `Logs/gamevault-log-{Date}.txt`) using Serilog. Ensure the application has write access to the logging directory specified in the configuration.
