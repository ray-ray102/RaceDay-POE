# RaceDay - Event Management Platform

![Part 1 Validation](https://github.com/ray-ray102/RaceDay-POE/actions/workflows/part1-validation.yml/badge.svg)


RaceDay is a full-stack web based event management system built for the South African road
running, walking and cycling community. Many local events are still run through paper
registration and spreadsheets, RaceDay brings that process online. Event Organisers can create
and manage events, categories and participant results, while Participants can browse upcoming
events, enter events, and track their personal performance history.

This repository holds the Portfolio of Evidence (POE) for the RaceDay project, submitted in
three parts, each part builds directly on the one before it:

- **Part 1** - System planning, ERD, API endpoint plan and database script (this part)
- **Part 2** - RESTful API built with ASP.NET Core, connected to the Part 1 database
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
- [Video Walkthrough](#video-walkthrough)
- [Author](#author)

## User Roles

The system supports two distinct user roles, this decision shapes the entire data model and
API plan:

- **Organiser** - can create, edit and delete events, manage event categories, capture
  participant results and view all enrolments for their events.
- **Participant** - can create an account, browse events, enter an event by selecting a
  category, view their own enrolments and track their personal results.

A user picks their role once, at registration. Role based access is planned at the API level
here in Part 1, will be enforced server-side in the API in Part 2, and reflected consistently in
the MVC interface in Part 3.