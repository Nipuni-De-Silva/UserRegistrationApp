# User Registration Using Blazor and ASP.NET Core with Entity Framework

### Secure user registration application built with ASP.NET Core Blazor Server and ASP.NET Core Identity.

### Technology Srack
#### Backend: ASP.NET Core 9.0
#### Frontend: Blazor Server Compnent
#### Database: SQLite with Entity Framework Core
#### Authentication: ASP.NET Core Identity

### Updated the User Registration App to Note Taking App called as My Note Space where user registered users can add new notes, edit existing notes, delete notes and categorize notes

### My Note Space

![Home](Screenshots/home.PNG)
![Registration Form](Screenshots/register.PNG)
![Login Form](Screenshots/login.PNG)
![Dashboard](Screenshots/dashboard-new.PNG)

### Features

1. Note Taking App using ASP.NET Core 9.0 Blazor Server Application with real-time updates
2. Entity Framework Core with SQLite database for data storage
3. ASP.NET Core Identity authentication
4. User registration and login with email confrimation required
5. Create, edit, and delete notes
6. Predefined nore categories with unique color coding
7. Category filtering to organize and find notes quickly
8. Full-screen responsive dashboard with three panel layout

# Clone and Run the Project

## 1. Clone the Repository
```bash
git clone https://github.com/Nipuni-De-Silva/UserRegistrationApp.git
```

```bash
cd UserRegistrationApp
```

## 2. Restore Dependencies
```bash
dotnet restore
```

## 3. Setup Database
```bash
dotnet ef migrations add InitialCreate
```

```bash
dotnet ef database update
```

## 4. Run the Application
```bash
dotnet run
```

#### application will be available at: http://localhost:5171


## API Endpoints

```bash
curl -X POST http://localhost:5171/api/user/register -H "Content-Type: application/json" -d '{"username":"testuser","email":"test@example.com","password":"Test@123"}'
```

# Contributing

## 1. Fork and Clone

```bash
git clone https://github.com/Nipuni-De-Silva/UserRegistrationApp.git
```

```bash
cd UserRegistrationApp
```

## 2. Create a Feature Branch

```bash
git checkout -b feature/your-branch-name
```

## 3. Make Changes

## 4. Commit Changes

```bash
git add .
```

```bash
git commit -m "Add Commit Message"
```

## 5. Push and Create Pull Request

```bash
git push origin feature/your-branch-name
```
