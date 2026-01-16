# Chat App

A full-stack real-time chat application built as a resume and learning project, focusing on clean architecture, modern web technologies, and extensibility for future system-design improvements.

## Tech Stack
### Backend

- ASP.NET 8 Web API
- SignalR for real-time messaging
- Entity Framework Core
- JWT Authentication

### Frontend
- Angular
- REST API + SignalR integration

### Database
- PostgreSQL

### Infrastructure
- Docker & Docker Compose (local development)

## Features
### Authentication & Users
- User registration and login
- JWT-based authentication with refresh token
- Secure API access

### Chat
- Real-time 1-to-1 messaging using SignalR
- Message persistence in PostgreSQL
- Message history loading

### Social / Connections
- Friend request system
- Accept / reject connections
- Friend suggestions
- Chat overview with recent conversations

### Presence
- Online / offline user status tracking

### Architecture
- Clear controller separation:
  - Auth
  - User
  - Friends
  - Messages
  - ChatOverview
- Designed with extensibility in mind for future scaling features

## Local Development Setup
### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK
- Node.js (for Angular, if running outside Docker)
### Clone the Repository
```
git clone <repo-url>
cd chat-app
```
### Start Services (Backend + Database)
```
docker-compose up --build
```

This will start:
- ASP.NET Web API
- PostgreSQL
### Run Frontend
```
cd frontend
npm install
ng serve
```
Frontend will be available at:
```
http://localhost:4200
```
Backend API:
```
http://localhost:5000
```

## Project Status

This project is actively evolving and currently runs locally only.
Cloud deployment has not yet been performed.

## Planned Enhancements

The architecture is intentionally designed to support future improvements, including:

- Redis caching
- Database sharding and archiving strategies
- GraphQL API layer
- Group chat
- Message replies and reactions
- Image/file sharing
- Cloud deployment (AWS / Azure)
- Scalability concepts (replicas, load balancing)
- These are planned extensions, not yet implemented.

## Purpose
This project is built to:
- Demonstrate full-stack development skills
- Showcase real-time systems using SignalR
- Apply clean backend design with ASP.NET
- Serve as a foundation for system design discussions in interviews

## Author
Jakir Hossen
Full-stack developer focused on the .NET ecosystem
