# MVP Definition and Architecture Plan

## Goal

Define the minimum viable product (MVP) for AI-Powered Education.

The MVP should provide enough functionality for teachers to create interactive learning games and for students to complete them through a simple mobile experience.

The MVP should prioritize simplicity, teacher control, and AI-assisted content creation.

---

# MVP Principles

## AI First

AI is the preferred way to create learning games.

Teachers can use AI to generate learning content quickly.

---

## Teacher Control

Teachers always remain in control.

Teachers can edit AI-generated content or create tasks manually.

---

## Simple Student Experience

Students should complete learning games with minimal interaction.

---

## One Task at a Time

Students focus on a single task.

Future tasks are hidden until the current task is completed.

---

## No Permanent Blocking

Technical problems should never permanently block student progress.

---

## Validation Before Save

Incomplete or invalid tasks cannot be saved.

---

# Teacher MVP Capabilities

Teachers can:

* Create Learning Games
* Fill AI Game Creation Form
* Generate AI Drafts
* Edit Learning Games
* Add Manual Tasks
* Delete Tasks
* Change Task Order
* Publish Learning Games
* Activate Learning Games
* Deactivate Learning Games
* Clone Learning Games
* Review Results

---

# Student MVP Capabilities

Students can:

* Join Learning Games
* Enter Student Name
* Complete Learning Tasks
* Leave the Game
* Complete the Learning Game
* View Final Results

Students who leave the game start a new session if they join again.

---

# MVP Task Types

The MVP supports:

* Quiz Tasks
* QR Code Tasks
* GPS Tasks

No additional task types are included in the MVP.

---

# Business Rules

## Expected Number of Students

The expected number of students is informational.

It helps AI generate suitable activities.

It does not limit participation.

---

## Unique Student Names

Student names must be unique within a learning game.

If a name already exists, another name must be chosen.

---

## QR Permission

If camera access is denied or QR scanning fails:

* 0 points
* Continue to next task

---

## GPS Permission

If location access is denied or GPS fails:

* 0 points
* Continue to next task

---

# Task Validation

## Quiz

A Quiz Task requires:

* Question
* Four answer choices
* One correct answer

Otherwise, it cannot be saved.

---

## QR

A QR Task requires:

* AI generated QR Code
* Time limit

Otherwise, it cannot be saved.

---

## GPS

A GPS Task requires:

* Game area
* Target location
* Time limit

Otherwise, it cannot be saved.

---

# Teacher Results Dashboard

## Game Summary

Teachers can view:

* Expected Students
* Joined Students
* Completed Students

---

## Student Results

Teachers can view:

* Student Name
* Total Score
* Completion Status
* Completed Tasks
* Completion Time
* Unfinished Tasks
* Timed-Out Tasks

---

## Ranking

Students are automatically ranked by score.

Ranking is visible only to teachers.

---

# MVP Out of Scope

The following features are intentionally excluded from the MVP:

## Student Features

* Student Accounts
* Student Profiles
* Student Leaderboard

## Additional Task Types

* Photo Tasks
* Audio Tasks
* Video Tasks
* NFC Tasks
* AR Tasks

## Gameplay

* Team Play
* Live Multiplayer

## Platform

* LMS Integration
* Teacher Marketplace
* Offline Mode
* Push Notifications
* PDF Export
* Excel Export

## AI

* AI Learning Analytics
* Personalized Recommendations

---

# Teacher Frontend

Teachers can:

## AI Game Creation

* Fill AI Form
* Generate AI Draft

## Learning Game Editor

* View Tasks
* Edit Tasks
* Add Manual Tasks
* Delete Tasks
* Drag and Drop Task Order

## GPS Area Management

* Draw Game Area
* Create GPS Points
* Edit GPS Points

## Task Library

* View Previous Tasks
* Reuse Existing Tasks

## Student Preview

* Preview Student Experience

## Learning Game Management

* Publish
* Activate
* Deactivate
* Clone

## Results Dashboard

* View Results
* View Rankings
* View Participation

---

# Student Mobile Experience

## Join Screen

Students:

* Enter Game Code
* Enter Student Name
* Join Learning Game

---

## Task Screen

* One task per screen
* Current task progress displayed

Example:

Task 3/10

---

## Quiz Experience

Wrong answer:

"Incorrect Answer. Try Again."

After three incorrect attempts:

* Show correct answer
* Continue to next task

---

## QR Experience

Students:

* Read destination
* Press Scan QR button
* Scan QR Code

Correct:

* Success
* Next Task

Wrong:

* Wrong QR

Timeout:

* Time is Up
* Next Task

---

## GPS Experience

Students:

* Follow direction arrow

Target reached:

* Target Reached
* Next Task

Timeout:

* Time is Up
* Next Task

---

## Game Complete Screen

Students can view:

* Total Score
* Completed Tasks
* Total Completion Time

---

# MVP Success Criteria

The MVP is successful if:

* Teachers can create learning games in minutes.
* Teachers remain in control.
* Students can easily complete activities.
* AI accelerates content creation.
* Learning games work reliably in real classroom environments.
