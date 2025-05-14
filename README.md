# 🛠️ Freelancer Marketplace – Backend

This project is the **backend service** for the **Freelancer Marketplace** Capstone Project, developed as part of the **Elm Dev Training Program - Web Track**.  
It powers the platform’s business logic, API communication, database management, real-time chat, authentication, and payment processing using **.NET 8**, **SignalR**, **Stripe**, **Twilio**, and **Entity Framework Core**.

The backend is built with **ASP.NET Core Web API** and provides a robust, scalable architecture for managing users, projects, chats, proposals, and payments.

---

## 🎯 Key Features

- **Unified User Model**: A single user can act as both a freelancer and client.
- **JWT Authentication**: Secure API access with bearer tokens.
- **Project & Proposal Management**: Create projects, submit and manage proposals.
- **Real-time Messaging**: Built with **SignalR** for dynamic client-server communication.
- **Stripe Payment Integration**: Secure payments between clients and freelancers.
- **Twilio SMS Integration**: Send SMS notifications.
- **EF Core ORM**: SQL Server integration via Entity Framework Core.
- **Caching with Redis**: Improve performance and scalability.
- **Swagger API Docs**: Interactive documentation for testing and reference.

---

## 🗂️ Project Structure

```txt
freelance_marketplace_backend/
 │
 ├──freelance_marketplace_backend/
 │  ├── Controllers/                      # API controllers (Chats, Projects, Users, etc.)
 │  ├── Data/                         
 │  │   ├── Repositories/                 # Repository implementations
 │  │   └── FreelancingPlatformContext    # EF Core DbContext and DB initializer  
 │  ├── Models/
 │  │    ├── Dtos/                        # Data Transfer Objects for API requests/responses
 │  │    └── Entities                     # Database models             
 │  ├── Interfaces/                       # Repository and service interfaces
 │  ├── Services/                         # Business logic layer (Auth, Payment, Twilio, etc.)
 │  ├── Hubs/                             # Chat Hub
 │  ├── appsettings.json                  # App configuration (connection strings, JWT, etc.)
 │  ├── Program.cs                        # App entry point and service registration
 │  └── FreelanceMarketplace.csproj       # Project file and NuGet dependencies
 │
 ├── freelance_marketplace.Tests/
 │  ├── Controllers/                      # Controllers Unit test
 │  └── FreelanceMarketplace.Tests.csproj # Test Project file and NuGet dependencies
 └── README.md                            # Project documentation
```

## 📦 Technologies Used

- **.NET 8**
- **Entity Framework Core 9**
- **SQL Server**
- **SignalR**
- **Stripe.NET**
- **Twilio**
- **Redis Caching**
- **Swashbuckle (Swagger)**

---

## ✅ Prerequisites

- [.NET SDK 8.0+](https://dotnet.microsoft.com/en-us/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server)
- [Redis](https://redis.io/) (optional, for caching)
- Stripe and Twilio accounts with API keys

---

## ⚙️ Setup Instructions

1. **Clone the repository**:
   ```bash
   git clone https://github.com/turki-aloufi/freelance_marketplace_backend.git
   cd freelance_marketplace_backend
   git checkout dev2
2. **Configure the environment**
Update appsettings.json with your own configuration:
```txt
{
  "ConnectionStrings": {
    "DefaultConnection": "your-sql-connection-string"
  },
  "Jwt": {
    "Key": "your-jwt-secret",
    "Issuer": "your-issuer"
  },
  "Stripe": {
    "SecretKey": "your-stripe-secret"
  },
  "Twilio": {
    "AccountSID": "your-sid",
    "AuthToken": "your-auth-token",
    "FromNumber": "+123456789"
  }
}
```
3. **Run the application**   <pre> ```dotnet run``` </pre>

4. **Visit Swagger UI**
Access the API documentation at:
`http://localhost:<port>/swagger`

---

## 🔗 Backend Repository

The backend for this project is developed using .NET and SignalR.  
You can find it here:  

`https://github.com/turki-aloufi/freelance_marketplace_backend`
---

## 👥 Team Members

This project was developed by the following team as part of our Elm Dev Training Program - Web Track:

- **Areej shareefi**  –  Full Stack Web Developer
- **Osama Alhejaily** –  Full Stack Web Developer
- **Razan Al-ahmadi** –  Full Stack Web Developer
- **Reham Alsaedi**  –   Full Stack Web Developer
- **Shadia Almutairi** – Full Stack Web Developer
- **Turki Aloufi**   –   Full Stack Web Developer
