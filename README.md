# User Management App

This is a simple ASP.NET Core web application with PostgreSQL backend for managing user accounts.

## Features

- User registration and login with authentication
- Block, unblock, and delete users (with support for self-block/delete)
- User management panel accessible only to authenticated users
- Clean Bootstrap 5 styling and tooltips
- Mobile-responsive design
- TempData alerts for success/error messages

## Technologies Used

- ASP.NET Core MVC
- PostgreSQL + Entity Framework Core
- Bootstrap 5
- Humanizer (for relative time)
- Git + GitHub for version control

## Getting Started

- Clone the repository
- Update `appsettings.Development.json` with your local DB credentials
- Run `dotnet ef database update` to apply migrations
- Build and run with `dotnet run`
