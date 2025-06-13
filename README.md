# 🛡️ UserWorkload – Real-World User Management System (.NET 8 + Razor Pages)

A production-ready **User Management App** built with **.NET 8 Razor Pages**, architected for **secure**, **compliant**, and **cost-efficient** deployment using **Azure-native services**.

> 🚀 Not just a toy project — this is how real-world internal tools should be built.

---

## 📌 Live Repository

- 🔗 GitHub: [github.com/jitangupta/UserWorkload](https://github.com/jitangupta/UserWorkload)  
- 🐳 Docker Hub: [docker.com/r/jitangupta/user-workload](https://hub.docker.com/repository/docker/jitangupta/user-workload/general)

---

## 💡 Project Overview

This foundational app simulates internal workloads like admin panels, user management tools, or technical documentation dashboards.  
It follows modern best practices around:

- ✅ Azure-native networking & security
- ✅ Auto-scaling & CI/CD workflows
- ✅ Compliance-aware cloud architecture

---

## 🧰 Tech Stack

| Layer              | Technology Used                                      |
|--------------------|------------------------------------------------------|
| Frontend + Backend | **.NET 8 Razor Pages**                               |
| Hosting            | **Azure Container Apps** (with min-replica always-on)|
| Secrets            | **Azure Key Vault** (accessed via Managed Identity)  |
| Database           | **Azure SQL** (subnet-restricted private access)     |
| File Storage       | **Azure Blob Storage** (for profile images)          |
| Networking         | **Azure VNet with subnets for SQL & Container App**  |
| CI/CD              | **GitHub Actions → Docker Hub → Azure Container App**|

---

## 📦 Features

- 🔐 Secure login/logout with basic username/password auth  
- 👤 Create and list users  
- 🖼️ Upload profile picture to blob storage  
- 🌐 Custom domain bound via Namecheap with Azure-managed SSL  
- 🔄 Auto-scaling with min-1 replica to prevent cold starts  
- 🛡️ No public access to SQL — private subnet only  
- 🔑 Managed Identity-based Key Vault access  
- ⚙️ CI/CD pipeline using GitHub Actions + Docker Hub  

---

## 🗺️ Architecture Overview

```text
+----------------------+
|  .NET 8 Razor App    |
|  (Container App)     |
+----------------------+
           |
     Managed Identity
           |
+----------------------+
|   Azure Key Vault    |
| (App Secrets Store)  |
+----------------------+

           |
           VNet
     ┌────────────┐
     │  Subnet A  │ ── Azure Container App
     └────────────┘
           |
     ┌────────────┐
     │  Subnet B  │ ── Azure SQL Database (Private Endpoint)
     └────────────┘

           |
+----------------------+
| Azure Blob Storage   |
| (User profile photos)|
+----------------------+
```

## 🛠 Deployment Pipeline
```text
GitHub → Code Push to `main`
        ↓
GitHub Actions → Build & push to Docker Hub
        ↓
Azure Container App pulls latest image
        ↓
Deployment complete (auto-restart if needed)
```

## 📚 How to Use

### 1. Clone & Run Locally
```bash
git clone https://github.com/jitangupta/UserWorkload.git
cd UserWorkload

dotnet build
dotnet run
```
> Note: For local secrets, update appsettings.json.

### 2. CI/CD & Deployment
- PR to `main` branch triggers GitHub Action
- Docker image built and pushed to Docker Hub
- Azure Container App auto-deploys latest image

## 🌍 Use Cases
This app architecture can be adapted for:
- Internal admin tooling
- User management dashboards
- Lightweight SaaS MVPs
- Technical documentation CMS

## 📄 License
MIT License – free to use with attribution.