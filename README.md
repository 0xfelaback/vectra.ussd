# Vectra.Ussd

A .NET-based USSD banking and customer service API that powers interactive mobile banking flows through a simple menu-driven interface. The solution is designed to accept USSD-style requests, manage user sessions, and orchestrate banking operations such as account opening, registration, transfers, account inquiry, airtime and data top-up, card management, and PIN management.

## Project Summary

This repository contains a multi-layer .NET solution that exposes a single API endpoint for USSD interactions and separates responsibilities across layers for maintainability and extensibility.

The application is structured around the following key concerns:

- API entry point for USSD requests
- orchestration and workflow logic for each banking feature
- domain entities and business rules
- infrastructure services such as Redis caching and persistence
- mock data support for local development and prototype scenarios

Use this project as the foundation for a banking USSD platform, a fintech interaction layer, or a mobile channel service that needs secure, menu-based customer communication.

## Features

The solution currently includes support for the following capabilities:

- Account registration via USSD
- Account opening and customer onboarding
- Account balance and account inquiry flows
- Transfers between linked accounts
- Airtime recharge workflows
- Data bundle purchase flows
- BVN inquiry handling
- PIN management
- Card request and card management flows
- Session handling for consecutive USSD steps
- Local mock data seeding for development and testing

## Tech Stack

- ASP.NET Core Web API
- .NET 10
- Entity Framework Core / SQLite
- Redis for distributed session caching
- AutoMapper, FluentValidation, and MediatR
- XML request handling for USSD channel communication

## Repository Structure

```text
Vectra.Ussd/
├── Vectra.Ussd.Api/              # API host and request pipeline
├── Vectra.Ussd.Application/      # Orchestrators, services, DTOs, validations
├── Vectra.Ussd.Domain/           # Core entities and business domain model
├── Infrastructure/
│   ├── Vectra.Ussd.Infrastructure/  # Persistence and infrastructure services
│   └── Vectra.Ussd.Mocks/            # Mock data and local sample database support
└── Vectra.Ussd.Tests/            # Test project
```

## Prerequisites

Before running the project locally, make sure you have the following installed:

- .NET SDK 10 or later
- Redis server running locally on `localhost:6379`
- SQLite support available for local development
- A code editor such as Visual Studio or VS Code
- Optional: Postman or HTTP client tooling for testing the API endpoint

## Installation

1. Clone the repository:

```bash
git clone https://github.com/0xfelaback/vectra.ussd.git
cd Vectra.Ussd
```

2. Restore NuGet dependencies:

```bash
dotnet restore
```

3. Build the solution:

```bash
dotnet build
```

## Configuration

The application configuration is defined in the API project configuration files.

Primary configuration file:

- `Vectra.Ussd.Api/appsettings.json`

The current configuration includes connection strings for:

- `localConnectionString`
- `mockConnectionstring`
- `redisConnectionString`
- `simulatorLocalDomain`

Example configuration:

```json
{
  "ConnectionStrings": {
    "localConnectionString": "Data Source=/Users/great/Desktop/cs/Vectra.Ussd/Vectra.Ussd.Infrastructure/local.db",
    "mockConnectionstring": "Data Source=/Users/great/Desktop/cs/Vectra.Ussd/Infrastructure/Vectra.Ussd.Mocks/mock.db",
    "redisConnectionString": "localhost:6379",
    "simulatorLocalDomain": "https://localhost:7110"
  }
}
```

Update these values to match your local environment, database paths, and deployment target.

## Running the Application

Start the API from the solution root:

```bash
dotnet run --project Vectra.Ussd.Api
```

Once the API is running, the endpoint for USSD interaction is exposed under:

```text
POST /api/ussd-channel
```

The request format uses XML content and expects the following basic fields:

- `msisdn`
- `network`
- `sessionid`
- `msg`
- `type`

## Usage Examples

The repository includes a sample request file at `request.http`. Use it as a quick starting point for manual testing.

### Example: Initial request

```http
POST http://localhost:5225/api/ussd-channel
Content-Type: application/xml
Accept: application/xml

<request>
    <msisdn>08005857032</msisdn>
    <network>4</network>
    <sessionid>260410110945569</sessionid>
    <msg>*822*1#</msg>
    <type>1</type>
</request>
```

### Example: Continue existing session

```http
POST http://localhost:5225/api/ussd-channel
Content-Type: application/xml
Accept: application/xml

<request>
    <msisdn>2348058926921</msisdn>
    <network>4</network>
    <sessionid>260410110945569</sessionid>
    <msg>1</msg>
    <type>2</type>
</request>
```

### Common menu flow

Typical flows include:

- `*822#` → Main menu
- `*822*1#` → Registration
- `*822*7#` → Account opening
- `*822*6#` → Account balance
- `*822*8#` → Account number services

For production use, connect the API to a real telecommunication gateway or USSD aggregator that can forward these XML requests to the service.

## Session Handling

The API relies on a session identifier to maintain the state of a USSD conversation across multiple request/response rounds.

This means:

- The first request establishes the session
- Subsequent requests reuse the same `sessionid`
- The service stores session context in Redis-backed cache
- The response format is designed to be consumed by a USSD gateway or simulator

## Development Notes

The project is organized around clean separation of concerns:

- `Api` layer handles HTTP and request parsing
- `Application` layer contains orchestration, validation, and services
- `Domain` layer describes business entities and rules
- `Infrastructure` layer provides data access and supporting integrations

This architecture makes it easier to add new USSD features without disturbing the existing request pipeline.

## Testing

Run the test suite with:

```bash
dotnet test
```

If you are extending the solution, add or update unit tests in the `Vectra.Ussd.Tests` project to cover new workflows and regression behavior.

## Contributing

Contributions are welcome. To contribute:

1. Create a feature branch.
2. Make your changes with clear commit messages.
3. Update relevant documentation and tests.
4. Open a pull request for review.

Suggested contribution checklist:

- Keep handlers and services focused on a single responsibility
- Maintain session flow consistency for USSD interactions
- Validate request inputs before processing financial operations
- Prefer deterministic, testable service logic

## Project Status

This repository is intended as a working banking USSD backend, against the product documentation this is an incomplete project and may require additional hardening for production environments, including:

- production-grade authentication and authorization
- secure secrets management
- audit logging and monitoring
- external banking or core-banking integration hardening
- compliance and fraud tooling
