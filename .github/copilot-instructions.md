# Copilot Instructions

## Project Guidelines
- User prefers API parameters and field names to use specific identifiers (e.g., yearLevelId, studentId) instead of generic id where possible.
- Team prefers consolidating student-related functionality (enrollments, grades, year levels) under students endpoints instead of separate controllers when feasible.
- Endpoints should expose explicit parameters and response schemas in Swagger for easier debugging; avoid vague IActionResult/anonymous responses when possible.
- For calendar visibility, advisers/chairmen should only see appointments for students in the year levels assigned to them via adviser assignments.