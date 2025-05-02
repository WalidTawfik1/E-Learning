# 🎓 E-Learning Platform – ASP.NET Core Web API

A modular, role-based learning management back-end system developed with ASP.NET Core Web API, Entity Framework Core, and SQL Server. This platform enables course creation, enrollment, reviews, and progress tracking with a secure and scalable architecture, suitable for online learning environments.

---

## 📌 Purpose

This platform demonstrates back-end development for a real-world e-learning system, supporting role-based access (Student, Instructor, Admin), media integration, and scalable architecture.

---

## 🧩 Core Features

- 🧑‍💻 **Authentication & Roles**: JWT + OTP login with roles for Students, Instructors, and Admins
- 📚 **Course Management**: Add, update, delete, and categorize courses
- 🎥 **Video Hosting**: Cloudinary integration for secure and fast media storage
- 📝 **Course Reviews**: Authenticated users can review and rate courses
- 📈 **Progress Tracking**: Tracks what students have completed
- 🔐 **Secure API**: Input validation, 2FA, role-based access control
- 🔁 **Scalable Design**: Built with Clean Architecture for long-term maintainability

---

## 🏗️ Architecture Layers

### 1. **Presentation Layer**
- API Controllers, Swagger, DTOs, Auth Middleware

### 2. **Core Layer**
- Domain models, interfaces, services, and business logic

### 3. **Infrastructure Layer**
- EF Core database access, Cloudinary integration, email services, repository pattern

---

## 💻 Technologies Used

| Category | Technologies |
|---------|--------------|
| **Backend** | ASP.NET Core Web API, EF Core |
| **Database** | SQL Server |
| **Auth** | JWT + OTP |
| **Media Hosting** | Cloudinary API |
| **Role Management** | Student / Instructor / Admin |
| **Documentation** | Swagger |
| **Mapping** | AutoMapper |

---

## 🚀 Getting Started

1. Clone the repository  
2. Update `appsettings.json` with DB, Cloudinary, and email credentials  
3. Apply EF Core migrations  
4. Run the API and test via Swagger

---

## 🔗 Links

- [Live Preview](https://elcentre.vercel.app/)
- [GitHub Repository](https://github.com/WalidTawfik1/E-Learning)

