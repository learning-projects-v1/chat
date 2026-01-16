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

## Screenshots
Authentication
<p align="center"> <img src="https://github.com/user-attachments/assets/10f43200-c687-43d4-ac0d-23e00d5b41c5" alt="login" width="800" /> </p>

Login Screen
User authentication using JWT, providing secure access to the application.

Connections & Social Features
<p align="center"> <img src="https://github.com/user-attachments/assets/fe4f39dd-674f-4d7a-9658-933131a3d863" alt="connections" width="800" /> </p>

Connections Overview
View existing friends, pending requests, and suggested users.

<p align="center"> <img src="https://github.com/user-attachments/assets/c083db00-f17a-480d-99ae-58b6af4a3b26" alt="send-friend-request" width="800" /> </p>

#### Send Friend Request
Initiate a connection request to another user.

#### Messaging Overview
<p align="center"> <img src="https://github.com/user-attachments/assets/098c2c03-ccf3-4157-9169-6ef610562a08" alt="messages" width="800" /> </p>

#### Chat Overview
List of recent conversations with quick access to active chats.

#### Real-Time Chat
<p align="center"> <img src="https://github.com/user-attachments/assets/72d12355-e8ca-4a12-b276-4eefcc4771c9" alt="chat" width="800" /> </p>

#### Message Interactions
<p align="center"> <img src="https://github.com/user-attachments/assets/f0f3ff03-69fc-4b31-be1a-b67d945b26b2" alt="reply" width="800" /> </p>

#### Reply to Message
Reply to a specific message within a conversation thread.

<p align="center"> <img src="https://github.com/user-attachments/assets/47a1648f-3302-49af-ba3a-5763c7e84f0c" alt="react" width="800" /> </p>

#### Message Reactions
React to messages for quick, expressive feedback.

<p align="center"> <img src="https://github.com/user-attachments/assets/d88fe01e-500f-48e1-905a-52f32409fc61" alt="seen-status" width="800" /> </p>

#### Seen Status Indicator
Displays message read/seen status in real time.

## Architecture
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
