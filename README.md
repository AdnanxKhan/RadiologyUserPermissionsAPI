# Radiology System – API Module

## 📌 Project Summary

This solution delivers a complete API module for create user and its permissions radiology system, developed using **ASP.NET Core Web API** (Visual Studio 2022), **ADO.NET**, and **SQL Server**. It includes user authentication, permission management, patient record handling, session management, and more.

---

## 🛠️ Technologies Used

- ASP.NET Core Web API (Visual Studio 2022)
- ADO.NET
- SQL Server (SSMS)
- Swagger (for API testing/documentation)

---

## 📂 Project Structure

- **Controllers/** – All API endpoints
- **Models/** – Data models
- **DTOs/** – Data Transfer Objects
- **Startup.cs / Program.cs** – ASP.NET Core setup
- **UserPermissionsApi.sln** – Solution file
- **Signatures/** – Folder for signature uploads
- **PatientAttachments/** – Folder for patient file uploads

---

## 🗂️ Database Setup

### Option 1: Using `.bak` File
1. Open SQL Server Management Studio (SSMS).
2. Right-click **Databases** → Restore Database.
3. Select the `.bak` file included in the package.

### Option 2: Using SQL Script
1. Open SSMS.
2. Open the `.sql` file provided (`CompleteDBScript.sql`).
3. Execute the script to generate schema and seed data.

---

## ⚙️ Configuration

Edit the `appsettings.json` file with your SQL Server settings:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ClientTask;Trusted_Connection=True;"
}
Replacer server name with your server name.
