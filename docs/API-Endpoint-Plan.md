# RaceDay API Endpoint Plan

This document lists every API endpoint planned for the RaceDay system. It will be implemented in
Part 2 using ASP.NET Core Web API, following this plan as closely as possible. The plan covers
Authentication, User Profile, Events, Categories, Event Enrolments, and Results, plus a few
extra endpoints identified as necessary while planning the system.

## How Authentication and Roles Work

RaceDay uses session based authentication rather than tokens. When a user logs in successfully,
the server creates a session and stores the user's id and role inside it. On every request that
follows, the API reads the session to know who is calling and what role they have, there is no
need for the client to attach a token manually, the session cookie handles this automatically.

Every endpoint below lists a Role Required value, which means:

| Role Required | Who can call this endpoint |
|---|---|
| **None (public)** | Anyone, logged in or not, no session needed |
| **Any (logged in)** | Any user with an active session, Organiser or Participant |
| **Organiser** | Only users whose session role is Organiser |
| **Participant** | Only users whose session role is Participant |

If a request is made to a protected endpoint without a valid session, the API returns
**401 Unauthorized**. If the session is valid but the role does not match what the endpoint
requires, the API returns **403 Forbidden**. This distinction matters, 401 means "we don't know
who you are", 403 means "we know who you are, but you are not allowed to do this".
## HTTP Status Codes Used Throughout This Plan

| Code | Meaning | When it is used in RaceDay |
|---|---|---|
| 200 OK | Request succeeded | Successful GET, PUT or logout requests |
| 201 Created | A new resource was created | Successful POST requests that add a new row, such as registering or creating an event |
| 204 No Content | Request succeeded, nothing to return | Successful DELETE requests |
| 400 Bad Request | The request was invalid | Missing fields, validation failed, duplicate email, already enrolled |
| 401 Unauthorized | No valid session | The user is not logged in |
| 403 Forbidden | Valid session, wrong role or not the owner | A Participant calling an Organiser only endpoint, or an Organiser trying to edit someone else's event |
| 404 Not Found | The resource does not exist | Invalid id in the route, such as an event that was deleted |
