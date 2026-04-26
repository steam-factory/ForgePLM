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
