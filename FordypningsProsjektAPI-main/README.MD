Fordypningsprosjekt API
A minimal ASP .NET Core Web API for a student game project, providing:

User registration & login with JWT authentication
Leaderboard and high-score endpoints
v1 URL-segment API versioning
HTTPS/HSTS, Serilog rolling-file logs, rate-limiting on login
Swagger/OpenAPI documentation

---

Prerequisites
.NET 8 SDK
MySQL server (local or remote)
(Optional) MySQL Workbench for inspecting the database

---

1. Configure appsettings.json which looks like this:
2. {
   "ConnectionStrings": {
   "DefaultConnection": "Server=localhost;Database=game_db;User=root;Password=YourPassword;"
   },
   "Jwt": {
   "Key":      "AReallySecretKeyYouGenerate",
   "Issuer":   "FordypningsprosjektAPI",
   "Audience": "FordypningsprosjektAPI"
   }
   }
So you have to update this file with the appropriate information for your database.

3. Build and apply migrations
Run these commands in the project root:
dotnet restore
dotnet build
dotnet ef database update

4. Now you simply run the project you can do this through the IDE you are currently using or
just use the terminal/shell.

Some errors to consider and what they mean:
“Cannot connect to database”: Ensure MySQL is running and your connection string is correct.

“Unauthorized”: Check that you included Authorization: Bearer <token> exactly as shown.

Rate limit exceeded (429): You can only attempt login 5 times in 15 minutes per IP.