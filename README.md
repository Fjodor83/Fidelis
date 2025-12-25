# Suns Fidelity System - Documentation

## Project Overview
**Suns Fidelity** is a customer loyalty and rewards management system built for **Suns Zero & Company**. It allows store managers to register customers, assign points for purchases, and manage coupons. Customers can view their digital fidelity card, check their points balance, and redeem coupons.

## Technology Stack
- **Framework**: .NET 10 (Preview)
- **Frontend**: Blazor WebAssembly (WASM)
- **Backend**: ASP.NET Core Web API
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Styling**: Bootstrap 5 + Custom CSS (Mobile-First)

---

## Architecture
The project follows a **Lean Architecture** pattern, emphasizing loose coupling and testability.

### 1. Fidelity.Shared
Contains DTOs (Data Transfer Objects) and data models used by both Client and Server.
- **DTOs**: `TransazioneResponse`, `ClienteDettaglioResponse`, `CouponDTO`, etc.
- **Models**: `Cliente`, `Transazione`, `PuntoVendita`, `Responsabile`.

### 2. Fidelity.Server
The backend API responsible for business logic and data access.
- **Controllers**: Handle HTTP requests and delegate logic to services.
    - `AuthController`: Login & Token generation.
    - `TransazioniController`: Points assignment & history.
    - `CouponsController`: Coupon CRUD & redemption (delegates to `ICouponService`).
    - `ClientiController`: Customer search & profile (delegates to `IClienteService`).
    - `PuntiVenditaController`: Store management (uses AutoMapper).
    - `ResponsabiliController`: Manager accounts (uses AutoMapper).
    - `RegistrazioneController`: Email verification & customer sign-up (delegates to `ICardGeneratorService`).
    - `AnalyticsController`: Admin dashboards stats (delegates to `IAnalyticsService`).
- **Services**: Encapsulate core business logic.
    - `TransazioneService`: Handles points calculation and email notifications.
    - `CouponService`: Manages coupon lifecycle and assignment.
    - `CardGeneratorService`: Generates digital fidelity cards (System.Drawing).
    - `EmailService`: Sends transactional emails via MailKit/MimeKit.
- **Utilities**:
    - **AutoMapper**: Standardizes object mapping (`MappingProfile.cs`).
    - **System.Drawing**: Used with `SupportedOSPlatform("windows")` for image generation.

### 3. Fidelity.Client
The Blazor WebAssembly frontend.
- **Layouts**: `MainLayout` (Admin/Manager), `CustomerLayout` (Public/Customer).
- **Pages**:
    - `Dashboard.razor`: Admin overview (Tabs for Stores, Managers, Transactions).
    - `AssegnaPunti.razor`: Manager tool to assign points.
    - `CustomerDashboard.razor`: Customer points view.
    - `MyCoupons.razor`: Customer coupon wallet.
- **Responsiveness**: Uses custom `app.css` classes (`table-mobile-cards`, `nav-tabs-scrollable`) for optimal mobile experience.

---

## Setup & Deployment

### Prerequisites
- .NET 10 SDK
- SQL Server (LocalDB or Standard)

### Configuration
Update `appsettings.json` in `Fidelity.Server`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FidelisDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "YOUR_SUPER_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "FidelityServer",
    "Audience": "FidelityClient"
  },
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "apikey",
    "Password": "..."
  },
  "AppUrl": "https://localhost:7196"
}
```

### Running Locally
1.  **Database**:
    ```powershell
    dotnet ef database update --project Fidelity.Server
    ```
2.  **Server & Client**:
    ```powershell
    dotnet run --project Fidelity.Server
    ```
    The app will launch at `https://localhost:7196` (or configured port).

### Deployment
1.  **Build**:
    ```powershell
    dotnet publish Fidelity.Server -c Release -o ./publish
    ```
2.  **Deploy**:
    - Copy the contents of `./publish` to your Windows Server / IIS or Hosting Provider (e.g., MonsterASP).
    - Ensure the database connection string in `appsettings.json` points to the production DB.

---

## Key Features & Workflows

### 1. Registration Flow
1.  Manager requests email verification for a customer via `RegistrazioneController`.
2.  System generates a token and emails a link.
3.  Customer clicks link, fills form (`CompletaRegistrazione`).
4.  System creates `Cliente`, generates unique `CodiceFidelity`, creates Digital Card (PNG), and emails it.

### 2. Points Assignment
1.  Manager scans/inputs `CodiceFidelity` and Amount (€).
2.  `TransazioneService` calculates points (1 pt / 10€ default).
3.  Transaction recorded, Client points updated.
4.  Email notification sent to customer.

### 3. Coupon System
1.  Admin creates coupons (active dates, discount type).
2.  Coupons assigned to customers (manual or automated).
3.  Customer views coupons in `MyCoupons`.
4.  Manager "burns" (redeems) coupon via API/UI.

---

## Maintenance
- **Tests**: Run `dotnet test` to execute unit tests (`Fidelity.Tests`).
- **Logs**: Console logging is enabled; check stdout/stderr on server.
- **Email**: Uses MailKit. Inspect `EmailService.cs` for template adjustments.
