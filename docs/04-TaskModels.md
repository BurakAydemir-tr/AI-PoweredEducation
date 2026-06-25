# Task Models

## Goal

Define the supported task types and their expected behavior.

A learning game consists of one or more learning tasks.

Each task has its own gameplay, scoring rules, and completion conditions.

---

## Common Task Principles

All task types follow these rules:

* AI generates the initial task content.
* Teachers can edit any AI-generated content.
* Teachers can reorder tasks before publishing.
* Students complete tasks sequentially.
* Students should never become permanently blocked.
* Every task allows progression to the next task.

---

## Quiz Task

### Purpose

Allow students to answer educational questions.

### Gameplay

The student reads a question and selects one of four answer choices.

If the selected answer is incorrect, the student can try again.

The student cannot continue until the correct answer is found.

### Scoring

Correct on first attempt:

100 Points

Correct on second attempt:

75 Points

Correct on third attempt:

50 Points

Correct on fourth attempt:

25 Points

Each incorrect answer reduces the available score by 25 points.

Bonus points are not supported.

### Success Rule

The student selects the correct answer.

### Completion Rule

The student proceeds to the next task after selecting the correct answer.

### Teacher Controls

The teacher can:

* Edit the question.
* Edit answer choices.
* Edit the correct answer.

### AI Responsibilities

AI generates:

* Question.
* Four answer choices.
* Correct answer.

---

## QR Code Task

### Purpose

Encourage students to explore and interact with physical locations.

### Gameplay

The student searches for the assigned QR code and scans it.

A default time limit is assigned.

The teacher can modify the time limit before publishing.

### Scoring

If the QR code is scanned before the time limit:

100 Points

If the time limit expires:

0 Points

Bonus points are not supported.

### Success Rule

The student scans the correct QR code.

### Timeout Rule

If the time limit expires, the student receives zero points and automatically proceeds to the next task.

### Teacher Controls

The teacher can:

* Edit the QR task.
* Change the time limit.
* Print and place the generated QR codes.

### AI Responsibilities

AI generates:

* QR task instructions.
* QR codes required for the activity.

---

## GPS Task

### Purpose

Encourage students to reach a physical location.

### Gameplay

The student follows an on-screen directional arrow.

The interface displays only a simple directional arrow.

Maps and distance indicators are not displayed.

The objective is to reach a teacher-defined target location.

GPS tasks use a fixed 5-meter completion radius.

When the student reaches the target area, the task is completed automatically.

A default time limit is assigned.

The teacher can modify the time limit before publishing.

### Scoring

If the target location is reached before the time limit:

100 Points

If the time limit expires:

0 Points

Bonus points are not supported.

### Success Rule

The student reaches the target location.

### Timeout Rule

If the time limit expires, the student receives zero points and automatically proceeds to the next task.

### Teacher Controls

The teacher can:

* Edit the GPS task.
* Change the time limit.
* Define the play area.
* Define GPS target locations.

### AI Responsibilities

AI generates:

* GPS task instructions.
* Educational content for teacher-defined locations.

AI does not generate physical target locations.

---

## Task Model Principles

* Teachers define the structure.
* AI generates the content.
* Teachers stay in control.
* Students should never become permanently blocked.
* Tasks should be simple and engaging.
* Every completed or timed-out task allows progression.
* Bonus points are not used.

---

## Expected Outcome

Teachers can combine Quiz, QR Code, and GPS tasks into a single learning game.

Students can complete every learning game without becoming stuck, while teachers maintain full control over the educational experience.
