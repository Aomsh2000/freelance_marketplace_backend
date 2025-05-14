# 🖥️ Freelancer Marketplace – Backend

This is the **backend API** for the **Freelancer Marketplace** platform, built as part of the **Elm Dev Training Program - Web Track**.  
It handles user authentication, project management, proposal submissions, chat messaging, and payment processing.

The backend is developed using **.NET 7**, integrated with **SignalR** for real-time chat, **Stripe** for payment handling, and **Firebase** for authentication validation.

---

## 🚀 Key Features

- **Firebase Token Authentication**: Validates Firebase ID tokens for secure access.
- **RESTful API**: Provides endpoints for managing users, projects, proposals, payments, and chat.
- **SignalR Real-Time Chat**: Enables instant messaging between users.
- **Stripe Integration**: Supports secure and trackable payment operations.
- **Role Flexibility**: Each user can act as both a client and a freelancer.
- **Cross-Origin Support (CORS)**: Configured for Angular frontend consumption.

---

## 🗂️ Project Structure

```txt
freelance_marketplace_backend/
├── Controllers/                   # API controllers (Projects, Proposals, Chat, Payments, etc.)
├── Data/                          # Entity Framework DB context and seed data
├── DTOs/                          # Data Transfer Objects
├── Hubs/                          # SignalR chat hub
├── Interfaces/                    # Interfaces for repositories
├── Migrations/                    # EF Core migration files
├── Models/                        # Entity models (User, Project, Proposal, etc.)
├── Repositories/                 # Repository implementations for DB access
├── Services/                     # Business logic (e.g., FirebaseAuthService, StripeService)
├── appsettings.json              # Application configuration
├── Program.cs                    # Main entry point
├── Startup.cs                    # App service configuration and middleware
└── README.md                     # Project documentation
```

## 🔧 Getting Started

### 📦 Prerequisites
- **.NET 7 SDK**
- **A configured Firebase project (for token validation)**
- **A Stripe account with API keys**

### ⚙️ Setup Instructions

### 1. Clone the Repository

```txt
git clone https://github.com/turki-aloufi/freelance_marketplace_backend.git
cd freelance_marketplace_backend
```

### 2. Restore Dependencies
<pre> ```dotnet restore``` </pre>

### 3. Configure Your Environment
Update the appsettings.json file with your Firebase project ID, Stripe keys, and database connection string:

```txt
{
  "Firebase": {
    "ProjectId": "YOUR_FIREBASE_PROJECT_ID"
  },
  "Stripe": {
    "SecretKey": "YOUR_STRIPE_SECRET_KEY",
    "PublishableKey": "YOUR_STRIPE_PUBLISHABLE_KEY"
  },
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_SQL_CONNECTION_STRING"
  }
}
```

### 4. Apply Database Migrations
<pre> ```dotnet ef database update``` </pre>

### 5. Run the Server
<pre> ```dotnet run``` </pre>


By default, the API will be available at: `https://localhost:5001`

---

## 🔗 Frontend Repository

The frontend of this project is built using Angular:  

`https://github.com/turki-aloufi/freelance_marketplace`

---

## 👥 Team Members

This project was developed by the following team as part of our Elm Dev Training Program - Web Track:

- **Areej shareefi**  –  Full Stack Web Developer
- **Osama Alhejaily** –  Full Stack Web Developer
- **Razan Al-ahmadi** –  Full Stack Web Developer
- **Reham Alsaedi**  –   Full Stack Web Developer
- **Shadia Almutairi** – Full Stack Web Developer
- **Turki Aloufi**   –   Full Stack Web Developer
