# Tour Planner

A web-based application for planning and logging bike, hike, running, and vacation tours.

## Tech Stack

- **Frontend:** React 18 + TypeScript + Vite, with an explicit MVVM layer (`src/viewmodels`) + React Router + Leaflet
- **Backend:** C# ASP.NET Core with layer-based architecture (API / BL / DAL)
- **Database:** PostgreSQL via Entity Framework Core
- **Logging:** log4net
- **Tests:** NUnit (54 unit tests)
- **Maps/Routing:** OpenRouteService.org + Leaflet
- **Weather (unique feature):** Open-Meteo.com (no API key required)
- **Containerization:** Docker Compose

> **Note on the frontend framework:** the assignment specifies Angular. This project uses React with a hand-written
> MVVM layer instead — see `docs/protocol-final.html` (Sections 2 and 4) for the rationale and an explicit
> acknowledgement that this is a known deviation from the Must-Haves checklist.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [OpenRouteService API Key](https://openrouteservice.org/dev/#/signup) (free)

## Quick Start with Docker

```bash
cp .env.example .env   # fill in POSTGRES_PASSWORD, JWT_SECRET_KEY, ORS_API_KEY
docker-compose up --build
```

- Frontend: <http://localhost>
- Backend API: <http://localhost:5000>
- Swagger: <http://localhost:5000/swagger>

## Local Development

```bash
# Start only PostgreSQL
docker-compose up postgres -d

# Backend (in backend/ folder)
cp TourPlanner.API/appsettings.Example.json TourPlanner.API/appsettings.json   # fill in secrets
dotnet run --project TourPlanner.API

# Frontend (in frontend/ folder, new terminal)
npm install && npm run dev
```

Frontend: <http://localhost:5173>

## Running Tests

```bash
cd backend && dotnet test
```

## Architecture

```text
TourPlanner.API  (Controllers)
    TourPlanner.BL   (Services, Strategies, DTOs, ORS/Weather HTTP clients)
    TourPlanner.DAL  (Repositories, Unit of Work, EF DbContext, Entities)
    TourPlanner.Tests (NUnit, 54 tests)

frontend/src
    viewmodels/   (MVVM ViewModels: Auth, TourList, TourDetail)
    pages/        (Views - routed pages)
    components/   (Views - reusable UI components)
    api/          (Model - HTTP access)
    store/        (Model - persisted auth session)
```

**Design Patterns:** Repository, Dependency Injection, DTO, Adapter (ORS/Open-Meteo clients), Strategy
(child-friendliness classification, per-transport average speed), Unit of Work (atomic tour import), MVVM (frontend).

## Features

- Tour & tour log CRUD, full-text search across both (including computed popularity/child-friendliness)
- Route distance/duration from OpenRouteService, rendered on a Leaflet map, with a Strategy-based fallback estimate
- Tour images, validated and stored externally on the filesystem (not in the database)
- Import/export of tour data as JSON
- Current weather at a tour's starting location (Open-Meteo, no API key — the "unique feature")

## Configuration

Backend: copy `backend/TourPlanner.API/appsettings.Example.json` to `appsettings.json` and set:

- `ConnectionStrings:Default`
- `JwtSettings:SecretKey` (min 32 chars)
- `OpenRouteService:ApiKey` (get free key at [openrouteservice.org](https://openrouteservice.org))
- `Storage:BasePath` (where uploaded tour images are stored)

Docker Compose: copy `.env.example` to `.env` and set `POSTGRES_PASSWORD`, `JWT_SECRET_KEY`, `ORS_API_KEY`. Neither
file is committed to the repository.

## Documentation

See `docs/protocol-final.html` for the full submission protocol: use-case diagram, wireframes, architecture/class/
sequence diagrams, unit test rationale, Must-Haves compliance matrix, and time tracking.
