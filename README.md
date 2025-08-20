# User Registra+on Using Blazor and ASP.NET Core with En+ty
Framework


## Section A

1. Create new Blazor App 

    ```bash
    dotnet new blazorserver -n UserRegistrationApp
    ```

2. Entity Framework Integration with SQLite

    ```bash
    dotnet add package Microsoft.EntityFrameworkCore.Sqlite
    ```

    ```bash
    dotnet add package Microsoft.EntityFrameworkCore.Tools
    ```

    ```bash
    dotnet add package Microsoft.EntityFrameworkCore.Design
    ```

    Here I choose SQLite, because of it's lightweight and development-friendly database 

3. Database configuration done by adding connection string to appsettings.json

    ```bash
    "ConnectionStrings": {
        "DefaultConnection": "Data Source=UserRegistrationApp.db"
    }
    ```

4. Entity Framework configuration in Program.cs

    ```bash
    builder.Services.AddDbContext<ApplicationDbContext>(options => { options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));});
    ```

