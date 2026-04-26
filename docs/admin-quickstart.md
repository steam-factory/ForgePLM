# ForgePLM Quick Start

This guide will get you up and running with ForgePLM in just a few minutes.

---

## 🧠 Solution Overview

ForgePLM is split into two primary solutions:

- **Administrator Dashboard**  

ForgePLM.Administrator.slnx

UI for managing customers, projects, ECOs, and parts.

- **SolidWorks Add-In + Runtime**  

ForgePLM.SolidWorks.Addin.slnx

Handles runtime services and integrates directly with SolidWorks.

---

## ⚙️ Setup

From the repository root, run:

```powershell
.\scripts\setup.ps1
```

This will:

download nuget.exe if needed
restore all dependencies (including legacy packages)
build all solutions
⚠️ If PowerShell blocks the script

You may see:
```powershell
running scripts is disabled on this system
```

Run the following once per session:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\setup.ps1
```

This does not change your system settings. It only applies to the current PowerShell window.

🚀 Running ForgePLM
🖥️ Administrator Dashboard

Open:

ForgePLM.Administrator.slnx
Press F5

✅ A full-screen dashboard will appear

⚙️ Runtime + SolidWorks Add-In

Open:

ForgePLM.SolidWorks.Addin.slnx
Set startup project:
Right-click ForgePLM.Runtime.Host
Select Set as Startup Project
Press F5

✅ You should see:

A system tray icon (runtime is active)
The ForgePLM Add-In available inside SolidWorks (right-side task pane)
🔍 Verify Runtime

Once running, confirm the API is active:

https://localhost:<port>/swagger

default port is 5269

You should receive a successful response.

⚠️ Administrator Mode (when required)

Some components (especially the SolidWorks Add-In and COM registration) may require Visual Studio to run as Administrator.

If you encounter:

Add-In not appearing in SolidWorks
COM registration errors
Access denied issues

👉 Restart Visual Studio as Administrator

💡 Tips
Make sure the correct Startup Project is selected before running
Runtime must be running for the Add-In to function properly
Use code . to quickly explore the repository in VS Code
🧪 Status

ForgePLM is currently in an early but functional state:

Runtime API: ✅ working
Administrator UI: ✅ working
SolidWorks Add-In: ✅ working

Welcome to ForgePLM 🚀