# ALLDIGI Management

An IT asset-management system for tracking hardware assets, licenses, accessories, users, and organizational inventory.

## What it does

ALLDIGI Management modernizes a Snipe-IT-style inventory workflow with a .NET API and Angular web application. It lets IT teams manage asset lifecycle data, reference catalogs, companies, departments, locations, suppliers, and dashboard summaries in one place.

## Tech stack

- Backend: C# / ASP.NET Core 8, Entity Framework Core, FluentValidation, AutoMapper
- Frontend: Angular 18, Angular Material, TypeScript
- Database: MariaDB 11.4
- Development: Docker Compose
- Experimental UI: Next.js 16 and React 19 (`frontend-react/`)

## Repository layout

```text
backend/SnipeIt.Api/  ASP.NET Core API
frontend/            Primary Angular application
frontend-react/      Experimental Next.js frontend
docs/                Migration plans, audits, and evaluations
samples/csv/         CSV import fixtures
.github/             Repository automation and issue configuration
```

## Prerequisites

- Docker and Docker Compose, for the recommended full-stack setup
- Or, for local development: .NET SDK 8, Node.js 20+, and a MariaDB instance

## Run with Docker

1. Copy the environment template:

   ```bash
   cp .env.dev.example .env
   ```

2. Start the application stack:

   ```bash
   docker compose up --build
   ```

3. Open the Angular app at `http://localhost:4200`. The API is exposed on `http://localhost:5000`; Swagger is available at `http://localhost:5000/swagger` in development.

## Run locally

Start MariaDB (or use the database service from Docker Compose), then run the API:

```bash
dotnet run --project backend/SnipeIt.Api
```

In a separate terminal, start the Angular frontend:

```bash
cd frontend
npm ci
npm start
```

The experimental Next.js UI can be run independently:

```bash
cd frontend-react
npm ci
npm run dev
```

## Contributing

Keep changes focused, update relevant documentation, and verify the affected application before opening a pull request.
