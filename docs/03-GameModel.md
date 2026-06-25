# Game Model

## Goal

Define the structure of a learning game.

A learning game is a collection of interactive learning tasks created by AI and controlled by the teacher.

---

## Main Object

### Learning Game

A learning game is the main container of the learning experience.

A learning game contains:

* Game Information
* Learning Tasks
* Results

---

## Game Information

A learning game includes:

* Grade Level
* Subject
* Topic
* Indoor or Outdoor
* Number of Students

The teacher defines this information before AI generates the game.

---

## Learning Tasks

A learning game consists of one or more learning tasks.

The teacher defines:

* Task Types
* Number of Tasks

AI generates the content for each task.

---

## Task Types

### Quiz Task

Students answer multiple-choice questions.

---

### QR Code Task

Students scan a QR code to complete the task.

QR tasks include a time limit.

If the QR code is scanned before the time limit, the student earns points and continues.

If the time limit expires, the student receives no points and automatically continues to the next task.

---

### GPS Task

Students reach a teacher-defined target location to complete the task.

GPS tasks should remain simple.

Teachers define the play area and GPS target locations.

AI generates educational instructions but does not create physical locations.

Students receive simple directional guidance through an on-screen arrow.

The application does not display maps or distance information.

When the student reaches the target area, the task is completed automatically.

---

## Task Order

AI generates an initial task sequence.

The teacher can reorder tasks at any time before publishing.

The final task order is always controlled by the teacher.

Example:

* Quiz
* GPS
* QR Code
* Quiz
* GPS
* QR Code
* Quiz

---

## AI Responsibilities

AI should:

* Generate the first draft of the learning game.
* Generate the content of each task.
* Respect the structure defined by the teacher.
* Create age-appropriate and engaging activities.
* Respect teacher-defined physical locations and play areas.

---

## Teacher Responsibilities

The teacher should:

* Define the game structure.
* Select task types.
* Select the number of tasks.
* Review AI-generated content.
* Edit any AI-generated content.
* Reorder tasks.
* Publish the final learning game.

---

## Student Experience

Students play the learning game using their mobile devices.

Students complete tasks one by one.

Students earn points by completing activities and answering questions correctly.

---

## Game Model Principles

* A learning game is a collection of tasks.
* Teachers define the structure.
* AI generates the content.
* AI creates the first draft.
* Teachers stay in control.
* Teachers can edit AI-generated content.
* Teachers can reorder tasks.
* Tasks are completed sequentially.
* Interactive learning should remain simple and engaging.

---

## Expected Outcome

A teacher should be able to define a game structure and receive a complete AI-generated learning game that can be reviewed, reordered, edited, and published with minimal effort.
