# RaceDay - Event Management Platform

![Part 1 Validation](https://github.com/ray-ray102/RaceDay-POE/actions/workflows/part1-validation.yml/badge.svg)

RaceDay is a full-stack web based event management system designed for the South African road running, walking and cycling community. Many local events can still rely on paper registration and spreadsheets, so RaceDay is planned to bring these processes online. Event Organisers will be able to create and manage events, categories and participant results, while Participants will be able to browse upcoming events, enter events, and track their personal performance history.

This repository contains the Portfolio of Evidence (POE) for the RaceDay project. The project is completed in three parts, with each part building on the work completed in the previous part:

- **Part 1** - System planning, ERD, API endpoint plan and database script
- **Part 2** - RESTful API built with ASP.NET Core and connected to the Part 1 database
- **Part 3** - MVC web application consuming the Part 2 API, Azure Blob Storage and Docker

## Table of Contents

- [User Roles](#user-roles)
- [Repository Structure](#repository-structure)
- [Planned Tech Stack](#planned-tech-stack)
- [Part 1 Deliverables](#part-1-deliverables)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Running the SQL Script](#running-the-sql-script)
- [Troubleshooting](#troubleshooting)
- [Design Notes](#design-notes)
- [GitHub and Commit History](#github-and-commit-history)
- [CI/CD](#cicd)
- [Author](#author)

## User Roles

The system is designed around two distinct user roles. These roles shape the planned data model and API structure:

- **Organiser** - will be able to create, edit and delete events, manage event categories, capture participant results and view enrolments for their events.
- **Participant** - will be able to create an account, browse events, enter an event by selecting a category, view their own enrolments and track their personal results.

Role-based access is planned as part of the system design in Part 1. The API-level enforcement is planned for Part 2, with the role-specific behaviour reflected in the MVC interface planned for Part 3.

## Repository Structure

```text
RaceDay-POE/
├── .github/
│   └── workflows/
│       └── part1-validation.yml   # CI workflow for Part 1 repository validation
├── docs/
│   ├── ERD.png                    # Entity Relationship Diagram
│   ├── API-Endpoint-Plan.md       # API endpoint specification
│   └── RaceDay-Schema.sql         # SQL Server database script
├── LICENSE
├── .gitignore
└── README.md
```

The `/docs` folder contains the planning documents produced for Part 1. Future parts can add their implementation files while keeping the original Part 1 planning documents available for reference.

## Planned Tech Stack

- **Database:** SQL Server
- **API:** ASP.NET Core Web API with Entity Framework Core
- **Web App:** ASP.NET Core MVC
- **Cloud Storage:** Azure Blob Storage for event banners and profile pictures
- **Containerisation:** Docker
- **CI/CD:** GitHub Actions

The API, MVC application, Azure Blob Storage integration and Docker configuration are planned for the later parts of the project.

## Part 1 Deliverables

The Part 1 planning documents are stored inside the `/docs` folder:

| File | Description |
|---|---|
| `docs/ERD.png` | Entity Relationship Diagram representing the planned RaceDay data model, including the entities, attributes, keys and relationships. |
| `docs/API-Endpoint-Plan.md` | API endpoint plan covering Authentication, User Profile, Events, Categories, Enrolments and Results, including routes, roles, request bodies and expected responses. |
| `docs/RaceDay-Schema.sql` | SQL Server script intended to create and populate the RaceDay database schema, including tables, constraints and sample data. |

These documents represent the planned system structure that will be used as the basis for the implementation in Part 2.

## Prerequisites

To work with the current Part 1 repository, the following software may be used:

- SQL Server (Developer or Express edition)
- SQL Server Management Studio (SSMS) to execute and inspect the SQL script
- Git to clone the repository and view the commit history
- Graphviz (optional) if the ERD is regenerated from a Graphviz source file

The .NET SDK and development tools will be required when the ASP.NET Core API is implemented in Part 2.

## Getting Started

1. Clone the repository:

   ```bash
   git clone https://github.com/ray-ray102/RaceDay-POE.git
   cd RaceDay-POE
   ```

2. Open the `docs` folder to review the Part 1 planning documents.

   Open:
   - `docs/ERD.png`
   - `docs/API-Endpoint-Plan.md`
   - `docs/RaceDay-Schema.sql`

3. To create the database from the SQL script, follow the steps in [Running the SQL Script](#running-the-sql-script).

## Running the SQL Script

1. Open SQL Server Management Studio (SSMS).
2. Connect to the SQL Server instance being used for development.
3. Open:

   ```
   docs/RaceDay-Schema.sql
   ```

4. Review the database creation section before executing the script.
5. Execute the script using **Execute** or by pressing **F5**.
6. If the script completes successfully, inspect the database in Object Explorer.
7. Verify that the expected tables, relationships, constraints and sample data have been created.
8. Review the verification queries at the end of the script, where provided, to confirm the resulting database structure and data.

> The exact database name and SQL Server instance depend on the configuration used when the script is executed.

## Troubleshooting

**Database already exists**

If the script reports that the database already exists, check the database creation section of the SQL script and confirm that the existing database is not being used by another connection.

**Login or permission errors**

Make sure the SQL Server account being used in SSMS has the permissions required to create and modify the database.

**Tables are empty**

Check the Messages tab in SSMS for errors generated during the `INSERT` statements. A failed constraint or relationship can prevent sample data from being inserted.

**Foreign key errors**

Check that referenced records exist before inserting records that depend on them. For example, an enrolment must reference valid Participant, Event and Category records.

**Constraint errors**

Review the `CHECK`, `UNIQUE`, `NOT NULL`, primary key and foreign key constraints in the SQL script. The inserted data must satisfy the rules defined by the schema.

## Design Notes

The following design decisions form part of the current Part 1 planning and match `docs/ERD.png`:

- The data model uses six tables: **Roles**, **Users**, **Events**, **Categories**, **Enrolments** and **Results**.
- **Roles** is a lookup table linked one-to-many to **Users**, so each account is either an Organiser or a Participant.
- **Events** are owned by one Organiser via `OrganiserId`, which references `Users.UserId`.
- **Categories** belong to one Event and support optional age ranges (`MinAge`, `MaxAge`) and an optional `DistanceKm`.
- **Enrolments** connect a Participant, an Event and a chosen Category. Status values are Pending, Confirmed or Cancelled, and a Participant may enrol only once per event.
- **Results** are one-to-one with Enrolments (`EnrolmentId` is unique). Each result stores `FinishTime` and `FinishPosition` only.
- Constraints, indexes and foreign keys are included so the SQL script stays consistent with the ERD relationships.

These decisions are documented in greater detail through the ERD and SQL database script.

## GitHub and Commit History

Development is being recorded through incremental Git commits rather than relying on a single large repository upload.

Meaningful commits should represent actual development work, such as:

- Adding or improving the ERD.
- Developing sections of the API endpoint plan.
- Creating or refining the SQL schema.
- Adding database constraints.
- Adding sample data.
- Improving documentation.
- Configuring GitHub Actions.
- Fixing genuine issues discovered during testing.

The GitHub repository and commit history provide a record of the development process. The required number of meaningful commits for each part will be maintained through the development of that part.

## CI/CD

GitHub Actions is being used for automated repository validation during Part 1.

The workflow is located at:

```
.github/workflows/part1-validation.yml
```

The Part 1 workflow is designed to run when repository changes are pushed and checks that the required planning structure is present.

The validation includes checks for:

- The `/docs` folder.
- The ERD file.
- The API endpoint plan.
- The SQL database script.
- The root `README.md`.

A successful workflow run should produce a green check in GitHub Actions.

### Successful CI Build

The screenshot below provides evidence of a successful Part 1 workflow run:

<img src="docs/workflow-run.png" alt="Successful CI workflow run" width="700" />

The CI workflow in Part 1 focuses on repository and documentation validation. Project building and automated unit testing are planned for Part 2 when the ASP.NET Core API and its test project are introduced.

## Author

**Raesibe Betty Lelaka**
Student Number: ST10485428