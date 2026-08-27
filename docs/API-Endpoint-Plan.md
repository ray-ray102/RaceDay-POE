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
## 1. Authentication

Handles account creation and starting or ending a session. Every user registers with a role,
this decision cannot be changed later without going through an administrator, since the whole
system depends on knowing whether a user is an Organiser or a Participant.

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Creates a new user account as either an Organiser or a Participant. The password is hashed before it is stored, it is never saved in plain text. | None (public) | { "firstName", "lastName", "email", "password", "phoneNumber", "role" } | 201 Created - returns the new user's id, name and role. 400 Bad Request - validation failed, email already in use, or role is not Organiser or Participant. |
| POST | /api/auth/login | Verifies a user's email and password against the stored hash, then starts a session and stores the user id and role server-side. | None (public) | { "email", "password" } | 200 OK - returns basic user details and role, session cookie set on the response. 401 Unauthorized - email or password incorrect. |
| POST | /api/auth/logout | Ends the current session for the logged in user, clearing the stored user id and role. | Any (logged in) | None | 200 OK - session cleared, subsequent requests are treated as logged out. |

**Example register request body**
```json
{
  "firstName": "Sipho",
  "lastName": "Dlamini",
  "email": "sipho.dlamini@gmail.com",
  "password": "MySecurePassword123",
  "phoneNumber": "0721112222",
  "role": "Participant"
}
```
**Example successful login response**
```json
{
  "userId": 3,
  "firstName": "Sipho",
  "lastName": "Dlamini",
  "role": "Participant"
}
```
## 2. User Profile

Both roles share the same profile endpoints, since a Users record holds the same core fields
for an Organiser or a Participant. The API works out which user is calling from the session, so
these routes never take a user id in the path, only /me is used.

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/users/me | Returns the profile details of the currently logged in user, including their role. | Any (logged in) | None | 200 OK - user profile object, password hash is never included in the response. 401 Unauthorized - no active session. |
| PUT | /api/users/me | Updates the profile details of the currently logged in user, such as name, phone number and profile picture URL. Email and role cannot be changed through this endpoint. | Any (logged in) | { "firstName", "lastName", "phoneNumber", "profilePictureUrl" } | 200 OK - updated profile returned. 400 Bad Request - validation failed, such as an empty first name. |

**Example profile response**
```json
{
  "userId": 3,
  "firstName": "Sipho",
  "lastName": "Dlamini",
  "email": "sipho.dlamini@gmail.com",
  "phoneNumber": "0721112222",
  "profilePictureUrl": null,
  "role": "Participant"
}
```
## 3. Events

Events are owned by the Organiser who created them. Anyone can browse events, but only the
owning Organiser can change or remove one, this ownership check happens on every write endpoint
below, not just the role check, since two different Organisers must not be able to edit each
other's events.

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events | Lists all upcoming events, viewable by anyone. Supports optional query string filters. | None (public) | None | 200 OK - array of event summaries. |
| GET | /api/events/{id} | Returns the full detail for a single event, including its list of categories. | None (public) | None | 200 OK - event detail object with a nested categories array. 404 Not Found - event does not exist. |
| POST | /api/events | Creates a new event owned by the logged in Organiser, the OrganiserId is taken from the session, not from the request body. | Organiser | { "name", "description", "eventDate", "location", "distanceKm", "eventType", "bannerImageUrl" } | 201 Created - the new event object. 400 Bad Request - validation failed, such as eventType not being Run, Walk or Cycle. |
| PUT | /api/events/{id} | Updates an existing event. Only the Organiser who owns the event may edit it. | Organiser | { "name", "description", "eventDate", "location", "distanceKm", "eventType", "bannerImageUrl" } | 200 OK - updated event. 403 Forbidden - logged in Organiser does not own this event. 404 Not Found - event does not exist. |
| DELETE | /api/events/{id} | Deletes an event owned by the logged in Organiser, this will fail if the event still has active enrolments, since the database enforces that link. | Organiser | None | 204 No Content - deleted successfully. 400 Bad Request - event still has enrolments and cannot be deleted. 403 Forbidden - not the owning Organiser. 404 Not Found - event does not exist. |
| GET | /api/events/mine | Lists all events created by the logged in Organiser, along with a count of enrolments per event, used to build the Organiser dashboard in Part 3. | Organiser | None | 200 OK - array of the Organiser's own events with enrolment counts. |
**Optional query string filters on GET /api/events**

| Parameter | Example | Effect |
|---|---|---|
| eventType | ?eventType=Run | Only returns events of that type |
| location | ?location=Durban | Only returns events whose location contains this text |
| fromDate | ?fromDate=2026-09-01 | Only returns events on or after this date |

**Example event detail response**
```json
{
  "eventId": 1,
  "organiserId": 1,
  "name": "Joburg City Run",
  "description": "A morning road run through the streets of Johannesburg",
  "eventDate": "2026-10-04T06:00:00",
  "location": "Sandton, Johannesburg",
  "distanceKm": 10.0,
  "eventType": "Run",
  "bannerImageUrl": null,
  "categories": [
    { "categoryId": 1, "name": "10km Open", "distanceKm": 10.0 },
    { "categoryId": 2, "name": "5km Fun Run", "distanceKm": 5.0 }
  ]
}
```
## 4. Categories

Categories always belong to exactly one event, this is why every category route sits either
under /api/events/{eventId}/categories for creating and listing, or under /api/categories/{id}
directly for reading, updating or deleting a single category once you already know its id.

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events/{eventId}/categories | Lists all categories that belong to a specific event. | None (public) | None | 200 OK - array of categories. 404 Not Found - event does not exist. |
| GET | /api/categories/{id} | Returns a single category by id. | None (public) | None | 200 OK - category object. 404 Not Found - category does not exist. |
| POST | /api/events/{eventId}/categories | Adds a new age or distance category to an event owned by the logged in Organiser. | Organiser | { "name", "description", "minAge", "maxAge", "distanceKm" } | 201 Created - the new category. 400 Bad Request - minAge is greater than maxAge. 403 Forbidden - not the owning Organiser. 404 Not Found - event does not exist. |
| PUT | /api/categories/{id} | Updates an existing category belonging to one of the Organiser's events. | Organiser | { "name", "description", "minAge", "maxAge", "distanceKm" } | 200 OK - updated category. 403 Forbidden - not the owning Organiser. 404 Not Found - category does not exist. |
| DELETE | /api/categories/{id} | Removes a category from an event owned by the logged in Organiser, this will fail if participants are already enrolled in the category. | Organiser | None | 204 No Content - deleted successfully. 400 Bad Request - category still has enrolments. 403 Forbidden - not the owning Organiser. 404 Not Found - category does not exist. |

**Example category request body**
```json
{
  "name": "10km Open",
  "description": "Open category for the full 10km route",
  "minAge": 16,
  "maxAge": null,
  "distanceKm": 10.0
}