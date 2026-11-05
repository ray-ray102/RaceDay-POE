# RaceDay - Event Management Platform

![Part 1 Validation](https://github.com/ray-ray102/RaceDay-POE/actions/workflows/part1-validation.yml/badge.svg)
![Part 2 and 3 CI](https://github.com/ray-ray102/RaceDay-POE/actions/workflows/ci.yml/badge.svg)

**Student:** Odirile Masemola  
**Student number:** ST10441421  
**Repository:** https://github.com/ray-ray102/RaceDay-POE  
**Working branch (Part 2 + Part 3):** `poe-full-web-app`

RaceDay is a full-stack web based event management system for the South African road running, walking and cycling community. Organisers create and manage events, categories and results. Participants browse upcoming events, enrol in a category, and track their personal race history.

This Portfolio of Evidence (POE) is delivered in three parts:

| Part | Focus |
|------|--------|
| **Part 1** | System planning: ERD, API endpoint plan, SQL Server schema script |
| **Part 2** | ASP.NET Core Web API, EF Core, session auth, unit tests, GitHub Actions |
| **Part 3** | ASP.NET Core MVC web app consuming the API, Azure Blob Storage, Docker |

---

## Table of Contents

- [YouTube video](#youtube-video)
- [User roles](#user-roles)
- [Architecture](#architecture)
- [Features](#features)
- [Application screenshots](#application-screenshots)
- [Tech stack](#tech-stack)
- [Repository structure](#repository-structure)
- [Prerequisites](#prerequisites)
- [Setup and run](#setup-and-run)
- [Azure Blob Storage](#azure-blob-storage)
- [Docker](#docker)
- [CI/CD](#cicd)
- [Part 1 deliverables](#part-1-deliverables)
- [Part 2 API summary](#part-2-api-summary)
- [Image credits](#image-credits)
- [References](#references)
- [Author](#author)

---

## YouTube video

> **YouTube video: TODO add link**  
> Unlisted YouTube walkthrough of the running application (MVC structure, Blob Storage, Docker) will be added here before final ARC submission. No AI-generated voiceovers.

---

## User roles

| Role | Capabilities |
|------|----------------|
| **Organiser** | Create, edit and delete events; upload event banners; manage categories; view enrolments; capture finish times and positions |
| **Participant** | Register and log in; browse and filter events; enrol by category; view own enrolments and results; update profile and profile picture |

Role-based access is enforced on the API (`SessionAuthorize`) and mirrored in the MVC app with `[RequireLogin]` / `[RequireRole]` and role-specific sidebar navigation.

---

## Architecture

```text
Browser  -->  RaceDay-POE-Web (ASP.NET Core MVC)
                    |
                    | HTTP + session cookie (RaceDay.Session)
                    v
              RaceDay-POE-API (ASP.NET Core Web API)
                    |                    |
                    | EF Core            | Azure.Storage.Blobs
                    v                    v
              SQL Server            Azure Blob Storage
              (or LocalDB)          (or Azurite locally)
```

- The **MVC project has no direct database access** and does not call Azure. All data and file uploads go through the Part 2 API.
- Session authentication is established against the API on login; the web app forwards the API session cookie on subsequent requests.
- Organiser and Participant pages use `_DashboardLayout` (navy sidebar). Public pages (home, about, contact, login, register) keep the standard top navbar.

### MVC structure (Part 3)

```text
RaceDay-POE-Web/
  Controllers/     Account, Home, Organiser, Participant
  Filters/         RequireLogin, RequireRole
  Models/          View models + API DTO mirrors
  Services/        RaceDayApiClient (HttpClient)
  Views/
    Shared/        _Layout, _DashboardLayout, _MediaSlot
    Organiser/     Dashboard, Events, EventForm, Categories, Enrolments, Results, Delete
    Participant/   Dashboard, Events, EventDetails, Enrolments, Results, Profile
  wwwroot/         CSS (navy/orange theme), images, Bootstrap
```

---

## Features

### Organiser

- Dashboard with event counts, upcoming/completed stats, total entries and recent events
- Full event CRUD (name, description, date, location, distance, type: Run / Walk / Cycle)
- Banner image upload when editing an event
- Category management per event (age / distance bands)
- Enrolment list with status updates (Pending / Confirmed / Cancelled)
- Results capture: finish time and finishing position

### Participant

- Browse upcoming events with type, location and date filters
- Event detail with categories and enrol action
- My Enrolments with colour-coded status badges
- My Results with finish time and position (including total finishers where available)
- Profile edit and profile picture upload

### Public

- Branded home, about and contact pages
- Register (choose Organiser or Participant) and login

---

## Application screenshots

Public home:

![Public home](docs/images/public-home.jpg)

Organiser dashboard (sidebar layout):

![Organiser dashboard](docs/images/organiser-dashboard.png)

Participant dashboard:

![Participant dashboard](docs/images/participant-dashboard.png)

Sidebar active state (subtle grey highlight):

![Sidebar active](docs/images/sidebar-active.png)

Mobile offcanvas menu:

![Mobile sidebar](docs/images/mobile-sidebar.png)

---

## Tech stack

- **.NET 8** — ASP.NET Core Web API and MVC
- **Entity Framework Core** — SQL Server (code-first migrations)
- **Session authentication** — hashed passwords (`PasswordHasher`)
- **Swagger** — API exploration in Development
- **xUnit** — API unit tests (EF InMemory)
- **Azure Blob Storage** — event banners and profile pictures (Azurite for local demo)
- **Bootstrap 5 + Bootstrap Icons** — responsive UI (navy `#1b1f3b`, orange `#ff5a1f`)
- **Docker** — multi-stage container images
- **GitHub Actions** — restore, build API + Web, run API tests

---

## Repository structure

```text
RaceDay-POE/
├── .github/workflows/
│   ├── part1-validation.yml      # Part 1 docs structure checks
│   └── ci.yml                    # Part 2 and 3 build + API tests
├── docs/
│   ├── ERD.png
│   ├── API-Endpoint-Plan.md
│   ├── RaceDay-Schema.sql
│   ├── images/                   # README screenshots + CI green capture
│   ├── workflow-run.png          # earlier Part 1 CI evidence
│   └── part2-ci-run.png          # earlier Part 2 CI evidence
├── RaceDay-POE-API/              # Part 2 Web API
├── RaceDay-POE-API.Tests/        # xUnit tests
├── RaceDay-POE-Web/              # Part 3 MVC
├── Dockerfile                    # Web multi-stage image
├── Dockerfile.api                # Optional API image
├── RaceDay-POE-API.sln
└── README.md
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB with Visual Studio, Express, or full SQL Server)
- [EF Core tools](https://learn.microsoft.com/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`
- Git
- Optional for Blob local demo: [Node.js](https://nodejs.org/) (to run Azurite via `npx`)
- Optional for containers: [Docker](https://docs.docker.com/get-docker/)

---

## Setup and run

### 1. Clone

```bash
git clone https://github.com/ray-ray102/RaceDay-POE.git
cd RaceDay-POE
git checkout poe-full-web-app
```

### 2. Configure SQL Server

Default connection string in `RaceDay-POE-API/appsettings.json`:

```text
Server=(localdb)\MSSQLLocalDB;Database=RaceDayPOEDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

Update `ConnectionStrings:RaceDayConnection` if you use a different SQL Server instance.

Apply EF Core migrations:

```bash
cd RaceDay-POE-API
dotnet ef database update
cd ..
```

(Alternatively, Part 1 script `docs/RaceDay-Schema.sql` can create a matching schema in SSMS for reference; the running API uses the EF migrations.)

### 3. Configure Blob Storage (user-secrets)

Do **not** commit real Azure keys. Prefer user-secrets for the API project:

```bash
cd RaceDay-POE-API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:AzureBlobStorage" "UseDevelopmentStorage=true"
cd ..
```

For a real Azure Storage account later, set the same key to your account connection string (see [Azure Blob Storage](#azure-blob-storage)).

### 4. Start Azurite (local Blob emulator)

```bash
npx azurite --silent --location ./azurite --blobHost 127.0.0.1 --blobPort 10000
```

Leave this running while testing banner and profile uploads. Development config already defaults to `UseDevelopmentStorage=true`.

### 5. Run the API

```bash
cd RaceDay-POE-API
dotnet run --urls http://localhost:5141
```

Open Swagger at `http://localhost:5141/swagger`.

### 6. Run the MVC web app

Point the web app at the API base URL (`ApiSettings:BaseUrl`):

```bash
cd RaceDay-POE-Web
dotnet run --urls http://localhost:5027
```

Or override without editing files:

```bash
dotnet run --urls http://localhost:5027 --project RaceDay-POE-Web \
  -- --ApiSettings:BaseUrl=http://localhost:5141/
```

Open `http://localhost:5027`. Register a new Organiser or Participant account (or log in with an account you already created).

### 7. Run API unit tests

```bash
dotnet test RaceDay-POE-API.sln
```

---

## Azure Blob Storage

Blob Storage is integrated **only in the Part 2 API**. The MVC app never talks to Azure (or Azurite) directly. The browser uploads a file to the MVC site as multipart form data; the web project forwards that stream to the API; the API validates the file, uploads via `IBlobStorageService`, and saves the returned public URL on the event or user entity.

### Upload flow

```mermaid
sequenceDiagram
  participant Browser
  participant MVC as RaceDay-POE-Web
  participant API as RaceDay-POE-API
  participant Blob as IBlobStorageService
  participant Store as Azure Blob / Azurite

  Browser->>MVC: multipart POST (banner or picture file)
  MVC->>API: forward multipart to API endpoint
  API->>API: ImageUploadValidator (type + size)
  API->>Blob: UploadAsync(stream, contentType, container, name)
  Blob->>Store: Put blob (public container access)
  Store-->>Blob: blob URI
  Blob-->>API: public URL
  API->>API: save URL on Event.BannerImageUrl / User.ProfilePictureUrl
  API-->>MVC: JSON with updated URL
  MVC-->>Browser: page re-render; img src uses stored URL
```

| Upload | MVC action | API endpoint | Container (config) |
|--------|------------|--------------|--------------------|
| Event banner | Organiser edit / upload banner | `POST /api/events/{id}/banner` | `BlobStorage:BannersContainer` (default `raceday-banners`) |
| Profile picture | Participant Profile | `POST /api/users/me/picture` | `BlobStorage:ProfilesContainer` (default `raceday-profiles`) |

### Validation

`ImageUploadValidator` on the API accepts:

- Content types: `image/jpeg`, `image/jpg`, `image/png`, `image/webp`
- Max size: **2 MB**

Invalid files return `400 Bad Request` with a clear message (no raw exception text to the user).

### Public blob URLs

Containers are created with **public blob access** so the MVC views can use ordinary `<img src="...">` tags against the stored URL. The URL string is persisted on the entity (`BannerImageUrl` / `ProfilePictureUrl`).

### Configuration keys

| Key | Purpose |
|-----|---------|
| `ConnectionStrings:AzureBlobStorage` | Preferred connection string (user-secrets / env / container env) |
| `BlobStorage:ConnectionString` | Fallback (defaults to `UseDevelopmentStorage=true` in Development) |
| `BlobStorage:BannersContainer` | Banner container name |
| `BlobStorage:ProfilesContainer` | Profile picture container name |

`BlobStorageService` reads `ConnectionStrings:AzureBlobStorage` first, then falls back to `BlobStorage:ConnectionString`.

### Local development (Azurite) — how the demo was run

This project's local demo used **Azurite only**. No real Azure Storage account or paid cloud credentials were required.

1. Start Azurite (see Setup step 4).
2. Keep `UseDevelopmentStorage=true` via user-secrets or `appsettings.Development.json`.
3. Run API + Web; upload a banner or profile picture through the UI.
4. Images are stored under the Azurite blob endpoint and the public URL is saved in SQL.

### Switching to a real Azure Storage Account

1. Create a Storage Account in the Azure portal and copy the connection string.
2. Store it with user-secrets (recommended) or an environment variable — never commit it:

   ```bash
   cd RaceDay-POE-API
   dotnet user-secrets set "ConnectionStrings:AzureBlobStorage" "<your-azure-storage-connection-string>"
   ```

   Or when running in Docker / hosting:

   ```bash
   export ConnectionStrings__AzureBlobStorage="<your-azure-storage-connection-string>"
   ```

3. Restart the API. Containers `raceday-banners` and `raceday-profiles` are created on first upload if they do not exist.
4. Confirm uploaded image URLs open in the browser and appear on event / profile pages.

---

## Docker

The MVC web application is containerised with a multi-stage .NET 8 Dockerfile at the repository root. The API has a matching `Dockerfile.api` if you want to run both in containers.

### Build the web image

```bash
docker build -t raceday-web -f Dockerfile .
```

### Run the web container

Point the container at a reachable RaceDay API with `ApiSettings__BaseUrl` (the MVC app never talks to the database or Azure directly):

```bash
docker run --rm -p 8080:8080 \
  -e ApiSettings__BaseUrl=http://host.docker.internal:5141/ \
  raceday-web
```

Then open `http://localhost:8080`. On Linux you may need `--add-host=host.docker.internal:host-gateway` so the container can reach an API running on the host.

### Optional: API image

```bash
docker build -t raceday-api -f Dockerfile.api .
docker run --rm -p 5141:8080 \
  -e ConnectionStrings__RaceDayConnection="<your-sql-server-connection-string>" \
  -e ConnectionStrings__AzureBlobStorage="<your-azure-or-azurite-connection-string>" \
  raceday-api
```

---

## CI/CD

GitHub Actions workflows:

| Workflow | File | What it does |
|----------|------|----------------|
| Part 1 Validation | `.github/workflows/part1-validation.yml` | Checks `/docs` planning files and README |
| Part 2 and 3 CI | `.github/workflows/ci.yml` | Restores and builds API + tests + MVC Web; runs API unit tests |

The Part 2 and 3 workflow runs on pushes to `main`, `part-2*`, `part-3*`, `final*` and `poe*` branches, and on pull requests to `main`.

### Successful CI build

Screenshot of a green **Part 2 and 3 CI** run on branch `poe-full-web-app`:

![Successful Part 2 and 3 CI run](docs/images/ci-green.png)

Earlier evidence is also kept at `docs/workflow-run.png` (Part 1) and `docs/part2-ci-run.png` (Part 2 era).

---

## Part 1 deliverables

| File | Description |
|------|-------------|
| `docs/ERD.png` | Entity Relationship Diagram (Roles, Users, Events, Categories, Enrolments, Results) |
| `docs/API-Endpoint-Plan.md` | Endpoint plan: method, route, role, body, responses |
| `docs/RaceDay-Schema.sql` | SQL Server create + seed script aligned with the ERD |

### Design notes

- Six tables: Roles, Users, Events, Categories, Enrolments, Results
- Events owned by one Organiser (`OrganiserId`)
- Enrolments link Participant + Event + Category; status Pending / Confirmed / Cancelled
- Results are one-to-one with Enrolments (`FinishTime`, `FinishPosition`)

---

## Part 2 API summary

| Area | Endpoints | Role |
|------|-----------|------|
| Auth | `POST /api/auth/register`, `login`, `logout` | Public |
| Users | `GET/PUT /api/users/me`, `POST /api/users/me/picture` | Logged in |
| Events | `GET /api/events`, `GET /api/events/{id}` | Public |
| Events | `POST/PUT/DELETE`, `GET /api/events/mine`, `POST /api/events/{id}/banner` | Organiser |
| Categories | Public read; Organiser create/update/delete | Mixed |
| Enrolments | Participant enrol / mine; Organiser list + status | Mixed |
| Results | Organiser capture/update/delete; Participant `GET /api/results/mine` | Mixed |

Full detail: `docs/API-Endpoint-Plan.md` and Swagger UI.

---

## Image credits

Photos used on the public site:

**Magnific**

- [different people participating cross country](https://www.magnific.com/free-photo/different-people-participating-cross-country_20283238.htm)
- [team diverse people ready race](https://www.magnific.com/free-photo/team-diverse-people-ready-race_2826771.htm) (rawpixel.com)
- [cheerful runners](https://www.magnific.com/free-photo/cheerful-runners-having-fun-while-participating-race-nature_26346847.htm) (Drazen Zigic)

**Unsplash**

- [Olek Buzunov](https://unsplash.com/photos/people-walking-in-charity-run-OFsKQCFI9SE)
- [Markus Spiske](https://unsplash.com/photos/group-of-cyclist-on-asphalt-road-WUehAgqO5hE)
- [Daniel Romero](https://unsplash.com/photos/a-crowd-gathers-at-the-statesman-cap10k-race-qOhho1nnUVQ)
- [lucas Favre](https://unsplash.com/photos/silhouette-photo-of-a-person-running-on-road-JnoNcfFwrNA)

---

## References

- Microsoft Learn — [ASP.NET Core](https://learn.microsoft.com/aspnet/core/), [EF Core](https://learn.microsoft.com/ef/core/), [Session state](https://learn.microsoft.com/aspnet/core/fundamentals/app-state)
- Microsoft Learn — [Azure Blob Storage](https://learn.microsoft.com/azure/storage/blobs/) and [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite)
- [Docker docs](https://docs.docker.com/) — multi-stage .NET images
- [Bootstrap 5](https://getbootstrap.com/docs/5.3/) and [Bootstrap Icons](https://icons.getbootstrap.com/)
- [xUnit](https://xunit.net/) and [GitHub Actions](https://docs.github.com/actions)
- Unsplash and Magnific — stock photography (see Image credits)

---

## Author

**Odirile Masemola**  
Student number: **ST10441421**  
Module: Automation of Web Applications (POE Parts 1–3)
