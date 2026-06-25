# Technical Decisions

## Goal

Define the approved technology stack, architecture decisions, and technical standards for AI-Powered Education.

All decisions in this document are considered frozen unless there is a strong business or technical reason to change them.

---

# Backend Technology Stack

## Language

**Decision**

C#

**Status**

✅ Frozen

---

## Framework

**Decision**

ASP.NET Core Web API

**Status**

✅ Frozen

---

## Version

**Decision**

.NET 9

**Reason**

* Approved project-wide implementation target
* ASP.NET Core and Entity Framework Core 9 integration

**Status**

✅ Frozen

---

# Backend Architecture

## Architecture Pattern

**Decision**

N-Layer Architecture

Layers:

* Core Layer
* Entity Layer
* Data Access Layer
* Business Layer
* API Layer

**Reason**

* Simpler than Clean Architecture
* Easy to understand and maintain
* Matches project complexity
* Matches team experience

**Status**

✅ Frozen

---

## Core Layer

The Core Layer contains reusable components that can be shared across projects.

Examples:

* JWT Helpers
* Base Response Models
* Pagination Models
* Constants
* Extensions
* Exceptions
* Utility Classes

---

# Business Layer Structure

**Decision**

Business Layer contains:

* Services
* Interfaces
* DTOs
* Validators
* Mappings

**Status**

✅ Frozen

---

## Validation Strategy

**Decision**

FluentValidation

**Reason**

* Clean validation rules
* ASP.NET Core integration
* Easy testing
* Widely adopted

**Status**

✅ Frozen

---

## Mapping Strategy

**Decision**

Manual Mapping

**Reason**

* Avoids production licensing requirements
* Keeps MVP mappings explicit
* Avoids an additional runtime dependency

**Status**

✅ Frozen

---

# Database

## Database Engine

**Decision**

PostgreSQL

**Reason**

* Open source
* Free
* Docker friendly
* Good spatial data support
* Strong EF Core integration

**Status**

✅ Frozen

---

## ORM

**Decision**

Entity Framework Core

**Status**

✅ Frozen

---

## Database Provider

**Decision**

Npgsql.EntityFrameworkCore.PostgreSQL

**Status**

✅ Frozen

---

# Authentication

## Teacher Authentication

**Decision**

ASP.NET Core Identity Core

JWT Access Token

Refresh Token

**Reason**

* Secure password management
* User management support
* Role support
* Industry standard approach

**Status**

✅ Frozen

---

## Student Authentication

**Decision**

Students do not have accounts.

Authentication flow:

* Game Code
* Student Name
* Session Token

**Reason**

* Fast participation
* No registration required
* Simplified student experience

**Status**

✅ Frozen

---

# Frontend

## Teacher Panel

**Decision**

React

TypeScript

Material UI

**Reason**

* Strong React ecosystem
* Type safety
* Fast development
* Large community support

**Status**

✅ Frozen

---

# Mobile Application

## Student Application

**Decision**

Flutter

Dart

Target Platforms:

* Android
* iOS

**Reason**

* Excellent cross-platform support
* Strong GPS support
* Strong QR and camera support
* High performance

**Status**

✅ Frozen

---

# Maps

## Map Provider

**Decision**

OpenStreetMap

**Status**

✅ Frozen

---

## Teacher Frontend Maps

**Decision**

React Leaflet

**Status**

✅ Frozen

---

## Student Mobile Maps

**Decision**

flutter_map

**Status**

✅ Frozen

---

# Artificial Intelligence

## AI Integration Strategy

**Decision**

Provider abstraction layer

Interface:

IAiProvider

Implementations:

* OpenAIProvider
* GeminiProvider

Default Provider:

OpenAI

**Reason**

* Prevent vendor lock-in
* Easy provider replacement
* Better long-term flexibility

**Status**

✅ Frozen

---

# File Storage

## Decision

No file storage solution is required for MVP.

Examples currently not required:

* MinIO
* Azure Blob Storage
* AWS S3

**Reason**

The MVP does not require file uploads or media storage.

**Status**

✅ Frozen

---

# Architecture Principles

* Keep the MVP simple.
* Prefer proven technologies over experimental technologies.
* Teachers remain in control of educational content.
* AI assists but does not replace teachers.
* Minimize operational complexity.
* Design for maintainability and future growth.
* Avoid unnecessary abstractions.
* Prefer simplicity over premature optimization.
