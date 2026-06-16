# API Project

## Program.cs Explained

This file is the entry point of an ASP.NET Core Minimal API application. It uses top-level statements (no `Main` method or `Startup` class), which is the default style since .NET 6.

### Breakdown

1. **Host & Service Configuration**
   ```csharp
   var builder = WebApplication.CreateBuilder(args);
   ```
   Creates a preconfigured builder with default settings (logging, configuration from appsettings.json, environment variables, etc.).

2. **Swagger/OpenAPI Registration**
   ```csharp
   builder.Services.AddEndpointsApiExplorer();
   builder.Services.AddSwaggerGen();
   ```
   Registers services needed to auto-generate an OpenAPI spec from your minimal API endpoints. This gives you interactive documentation out of the box.

3. **Pipeline Configuration**
   ```csharp
   var app = builder.Build();
   ```
   Builds the application and its middleware pipeline.

   ```csharp
   if (app.Environment.IsDevelopment())
   {
       app.UseSwagger();
       app.UseSwaggerUI();
   }
   ```
   Swagger UI is only exposed in Development to avoid leaking API docs in production.

   ```csharp
   app.UseHttpsRedirection();
   ```
   Redirects HTTP requests to HTTPS — a basic security best practice to ensure traffic is encrypted.

4. **Application Startup**
   ```csharp
   await app.RunAsync();
   ```
   Starts the web server asynchronously and listens for incoming requests until shutdown is triggered (e.g., CTRL+C or SIGTERM).

### Notesd

- This is a minimal scaffold — no controllers, no endpoint logic yet. You'd add endpoints via `app.MapGet(...)`, `app.MapPost(...)`, etc.
- For production, you'd typically add authentication, CORS, rate limiting, health checks, and structured logging on top of this foundation.

## StockController — Key Code Explained

### `_context.Stocks.FirstOrDefault(x => x.Id == id)`

```csharp
var stockModel = _context.Stocks.FirstOrDefault(x => x.Id == id);
```

This line queries the database to find a single stock by its ID. Here's what each part does:

- **`_context.Stocks`** — accesses the `Stocks` table in the database through Entity Framework's `DbSet<Stock>`.
- **`.FirstOrDefault(...)`** — a LINQ method that returns the first element matching the condition, or `null` if no match is found.
- **`x => x.Id == id`** — a lambda expression (filter condition) that checks if the stock's `Id` column equals the `id` parameter received from the route.

**Behind the scenes**, Entity Framework translates this into a SQL query like:

```sql
SELECT TOP(1) * FROM Stocks WHERE Id = @id
```

**Why `FirstOrDefault` instead of `Find`?**
- `Find(id)` looks up by primary key and checks the local cache first (faster if already tracked).
- `FirstOrDefault` always queries the database and allows more complex filter conditions (e.g., filtering by multiple fields).

Both return `null` if no record is found, which is why the next line checks `if (stockModel == null)` to return a 404.

## Repository Pattern — IStockRepository vs StockRepository

### `IStockRepository` (Interface)

```csharp
public interface IStockRepository
{
    Task<List<Stock>> GetAllAsync();
}
```

**Purpose:** Defines a *contract* — it declares **what** operations are available (e.g., "get all stocks") without specifying **how** they're implemented. It lives in the `Interfaces` folder.

### `StockRepository` (Implementation)

```csharp
public class StockRepository : IStockRepository
{
    private readonly ApplicationDBContext _context;

    public Task<List<Stock>> GetAllAsync()
    {
        return _context.Stocks.ToListAsync();
    }
}
```

**Purpose:** Provides the *actual implementation* — it contains the **how** (query the database using Entity Framework). It lives in the `Repository` folder.

### Key Differences

| | IStockRepository | StockRepository |
|---|---|---|
| **Type** | Interface | Class |
| **Role** | Defines the contract (what) | Implements the logic (how) |
| **Contains** | Method signatures only | Actual database queries |
| **Depends on** | Nothing (pure abstraction) | ApplicationDBContext (EF Core) |

### Why This Pattern?

1. **Loose coupling** — The controller depends on `IStockRepository`, not `StockRepository`. It doesn't know or care how data is fetched.
2. **Testability** — In unit tests, you can create a fake/mock implementation of `IStockRepository` without needing a real database.
3. **Swappability** — You can replace the implementation (e.g., switch from SQL Server to MongoDB) without changing the controller code.
4. **Separation of concerns** — Controllers handle HTTP logic, repositories handle data access.

### How They Connect (Dependency Injection)

In `Program.cs`, you register the implementation:

```csharp
builder.Services.AddScoped<IStockRepository, StockRepository>();
```

This tells the DI container: "When someone asks for `IStockRepository`, give them a `StockRepository` instance."
