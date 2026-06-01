## ADDED Requirements

### Requirement: Admin can list all authors
The system SHALL provide a paginated (or full-list, given small catalogue) table of all authors at `/admin/authors`, showing name, bio excerpt, avatar URL, and course count. The page SHALL be accessible only to Admin and SuperAdmin roles.

#### Scenario: Authors exist
- **WHEN** an admin navigates to `/admin/authors`
- **THEN** the page renders a table with one row per author showing name, bio (truncated to 80 chars), and assigned course count

#### Scenario: No authors exist
- **WHEN** there are no author records in the database
- **THEN** the page displays an empty state with a "Create Author" call-to-action

#### Scenario: Loading state
- **WHEN** the author list is being fetched
- **THEN** the table shows animated skeleton rows

---

### Requirement: Admin can fetch a single author by ID
The system SHALL expose `GET /api/admin/authors/:id` returning full author detail including course count.

#### Scenario: Author found
- **WHEN** a request is made to `GET /api/admin/authors/1` and the author exists
- **THEN** the response is HTTP 200 with `{ id, name, bio, avatarUrl, userId, courseCount }`

#### Scenario: Author not found
- **WHEN** a request is made to `GET /api/admin/authors/999` and no such author exists
- **THEN** the response is HTTP 404

---

### Requirement: Admin can create an author
The system SHALL allow an Admin to create a new author via `POST /api/admin/authors` with fields `name` (required, ≤200 chars), `bio` (optional, ≤1000 chars), `avatarUrl` (optional, valid URL ≤2048 chars), and `userId` (optional).

#### Scenario: Valid create request
- **WHEN** an admin submits a valid create author form with name "John Smith"
- **THEN** the system creates the record, returns HTTP 201, and the author appears in the list

#### Scenario: Missing required name
- **WHEN** an admin submits a create author form with an empty name field
- **THEN** the system returns HTTP 400 with a validation error message

#### Scenario: Name too long
- **WHEN** an admin submits a name exceeding 200 characters
- **THEN** the system returns HTTP 400 with a validation error

---

### Requirement: Admin can update an author
The system SHALL allow an Admin to update an existing author's `name`, `bio`, `avatarUrl`, and `userId` via `PUT /api/admin/authors/:id`. The same validation rules as create SHALL apply.

#### Scenario: Valid update request
- **WHEN** an admin submits a valid edit form for author ID 1
- **THEN** the system updates the record, returns HTTP 200 with the updated author, and the list reflects the changes

#### Scenario: Update non-existent author
- **WHEN** an admin submits an update for author ID 999 which does not exist
- **THEN** the system returns HTTP 404

#### Scenario: Name cleared on update
- **WHEN** an admin submits an update with an empty name
- **THEN** the system returns HTTP 400 with a validation error

---

### Requirement: Admin can delete an author
The system SHALL allow an Admin to delete an author via `DELETE /api/admin/authors/:id`, provided the author has no assigned courses.

#### Scenario: Delete unassigned author
- **WHEN** an admin confirms deletion of an author with zero assigned courses
- **THEN** the system deletes the record, returns HTTP 204, and the author no longer appears in the list

#### Scenario: Delete blocked by assigned courses
- **WHEN** an admin attempts to delete an author who is assigned to one or more courses
- **THEN** the system returns HTTP 409 with a message indicating the author has assigned courses and must be reassigned first

#### Scenario: Delete non-existent author
- **WHEN** an admin attempts to delete author ID 999 which does not exist
- **THEN** the system returns HTTP 404

---

### Requirement: Admin authors management is secured to Admin role
All endpoints under `/api/admin/authors` SHALL require the caller to be authenticated and hold the `Admin` or `SuperAdmin` role.

#### Scenario: Unauthenticated request
- **WHEN** an unauthenticated request is made to any `/api/admin/authors` endpoint
- **THEN** the response is HTTP 401

#### Scenario: Student role request
- **WHEN** a user with the `Student` role requests any `/api/admin/authors` endpoint
- **THEN** the response is HTTP 403
