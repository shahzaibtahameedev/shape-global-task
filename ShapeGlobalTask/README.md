### Three-Tier Architecture

This application uses a clean three-tier architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                    CONTROLLER LAYER                         │
│  (UsersController.cs)                                       │
│  - Handles HTTP requests (GET, POST, PUT, DELETE)           │
│  - Validates input using Data Annotations                   │
│  - Returns appropriate HTTP responses                       │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     SERVICE LAYER                           │
│  (UserService.cs)                                           │
│  - Contains business logic                                  │
│  - Validates business rules (e.g., no duplicate emails)     │
│  - Coordinates between controllers and repositories         │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    REPOSITORY LAYER                         │
│  (JsonUserRepository.cs)                                    │
│  - Handles data persistence                                 │
│  - Reads/writes to JSON file                                │
│  - Thread safe with SemaphoreSlim                           │
└─────────────────────────────────────────────────────────────┘
```

---

## 🚀 Quick Start (For Development)

When developing, you can run it like a normal console application:

### Step 1: Open Terminal

Open PowerShell or Command Prompt in the project folder:

```
cd "c:\Users\Shahzaib\Desktop\Repositories\.net self hosted api\ShapeGlobalTask\ShapeGlobalTask"
```

### Step 2: Run the Application

```powershell
dotnet run
```

### Step 3: Test the API

Open your browser and go to:

- **Swagger UI**: http://localhost:5046/swagger
- **Health Check**: http://localhost:5046/health
- **All Users**: http://localhost:5046/api/users

### Step 4: Stop the Application

Press `Ctrl+C` in the terminal.

---

## 🪟 Installing as a Windows Service (Production)

This is the main purpose of this application - running as a real Windows Service!

### Prerequisites

- Windows 10/11 or Windows Server
- .NET 8.0 Runtime installed
- Administrator access