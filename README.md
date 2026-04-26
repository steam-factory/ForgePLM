=======
# ForgePLM

ForgePLM is an opinionated PLM workflow for SolidWorks-based teams.

It is designed around ECO-centered change control, disciplined part numbering, revision control, and a clean separation between engineering work and production release records.

## Current Status

ForgePLM is in early public-preview preparation. The current system includes:

- SQL Server database schema
- Static part category seed data
- WPF Admin application
- ASP.NET Runtime API
- SolidWorks Add-In integration

## Core Philosophy

ForgePLM prioritizes clarity, traceability, and controlled engineering release discipline over convenience shortcuts.

## Repository Structure

- `src/ForgePLM.Admin` — WPF administrative application
- `src/ForgePLM.Runtime` — ASP.NET API/runtime service
- `src/ForgePLM.SolidWorks.Addin` — SolidWorks task pane add-in
- `db/schema` — SQL Server schema scripts
- `db/seed` — static reference data
- `docs` — setup and usage documentation

> 💡 Tip: If you're using VS Code, open the repo with `code .` before running setup.


## IMPORTANT
> ⚠️ Important: Set `ForgePLM.Runtime.Host` as the startup project before running.
>>>>>>> 882d1ff2a985b82e2262dcac854554ed5df554d1
