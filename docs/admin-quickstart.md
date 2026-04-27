# ForgePLM Quick Start

This guide will get you up and running with ForgePLM quickly.

> ⚠️ Before running ForgePLM, complete the Configuration step below.

---

## 🧠 Solution Overview

ForgePLM is split into two primary solutions:

### **Administrator Dashboard**
`ForgePLM.Administrator.slnx`  
UI for managing customers, projects, ECOs, and parts.

### **SolidWorks Add-In + Runtime**
`ForgePLM.SolidWorks.Addin.slnx`  
Handles runtime services and integrates directly with SolidWorks.

---

## ⚙️ Setup

From the repository root:

```powershell
.\scripts\setup.ps1
```

This will:
- Download `nuget.exe` if needed
- Restore dependencies
- Build all solutions
- Publish runtime and admin apps into `/run`

### ⚠️ If PowerShell blocks the script

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\setup.ps1
```

This applies only to the current session.

---

## 🔧 Configuration

A template is provided:

```
src/ForgePLM.Runtime.Host/appsettings.example.json
```

### Create your local config

```powershell
copy src\ForgePLM.Runtime.Host\appsettings.example.json src\ForgePLM.Runtime.Host\appsettings.json
```

### Update values

Edit `appsettings.json`:

- **ConnectionStrings → ForgePLMDb**
- **Paths → AdministratorExe**

Default path:

```
run\ForgePLM.Administrator\ForgePLM.Administrator.exe
```

### Notes

- `appsettings.json` is not committed
- Each user must configure their own environment

---

## 🗄️ Database Setup

### 1. Create database

```
ForgePLM
```

### 2. Run schema

```
db/schema/001_create_schema.sql
```

### 3. Run seed data

```
db/seed/001_seed_reference_data.sql
```

---

## 🚀 Running ForgePLM

### Start Runtime

```powershell
.un\ForgePLM.Runtime.Host\ForgePLM.Runtime.Host.exe
```

### Administrator Dashboard

Open:

```
ForgePLM.Administrator.slnx
```

Press **F5**

### SolidWorks Add-In

Open:

```
ForgePLM.SolidWorks.Addin.slnx
```

Set startup project:
- `ForgePLM.Runtime.Host`

Press **F5**

---

## 🔍 Verify Runtime

```
https://localhost:5269/swagger
```

---

## ⚠️ Administrator Mode

Run Visual Studio as Administrator if needed for:
- COM registration
- Add-in loading issues

---

## 💡 Tips

- Ensure correct startup project
- Runtime must be running
- Use `code .` for quick navigation

---

## 🧪 Status

- Runtime API: ✅
- Administrator UI: ✅
- SolidWorks Add-In: ✅

---

Welcome to ForgePLM 🚀
