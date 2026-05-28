## ADDED Requirements

### Requirement: Avatar displays user initials
The system SHALL render a circular avatar button in the top-right area of the page header for authenticated users. The avatar SHALL display 1–2 uppercase initials derived from the user's display name (first character of up to two whitespace-separated words) or, when display name is absent, the first two characters of the email local-part.

#### Scenario: Initials from display name with two words
- **WHEN** an authenticated user has `displayName` = "John Doe"
- **THEN** the avatar SHALL display "JD"

#### Scenario: Initials from single-word display name
- **WHEN** an authenticated user has `displayName` = "Mary"
- **THEN** the avatar SHALL display "M"

#### Scenario: Initials fallback to email
- **WHEN** an authenticated user has no `displayName` and `email` = "ab@church.org"
- **THEN** the avatar SHALL display "AB"

#### Scenario: Avatar not rendered when unauthenticated
- **WHEN** no user session exists (`user` is `null`)
- **THEN** the avatar component SHALL render nothing (null)

---

### Requirement: Dropdown opens on avatar click
The system SHALL open a dropdown menu when the user clicks the avatar button. The dropdown SHALL contain three items in order: (1) user email displayed as read-only text, (2) a "My Learning" navigation link, (3) a "Logout" action.

#### Scenario: Dropdown opens on click
- **WHEN** an authenticated user clicks the avatar button
- **THEN** a dropdown menu SHALL appear containing the user's email, a "My Learning" link, and a "Logout" button

#### Scenario: Dropdown closes on outside click
- **WHEN** the dropdown is open and the user clicks outside of it
- **THEN** the dropdown SHALL close

#### Scenario: Email is read-only
- **WHEN** the dropdown is open
- **THEN** the email item SHALL be non-interactive (not a link or button)

---

### Requirement: My Learning navigates to /my-learning
The system SHALL navigate the user to `/my-learning` when they select "My Learning" from the avatar dropdown.

#### Scenario: My Learning link click
- **WHEN** the user clicks "My Learning" in the avatar dropdown
- **THEN** the browser SHALL navigate to `/my-learning`
- **AND** the dropdown SHALL close

---

### Requirement: Logout signs out and redirects
The system SHALL call the `logout()` function from `AuthContext` and redirect to `/login` when the user selects "Logout" from the avatar dropdown.

#### Scenario: Logout action
- **WHEN** the user clicks "Logout" in the avatar dropdown
- **THEN** the system SHALL invoke `logout()` from `useAuth`
- **AND** the user SHALL be redirected to `/login`
- **AND** the user's access token SHALL be cleared from React state
