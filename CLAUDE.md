# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Solution is at FinanceTracker/FinanceTracker.slnx
dotnet build FinanceTracker/FinanceTracker.slnx
dotnet run --project FinanceTracker/FinanceTracker.API

# Tests (xUnit + FluentAssertions + Moq)
dotnet test FinanceTracker/FinanceTracker.Tests
dotnet test FinanceTracker/FinanceTracker.Tests --filter "FullyQualifiedName~ClassName.MethodName"

# EF Core migrations (run from FinanceTracker/FinanceTracker.API)
dotnet ef migrations add <MigrationName> --project ../FinanceTracker.Infrastructure --startup-project .
dotnet ef database update --project ../FinanceTracker.Infrastructure --startup-project .

# Frontend (React + Vite)
cd frontend && npm run dev
```

## Architecture

Clean Architecture with four layers. Dependencies flow inward: API → Application → Domain; Infrastructure → Domain.

- **FinanceTracker.Domain** — Entities (`User`, `Account`, `Transactions`, `SalaryCycle`, `SalaryDistribution`, `Expense`), enums, and repository interfaces. No external dependencies.
- **FinanceTracker.Application** — Service interfaces/implementations, DTOs, FluentValidation validators, AutoMapper profiles. Contains `Result<T>` wrapper for standardized responses.
- **FinanceTracker.Infrastructure** — EF Core `DataContext`, repository implementations, Unit of Work pattern (`IUnitOfWork`/`UnitOfWork` with transaction support), `CurrentUserService`.
- **FinanceTracker.API** — ASP.NET Core Web API controllers, `ExceptionMiddleware`, Program.cs startup. Routes follow `api/[controller]` convention.

### Key Patterns

- **Repository + Unit of Work**: Generic `IRepository<T>` base with specialized repositories. `IUnitOfWork` coordinates saves and transactions (`BeginTransactionAsync`/`CommitTransactionAsync`/`RollbackTransactionAsync`).
- **Result<T> wrapper**: All controller endpoints wrap responses in `Result<T>.Success()` or `Result<T>.Created()` from `Application/DTOs/Common/Result.cs`. This matches the frontend `ApiResult<T>` type (`{ isSuccess, data, error, statusCode }`). AuthController uses `Result<T>` from service layer directly. Delete/NoContent endpoints are the exception (no body).
- **DI registration**: Each layer has a `DependencyInjection.cs` extension method (`AddApplicationServices`, `AddInfrastructureServices`) called from Program.cs.
- **JWT auth**: Configured in Program.cs, token generation in `AuthService`. Claims use `NameIdentifier` for user ID.
- **AutoMapper**: Profile at `Application/Mappings/MappingProfile.cs` — uses `ForCtorParam()` for constructor-mapped DTOs.

## Tech Stack

- .NET 10.0, C# with nullable reference types enabled
- SQL Server (LocalDB/SQLEXPRESS), EF Core 10.0.2
- JWT Bearer authentication, BCrypt password hashing
- CORS configured for React frontend at `http://localhost:5173`
- Swagger/OpenAPI enabled

## Frontend

- React 19 + TypeScript + Vite (dev server on port 5173)
- Zustand for auth state (persisted to localStorage via `persist` middleware)
- TanStack React Query for server state
- Tailwind CSS + shadcn/ui components
- Vite proxy forwards `/api` requests to backend at `http://localhost:5142`
- All API hooks expect `ApiResult<T>` wrapper (`{ isSuccess, data, error, statusCode }`) — defined in `frontend/src/types/api.ts`
- Auth flow: login → store JWT in Zustand → axios interceptor attaches `Authorization: Bearer` header → `ProtectedRoute` guards dashboard routes (waits for store hydration before checking token)

## Database

Connection string in `appsettings.Development.json` targets `localhost\SQLEXPRESS` database `financetrackerDB` with Windows authentication.

## Project Status

All core feature controllers and services are implemented with comprehensive test coverage. React frontend is connected with full auth flow.

### Implemented Controllers & Endpoints
- **AuthController** — `POST login`, `POST register` (returns `Result<T>` from service)
- **AccountsController** — CRUD + `GET total-balance` (Authorized, all wrapped in `Result<T>`)
- **TransactionsController** — filtered listing with paging, CRUD, by-account lookup (Authorized, all wrapped in `Result<T>`)
- **SalaryCyclesController** — recent cycles, create, execute distributions, next-payday (Authorized, all wrapped in `Result<T>`)
- **DashboardController** — `GET get-dashboard`, balances, salary countdown, MTD expenses (Authorized, all wrapped in `Result<T>`)
- **ExpensesController** — listing, create, monthly summary, delete (all wrapped in `Result<T>`)

### Implemented Services
- `AuthService`, `AccountService`, `TransactionService`, `ExpenseService`, `SalaryCycleService`, `DashboardService`

### Frontend Pages & Hooks
- **Pages**: `LoginPage`, `RegisterPage`, `DashboardPage`, plus account/transaction/expense/salary-cycle pages
- **Hooks**: `useLogin`, `useRegister`, `useDashboard`, `useAccounts`, `useTransactions`, `useExpenses`, `useRecentCycles`, etc.
- **Stores**: `auth-store.ts` (Zustand + persist) — stores `token`, `user`, `_hasHydrated`

### Test Coverage (~150 tests)

Tests use **xUnit** `[Fact]`, **Moq** for mocking, **FluentAssertions** for assertions.

#### Integration Tests (`IntegrationTests/Repositories/`)
- `AccountRepositoryTests`, `TransactionRepositoryTests`, `ExpenseRepositoryTests`, `SalaryCycleRepositoryTests`, `UserRepositoryTests` — all use EF Core InMemory via `TestHelpers.CreateInMemoryContext()`

#### Unit Tests — Services (`UnitTests/Services/`)
- `AccountServiceTests`, `AuthServiceTests`, `TransactionServiceTests`, `ExpenseServiceTests`, `SalaryCycleServiceTests`, `DashboardServiceTests`
- Pattern: `Mock<IUnitOfWork>`, real `IMapper` via `MapperConfiguration` with `MappingProfile`, service as SUT
- `AuthServiceTests` uses `ConfigurationBuilder.AddInMemoryCollection` for JWT config
- `DashboardServiceTests` mocks `IExpenseService` and `ISalaryCycleService` (not repos directly)

#### Unit Tests — Controllers (`UnitTests/Controllers/`)
- `AuthControllerTests`, `AccountsControllerTests`, `TransactionsControllerTests`, `SalaryCyclesControllerTests`, `DashboardControllerTests`, `ExpensesControllerTests`
- Pattern: `Mock<IServiceInterface>`, `TestHelpers.SetupControllerContext()` for ClaimsPrincipal
- Controller tests assert on `Result<T>` wrapper (e.g., `BeOfType<Result<AccountDto>>()`, check `IsSuccess` and `Data`)
- Naming convention: `MethodName_ExpectedResult_WhenCondition`

### Test Helpers (`Helpers/TestHelpers.cs`)
- `CreateInMemoryContext()` — EF Core InMemory `DataContext`
- `SetupControllerContext()` — sets `ClaimsPrincipal` with `NameIdentifier` claim on controller
- Factory methods: `CreateTestUser`, `CreateTestAccount`, `CreateTestTransaction`, `CreateTestExpense`, `CreateTestSalaryCycle`
