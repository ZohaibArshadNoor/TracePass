
# TracePass

Digital Product Passport and Compliance Platform for Garment and Textile Exports.

## Overview

TracePass is a full stack platform designed to help garment and textile exporters maintain product traceability, manage supporting documents, and verify compliance claims.

The platform will connect product information, suppliers, materials, certificates, supply chain events, and compliance reviews in one auditable system.

AI will assist with document extraction and explanations. Final compliance decisions will remain under deterministic business rules and human review.

## Technology Stack

### Backend
- .NET 10 and ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL with Npgsql
- Serilog
- Swagger / OpenAPI

### Frontend
- React
- TypeScript
- Vite

### Infrastructure
- Docker
- Docker Compose
- GitHub Actions (CI planned)

### Planned Components
- Python and FastAPI AI service
- n8n workflow automation
- Automated backend and frontend tests

## Repository Structure

```text
TracePass/
├── backend/
│   ├── src/
│   │   ├── TracePass.Api/
│   │   ├── TracePass.Application/
│   │   ├── TracePass.Domain/
│   │   └── TracePass.Infrastructure/
│   └── tests/
├── frontend/
├── ai-service/
├── automation/
├── docs/
├── docker-compose.yml
├── .env.example
├── .gitignore
├── README.md
└── plan.md
```

Some directories and components will be implemented in later phases.

## Prerequisites

- .NET 10 SDK
- Node.js and npm
- Docker Desktop
- Git

## Run the Database

From the repository root:

```cmd
docker compose up -d db
```

Check the database container:

```cmd
docker compose ps
```

The local PostgreSQL port is configured through Docker Compose. The current development setup maps host port 5433 to container port 5432.

## Run the Backend

Open CMD in the repository root:

```cmd
cd backend
dotnet restore TracePass.sln
dotnet build TracePass.sln
dotnet run --project src\TracePass.Api
```

Use the API URL printed in the terminal to open Swagger.

The health endpoint is `/health`. It checks whether the API can connect to PostgreSQL.

## Run the Frontend

Open a separate CMD window in the repository root:

```cmd
cd frontend
npm install
npm run dev
```

Open the local URL printed by Vite in the terminal.

## Configuration

Use `.env.example` as the reference for local environment variables.

Never commit real credentials, API keys, or production secrets. Keep local `.env` files out of Git.

## Architecture Principles

- Keep business rules in the .NET application.
- Keep domain logic independent of infrastructure concerns.
- Validate incoming data on the server.
- Test important business rules and access restrictions.
- Treat AI outputs as suggestions that require validation.
- Do not let AI or n8n approve or reject compliance cases.
- Record important business changes in an auditable manner.

## Development Status

TracePass is under active development.

The current focus is the repository foundation, API setup, database connectivity, logging, and continuous integration. Authentication, organisations, product passports, compliance workflows, AI, and automation will be developed in later phases.

## License

No license has been selected yet.