<p align="center">
  <img src="docs/images/raceday-logo.gif" alt="RaceDay logo" width="260" />
</p>

<h1 align="center">RaceDay - Event Management Platform</h1>

<p align="center">
  <a href="https://github.com/ray-ray102/RaceDay-POE/actions/workflows/part1-validation.yml"><img src="https://github.com/ray-ray102/RaceDay-POE/actions/workflows/part1-validation.yml/badge.svg" alt="Part 1 Validation" /></a>
  <a href="https://github.com/ray-ray102/RaceDay-POE/actions/workflows/ci.yml"><img src="https://github.com/ray-ray102/RaceDay-POE/actions/workflows/ci.yml/badge.svg" alt="Part 2 and 3 CI" /></a>
</p>

| | |
|---|---|
| **Student** | Raesibe Betty Lelaka |
| **Student number** | ST10485428 |
| **Repository** | https://github.com/ray-ray102/RaceDay-POE |
| **Working branch (Part 2 + 3)** | `poe-full-web-app` |

RaceDay is a web app for South African road running, walking and cycling events. Organisers publish races and manage entries. Participants browse upcoming events, enrol, and track their results.

The project was built as a Portfolio of Evidence in three parts:

1. **Part 1:** Planning (ERD, API plan, SQL script)
2. **Part 2:** ASP.NET Core Web API with EF Core, session auth, unit tests and GitHub Actions
3. **Part 3:** MVC front end that calls the API, Azure Blob Storage for images, and Docker

## Contents

- [Features and roles](#features-and-roles)
- [Architecture](#architecture)
- [Screenshots](#screenshots)
- [Tech stack](#tech-stack)
- [Folder layout](#folder-layout)
- [Getting started](#getting-started)
- [Azure Blob Storage](#azure-blob-storage)
- [Docker](#docker)
- [CI/CD](#cicd)
- [Part 1 deliverables](#part-1-deliverables)
- [API overview](#api-overview)
- [Image credits](#image-credits)
- [References](#references)
- [Author](#author)

## Features and roles

There are two roles. The API enforces them with `SessionAuthorize`. The MVC app uses `[RequireLogin]` and `[RequireRole]` and shows a different sidebar menu per role.

| Role | What they can do |
|---|---|
| **Organiser** | Dashboard with stats and recent events. Full event CRUD (name, description, date, location, distance, type Run/Walk/Cycle). Banner upload on edit. Manage categories. View enrolments and update status (Pending / Confirmed / Cancelled). Capture results (finish time and position). |
| **Participant** | Register and log in. Browse events with type, location and date filters. View event details and enrol in a category. My Enrolments with colour-coded statuses. My Results with finish time and position (plus total finishers when the API returns it). Edit profile and upload a profile picture. |
| **Public** | Home, about, contact, register (pick a role) and login. |

## Architecture

```text
+---------------------------+
|          Browser          |
+-------------+-------------+
              |
              |  HTTP
              v
+---------------------------+
|  RaceDay-POE-Web          |
|  ASP.NET Core MVC         |
+-------------+-------------+
              |
              |  HTTP + session cookie (RaceDay.Session)
              v
+---------------------------+
|  RaceDay-POE-API          |
|  ASP.NET Core Web API     |
+------+--------------+-----+
       |              |
       | EF Core      | Azure.Storage.Blobs
       v              v
+--------------+  +---------------------+
| SQL Server   |  | Azure Blob Storage  |
| (or LocalDB) |  | (or Azurite local)  |
+--------------+  +---------------------+
```

| Layer | Responsibility |
|---|---|
| **MVC web app** | Renders the UI and forwards requests to the API through `RaceDayApiClient`. It never touches the database or Azure directly. |
| **Web API** | Handles business logic, role checks (`SessionAuthorize`), data access through EF Core, and image uploads through `IBlobStorageService`. |
| **SQL Server** | Stores users, events, categories, enrolments and results. |
| **Blob Storage** | Stores event banners and profile pictures. |

On login, the web app obtains a session from the API and keeps forwarding that cookie on every request.

Logged-in Organiser and Participant pages use `_DashboardLayout` (navy sidebar). Public pages use the standard top navbar.

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
  wwwroot/         CSS (navy/orange), images, Bootstrap
```

## Screenshots

<table>
<tr>
<td width="50%" valign="top" align="center">
<img src="docs/images/public-home.jpg" alt="Public home" width="400" /><br/>
<em>Public home</em>
</td>
<td width="50%" valign="top" align="center">
<img src="docs/images/login.png" alt="Login" width="400" /><br/>
<em>Login</em>
</td>
</tr>
<tr>
<td width="50%" valign="top" align="center">
<img src="docs/images/participant-dashboard.png" alt="Participant dashboard" width="400" /><br/>
<em>Participant dashboard</em>
</td>
<td width="50%" valign="top" align="center">
<img src="docs/images/participant-browse.jpg" alt="Browse events" width="400" /><br/>
<em>Browse events with filters</em>
</td>
</tr>
<tr>
<td width="50%" valign="top" align="center">
<img src="docs/images/participant-event-details.jpg" alt="Event details" width="400" /><br/>
<em>Event details with banner and enrol</em>
</td>
<td width="50%" valign="top" align="center">
<img src="docs/images/participant-enrolments.png" alt="My Enrolments" width="400" /><br/>
<em>My Enrolments (colour statuses)</em>
</td>
</tr>
<tr>
<td width="50%" valign="top" align="center">
<img src="docs/images/participant-results.png" alt="My Results" width="400" /><br/>
<em>My Results</em>
</td>
<td width="50%" valign="top" align="center">
<img src="docs/images/participant-profile.png" alt="Profile" width="400" /><br/>
<em>Profile with picture upload</em>
</td>
</tr>
<tr>
<td width="50%" valign="top" align="center">
<img src="docs/images/organiser-dashboard.png" alt="Organiser dashboard" width="400" /><br/>
<em>Organiser dashboard</em>
</td>
<td width="50%" valign="top" align="center">
<img src="docs/images/organiser-events.png" alt="My Events" width="400" /><br/>
<em>My Events</em>
</td>
</tr>
<tr>
<td width="50%" valign="top" align="center">
<img src="docs/images/organiser-create.png" alt="Create event" width="400" /><br/>
<em>Create event</em>
</td>
<td width="50%" valign="top" align="center">
<img src="docs/images/organiser-edit-banner.jpg" alt="Edit banner" width="400" /><br/>
<em>Edit event with banner upload</em>
</td>
</tr>
<tr>
<td width="50%" valign="top" align="center">
<img src="docs/images/organiser-categories.png" alt="Categories" width="400" /><br/>
<em>Categories</em>
</td>
<td width="50%" valign="top" align="center">
<img src="docs/images/organiser-enrolments.png" alt="Organiser enrolments" width="400" /><br/>
<em>Organiser enrolments</em>
</td>
</tr>
<tr>
<td width="50%" valign="top" align="center">
<img src="docs/images/organiser-results.png" alt="Results entry" width="400" /><br/>
<em>Results entry</em>
</td>
<td width="50%" valign="top" align="center">
<img src="docs/images/mobile-sidebar.png" alt="Mobile sidebar" width="280" /><br/>
<em>Mobile offcanvas menu</em>
</td>
</tr>
</table>

## Tech stack

| Area | Technology |
|---|---|
| Backend | .NET 8 Web API, Entity Framework Core, SQL Server |
| Frontend | ASP.NET Core MVC, Bootstrap 5, Bootstrap Icons |
| Auth | Session-based, hashed passwords |
| Storage | Azure Blob Storage (Azurite for local work) |
| Testing | xUnit |
| DevOps | Docker (multi-stage images), GitHub Actions |
| Docs | Swagger (Development only) |

UI colours: navy `#1b1f3b` and orange `#ff5a1f`.

## Folder layout

```text
RaceDay-POE/
├── .github/workflows/
│   ├── part1-validation.yml
│   └── ci.yml
├── docs/
│   ├── ERD.png
│   ├── API-Endpoint-Plan.md
│   ├── RaceDay-Schema.sql
│   ├── images/
│   ├── workflow-run.png
│   └── part2-ci-run.png
├── RaceDay-POE-API/
├── RaceDay-POE-API.Tests/
├── RaceDay-POE-Web/
├── Dockerfile
├── Dockerfile.api
├── RaceDay-POE-API.sln
└── README.md
```

## Getting started

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB is fine)
- EF Core tools: `dotnet tool install --global dotnet-ef`
- Git
- Node.js (optional, to run Azurite via `npx` for local image uploads)
- Docker (optional)

### 1. Clone

```bash
git clone https://github.com/ray-ray102/RaceDay-POE.git
cd RaceDay-POE
git checkout poe-full-web-app
```

### 2. Database

The default connection string in `RaceDay-POE-API/appsettings.json` is:

```text
Server=(localdb)\MSSQLLocalDB;Database=RaceDayPOEDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

If your SQL Server differs, change `ConnectionStrings:RaceDayConnection`, then apply the migrations:

```bash
cd RaceDay-POE-API
dotnet ef database update
cd ..
```

> `docs/RaceDay-Schema.sql` is the Part 1 SSMS script, kept for reference. The running API uses the EF migrations.

### 3. Blob storage secret

Never commit real Azure keys. For local Azurite:

```bash
cd RaceDay-POE-API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:AzureBlobStorage" "UseDevelopmentStorage=true"
cd ..
```

### 4. Start Azurite

```bash
npx azurite --silent --location ./azurite --blobHost 127.0.0.1 --blobPort 10000
```

Leave it running while you test banner and profile uploads. The Development config already defaults to `UseDevelopmentStorage=true`.

### 5. Run the API

```bash
cd RaceDay-POE-API
dotnet run --urls http://localhost:5141
```

Swagger is available at `http://localhost:5141/swagger`.

### 6. Run the web app

`ApiSettings:BaseUrl` in `RaceDay-POE-Web/appsettings.json` defaults to `http://localhost:5141/`.

```bash
cd RaceDay-POE-Web
dotnet run --urls http://localhost:5027
```

Or, from the repo root, override the API URL:

```bash
dotnet run --urls http://localhost:5027 --project RaceDay-POE-Web \
  -- --ApiSettings:BaseUrl=http://localhost:5141/
```

Open `http://localhost:5027` and register an Organiser or Participant account.

### 7. Run the tests

```bash
dotnet test RaceDay-POE-API.sln
```

## Azure Blob Storage

Blob Storage is used only by the API. The MVC app never talks to Azure or Azurite.

### Upload flow

1. The browser posts the banner or profile picture as multipart form data to the MVC web app.
2. The web app forwards the file to the API endpoint.
3. The API validates the file type and size.
4. The API uploads the stream through `IBlobStorageService` into the banners or profiles container.
5. The blob store returns a public URL.
6. The API saves that URL on the event or user record.
7. The web app re-renders the page and the `<img>` tag uses the stored URL.

### Endpoints and containers

| Upload | Route | Container (config key) | Default name | Stored as |
|---|---|---|---|---|
| Event banner (Organiser) | `POST /api/events/{id}/banner` | `BlobStorage:BannersContainer` | `raceday-banners` | `BannerImageUrl` |
| Profile picture (Participant) | `POST /api/users/me/picture` | `BlobStorage:ProfilesContainer` | `raceday-profiles` | `ProfilePictureUrl` |

`ImageUploadValidator` accepts `image/jpeg`, `image/jpg`, `image/png` and `image/webp`, up to 2 MB. Invalid files return a 400 with a readable message.

Containers are created with public blob access so pages can use a plain `<img src="...">`.

### Configuration keys

| Key | Purpose |
|---|---|
| `ConnectionStrings:AzureBlobStorage` | Preferred connection string (user-secrets or environment) |
| `BlobStorage:ConnectionString` | Fallback (defaults to `UseDevelopmentStorage=true` in Development) |
| `BlobStorage:BannersContainer` | Banner container name |
| `BlobStorage:ProfilesContainer` | Profile picture container name |

`BlobStorageService` reads `ConnectionStrings:AzureBlobStorage` first, then falls back to `BlobStorage:ConnectionString`.

### Local demo (Azurite)

The demo was run with Azurite only, with no real Azure account. Start Azurite, keep `UseDevelopmentStorage=true`, run the API and web app, then upload a banner or picture in the UI. The URL is saved in SQL and the image loads in the browser.

### Real Azure Storage

Create a Storage Account, then set the connection string without committing it:

```bash
cd RaceDay-POE-API
dotnet user-secrets set "ConnectionStrings:AzureBlobStorage" "<your-azure-storage-connection-string>"
```

Or use an environment variable:

```bash
export ConnectionStrings__AzureBlobStorage="<your-azure-storage-connection-string>"
```

Restart the API. The `raceday-banners` and `raceday-profiles` containers are created on first upload if they do not exist.

## Docker

### Web image

The multi-stage Dockerfile is at the repo root.

```bash
docker build -t raceday-web -f Dockerfile .
```

Run it against an API that is already running:

```bash
docker run --rm -p 8080:8080 \
  -e ApiSettings__BaseUrl=http://host.docker.internal:5141/ \
  raceday-web
```

Then open `http://localhost:8080`. On Linux you may need `--add-host=host.docker.internal:host-gateway`.

### API image (optional)

```bash
docker build -t raceday-api -f Dockerfile.api .
docker run --rm -p 5141:8080 \
  -e ConnectionStrings__RaceDayConnection="<your-sql-server-connection-string>" \
  -e ConnectionStrings__AzureBlobStorage="<your-azure-or-azurite-connection-string>" \
  raceday-api
```

## CI/CD

| Workflow | Purpose |
|---|---|
| `.github/workflows/part1-validation.yml` | Checks that the Part 1 docs and README are in place. |
| `.github/workflows/ci.yml` | Restores and builds the API, tests and MVC web, then runs the API unit tests. Runs on pushes to `main`, `part-2*`, `part-3*`, `final*` and `poe*` branches, and on PRs to `main`. |

Green Part 2 and 3 CI run:

<p align="center"><img src="docs/images/ci-green.png" alt="Successful Part 2 and 3 CI run" width="520" /></p>

Older screenshots: `docs/workflow-run.png` (Part 1) and `docs/part2-ci-run.png`.

## Part 1 deliverables

- `docs/ERD.png`: ERD for Roles, Users, Events, Categories, Enrolments and Results
- `docs/API-Endpoint-Plan.md`: method, route, role, body and responses
- `docs/RaceDay-Schema.sql`: SQL Server create and seed script

**Design choices:** six tables; each event is owned by one Organiser (`OrganiserId`); enrolments link Participant + Event + Category with status Pending / Confirmed / Cancelled; results are one-to-one with enrolments (`FinishTime`, `FinishPosition`).

## API overview

| Area | Endpoints | Access |
|---|---|---|
| Auth | `POST /api/auth/register`, `login`, `logout` | Public |
| Users | `GET/PUT /api/users/me`, `POST /api/users/me/picture` | Logged in |
| Events | `GET` (list/detail) | Public |
| Events | Create, update, delete, `GET /api/events/mine`, `POST /api/events/{id}/banner` | Organiser |
| Categories | Read | Public |
| Categories | Write | Organiser |
| Enrolments | Enrol, mine | Participant |
| Enrolments | List, update status | Organiser |
| Results | Capture, update, delete | Organiser |
| Results | `GET /api/results/mine` | Participant |

Full detail is in `docs/API-Endpoint-Plan.md` and Swagger.

## Image credits

**Magnific:** [cross country](https://www.magnific.com/free-photo/different-people-participating-cross-country_20283238.htm), [team ready for race](https://www.magnific.com/free-photo/team-diverse-people-ready-race_2826771.htm) (rawpixel.com), [cheerful runners](https://www.magnific.com/free-photo/cheerful-runners-having-fun-while-participating-race-nature_26346847.htm) (Drazen Zigic).

**Unsplash:** [Olek Buzunov](https://unsplash.com/photos/people-walking-in-charity-run-OFsKQCFI9SE), [Markus Spiske](https://unsplash.com/photos/group-of-cyclist-on-asphalt-road-WUehAgqO5hE), [Daniel Romero](https://unsplash.com/photos/a-crowd-gathers-at-the-statesman-cap10k-race-qOhho1nnUVQ), [lucas Favre](https://unsplash.com/photos/silhouette-photo-of-a-person-running-on-road-JnoNcfFwrNA).

## References

- Microsoft Learn: [ASP.NET Core](https://learn.microsoft.com/aspnet/core/), [EF Core](https://learn.microsoft.com/ef/core/), [Session state](https://learn.microsoft.com/aspnet/core/fundamentals/app-state)
- [Azure Blob Storage](https://learn.microsoft.com/azure/storage/blobs/) and [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite)
- [Docker docs](https://docs.docker.com/)
- [Bootstrap 5](https://getbootstrap.com/docs/5.3/) and [Bootstrap Icons](https://icons.getbootstrap.com/)
- [xUnit](https://xunit.net/) and [GitHub Actions](https://docs.github.com/actions)
- Unsplash and Magnific for the photos above

## Author

**Raesibe Betty Lelaka**
Student Number: ST10485428
