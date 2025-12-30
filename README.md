# Suns Fidelity System

![Build Status](https://img.shields.io/badge/build-success-brightgreen)
![Net Version](https://img.shields.io/badge/.NET-10.0-purple)
![Architecture](https://img.shields.io/badge/architecture-Clean-blue)

Suns Fidelity is a comprehensive customer loyalty management system designed with **Clean Architecture** principles and **ISO 25000** quality standards.

## 🏗 Architecture

The solution follows a strict **Clean Architecture** separation:

1.  **Fidelity.Domain**: 
    *   Enterprise logic and types (Entities, Enums, Exceptions).
    *   **Specification Pattern**: Reusable query logic (`ISpecification<T>`).
2.  **Fidelity.Application**: 
    *   Business logic and orchestration (CQRS with MediatR).
    *   **Behaviors**: Cross-cutting concerns like Caching and Idempotency.
3.  **Fidelity.Infrastructure**: 
    *   External concerns (EF Core, Email, File System).
    *   **Repository Pattern**: Concrete implementation of data access.
4.  **Fidelity.Server**: 
    *   API Gateway and Controllers.
    *   **ApiControllerBase**: Standardized API responses (`Result<T>`).
5.  **Fidelity.Client**: 
    *   Blazor WebAssembly UI.

## 🚀 Key Features

### Design Patterns Implemented
*   **Repository & Unit of Work**: Decouples business logic from data access.
*   **CQRS (Command Query Responsibility Segregation)**: Separates read and write operations using MediatR.
*   **Specification Pattern**: Encapsulates complex query rules (e.g., `active customers with > 100 points`).
*   **Pipeline Behaviors**: 
    *   `CachingBehavior`: Automatic caching for high-performance queries.
    *   `IdempotencyBehavior`: Prevents duplicate transaction processing.
    *   `ValidationBehavior`: Automatic FluentValidation execution.

### API Capabilities
*   **Versioning**: Semantic versioning supported (currently `v2.0`).
*   **Health Checks**: Real-time system monitoring at `/health`.
*   **Standardized Responses**: consistent JSON envelope via `Result<T>`.

## 🛠 Getting Started

### Prerequisites
*   .NET 10.0 SDK
*   SQL Server (LocalDB or Docker)

### Installation
1.  Clone the repository.
2.  Update connection string in `appsettings.json`.
3.  Run migrations:
    ```bash
    dotnet ef database update --project src/Infrastructure/Fidelity.Infrastructure --startup-project Fidelity.Server
    ```
4.  Start the Server:
    ```bash
    dotnet run --project Fidelity.Server
    ```

## 🔍 API Documentation

Swagger UI is available in development mode at:
*   `https://localhost:7085/swagger`

## 🧪 Quality Standards

This project adheres to **ISO 25000** for software quality:
*   **Maintainability**: Modular design, DI, and clear separation of concerns.
*   **Reliability**: Idempotency and robust error handling.
*   **Performance**: Distributed caching and optimized queries.

---
*Developed by TechService for Suns*
