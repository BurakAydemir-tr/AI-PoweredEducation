# Backend Architecture

## Goal

Define the core backend architecture of AI-Powered Education.

The backend should provide a simple, maintainable, and scalable foundation for managing learning games, student participation, task execution, and results.

The backend follows the principle that domain objects represent business concepts, while services manage business rules.

---

# Core Domain Objects

## LearningGame

### What is it?

A Learning Game is the main domain object of the platform.

It represents an educational game created by a teacher and played by students.

---

### What does it contain?

A Learning Game contains:

* Game Information
* Learning Tasks
* Results

---

### What can it do?

A Learning Game can:

* Edit
* Publish
* Activate
* Deactivate
* Clone

---

### Core Principles

* A Learning Game is teacher-owned.
* A Learning Game can contain AI-generated and manually created tasks.
* A Learning Game can be activated multiple times.
* Teachers remain in control of the final game content.

---

## LearningTask

### What is it?

A Learning Task is a single interactive activity within a Learning Game.

The MVP supports:

* Quiz Task
* QR Code Task
* GPS Task

---

### What does it contain?

A Learning Task contains:

* Task Type
* Task Content
* Task Order

---

### What can it do?

A Learning Task can:

* Edit
* Reorder
* Complete

---

### Core Principles

* A Learning Task belongs to one Learning Game.
* A Learning Task has one Task Type.
* Tasks are completed sequentially.
* Teachers can edit tasks.
* Tasks can be AI-generated or manually created.
* Students complete one task at a time.

---

## StudentSession

### What is it?

A Student Session represents a student's participation in a Learning Game.

A Student Session is created when a student joins an active learning game.

---

### What does it contain?

A Student Session contains:

* Student Name
* Learning Game
* Task Attempts
* Result

---

### What can it do?

A Student Session can:

* Join
* Start
* Complete Tasks
* Leave
* Finish

---

### Core Principles

* Students do not need accounts.
* Students join using a Game Code.
* Students provide a Student Name.
* Student Names must be unique within an active game.
* Leaving a game ends the current session.
* Rejoining creates a new session.
* Students complete tasks sequentially.
* Student progress and results belong to the Student Session.

---

## TaskAttempt

### What is it?

A Task Attempt represents a student's interaction with a single Learning Task.

---

### What does it contain?

A Task Attempt contains:

* Student Session
* Learning Task
* Attempt Count
* Score Earned
* Completion Status

---

### What can it do?

A Task Attempt can:

* Start
* Complete
* Timeout

---

### Core Principles

* A Task Attempt belongs to one Student Session.
* A Task Attempt belongs to one Learning Task.
* Each Student Session creates one Task Attempt for each Learning Task.
* Quiz answers are not stored.
* Only the number of attempts is stored.
* Task Attempts contribute to the final Result.

---

## Result

### What is it?

A Result represents the final outcome of a student's participation in a Learning Game.

A Result is automatically generated when a Student Session ends.

---

### What does it contain?

A Result contains:

* Student Session
* Total Score
* Completion Time
* Completed Tasks
* Unfinished Tasks
* Timed-Out Tasks
* Played At

---

### What can it do?

A Result can:

* Store Final Outcome
* Support Teacher Review

---

### Core Principles

* A Result belongs to one Student Session.
* A Result is generated automatically.
* A Result summarizes Task Attempts.
* A Result cannot be edited manually.
* Results are permanent records.
* Results support teacher analytics.
* Results include the date and time of gameplay.

---

# Service Layer

## LearningGameService

### What is it?

A LearningGameService manages the lifecycle and business rules of Learning Games.

---

### What does it manage?

* Learning Games
* Game Lifecycle
* Game States

---

### What can it do?

* Create Learning Games
* Edit Learning Games
* Publish Learning Games
* Activate Learning Games
* Deactivate Learning Games
* Clone Learning Games

---

### Core Principles

* Learning Games are teacher-owned.
* Learning Games can be edited before publication.
* Published games can be activated and deactivated.
* Learning Games can be activated multiple times.
* Cloned games start as Draft games.
* Learning Games with results cannot be deleted.

---

## LearningTaskService

### What is it?

A LearningTaskService manages the business rules of Learning Tasks.

---

### What does it manage?

* Quiz Tasks
* QR Code Tasks
* GPS Tasks
* Task Order

---

### What can it do?

* Create Tasks
* Edit Tasks
* Delete Tasks
* Reorder Tasks
* Validate Tasks

---

### Core Principles

* Tasks belong to one Learning Game.
* Tasks can be AI-generated or manually created.
* Teachers can edit tasks.
* Teachers can reorder tasks.
* Tasks must pass validation before saving.

---

## StudentSessionService

### What is it?

A StudentSessionService manages student participation in Learning Games.

---

### What does it manage?

* Student Join
* Student Sessions
* Task Attempts
* Final Results

---

### What can it do?

* Join Learning Games
* Create Student Sessions
* Start Tasks
* Complete Tasks
* Timeout Tasks
* Leave Learning Games
* Finish Learning Games
* Generate Results

---

### Core Principles

* Students do not need accounts.
* Students join using Game Code and Student Name.
* Student Names must be unique within an active game.
* Leaving a game ends the current session.
* Rejoining creates a new session.
* Task Attempts belong to Student Sessions.
* Results are generated automatically.

---

## AIService

### What is it?

An AIService manages AI-powered content generation.

---

### What does it manage?

* Quiz Generation
* QR Task Generation

---

### What can it do?

* Generate Quiz Tasks
* Generate QR Tasks
* Validate AI Responses

---

### Core Principles

* AI assists teachers.
* AI generates the first draft.
* Teachers remain in control.
* AI does not generate GPS locations.
* Teachers can manually create tasks.
* AI-generated content can be edited.

---

# Architecture Principles

* Domain Objects represent business concepts.
* Services manage business rules.
* Teachers always remain in control.
* AI assists but does not replace teachers.
* Students should never become permanently blocked.
* The backend should remain simple, maintainable, and scalable.
* MVP functionality should be prioritized over unnecessary complexity.
