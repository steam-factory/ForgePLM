
## Setup

From the repository root:

```powershell
.\scripts\setup.ps1
```

If PowerShell blocks the script

You may see a message about script execution being disabled.
Run the following once per session:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\setup.ps1
```
This does not change your system settings. It only applies to the current PowerShell window.



## Run the Runtime API

1. Open the solution in Visual Studio

2. Set the startup project:
   - Right-click `ForgePLM.Runtime.Host`
   - Select **Set as Startup Project**

3. Press **F5** or click **Run**

4. Confirm the API is running:
   - Navigate to:
     ```
     https://localhost:<port>/api/health
     ```

You should receive a successful response indicating the runtime is active.

## Running ForgePLM

### Runtime API

1. Open the solution in Visual Studio
2. Set `ForgePLM.Runtime.Host` as the startup project
3. Run the application

### ⚠️ Administrator Mode (when required)

Some components (particularly the SolidWorks Add-In and COM integration) may require Visual Studio to be run as Administrator.

If you encounter issues such as:
- Add-In failing to load in SolidWorks
- COM registration errors
- Access denied errors

Try restarting Visual Studio in Administrator mode.
