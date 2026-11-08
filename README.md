# Sagar Courier Management System

A web-based Courier Management System developed using **ASP.NET Core MVC, ASP.NET Core Web API, C#, ADO.NET, SQL Server, Bootstrap 5, and jQuery AJAX**.

The application provides shipment management and staff administration through role-based access for Admin and Staff users.

## Project Overview

Sagar Courier Management System allows administrators and staff members to manage courier shipments through a centralized web application.

The system follows an MVC + Web API approach:

- **ASP.NET Core MVC** handles page routing and rendering Razor views.
- **ASP.NET Core Web API** handles business operations and HTTP requests.
- **ADO.NET** communicates with SQL Server.
- **jQuery AJAX** calls API endpoints without requiring full-page reloads.
- **Session-based authentication** maintains the logged-in user's identity and role.

## Technology Stack

| Category | Technologies |
|---|---|
| Backend | C#, ASP.NET Core MVC |
| API | ASP.NET Core Web API |
| Database | Microsoft SQL Server |
| Data Access | ADO.NET |
| Frontend | HTML5, CSS3, Bootstrap 5 |
| JavaScript | JavaScript, jQuery, AJAX |
| Authentication | Session-based authentication |
| Password Security | BCrypt password hashing |
| Version Control | Git and GitHub |

## System Architecture

The application separates page rendering from API operations.

![Sagar Courier Management System Architecture](https://gitdiagram.com/7788551/sagarcourierapp/diagram.png)

[View Interactive Architecture Diagram](https://gitdiagram.com/7788551/sagarcourierapp)

### Architecture Components

**1. Web Experience – MVC Layer**

Responsible for handling browser page requests and rendering Razor views.

- DashboardController
- AccountController
- ShipmentController

**2. API Operations – API Layer**

Responsible for processing AJAX requests and performing application operations.

- ShipmentApiController
- Account API
- StaffApiController

**3. Domain and Storage Layer**

Contains the application's models and SQL Server database.

- Shipment.cs
- User.cs
- SQL Server database

### Request Flow

1. User requests a page through the browser.
2. ASP.NET Core MVC routing identifies the appropriate controller.
3. The controller returns the corresponding Razor view.
4. The view uses jQuery AJAX to communicate with the Web API.
5. The API validates the request and performs the required operation.
6. ADO.NET executes SQL queries against SQL Server.
7. The API returns a JSON response.
8. The view updates the user interface using the response.

## User Roles and Permissions

The application supports two roles.

### Admin

Admin has access to system-wide management operations.

- Access Admin Dashboard.
- Register new staff members.
- View registered staff members.
- Update staff information.
- Delete staff accounts.
- View all courier shipments.
- Create shipments.
- Update shipment information.
- Delete shipments.

### Staff

Staff members can manage their own shipments.

- Access Staff Dashboard.
- Create new shipments.
- View their own shipments.
- Update their own shipments.
- Delete their own shipments.

### Role-Based Access Control

| Operation | Admin | Staff |
|---|---|---|
| View all shipments | Yes | No |
| View own shipments | Yes | Yes |
| Create shipment | Yes | Yes |
| Update shipment | Yes | Yes |
| Delete shipment | Yes | Yes |
| Register staff | Yes | No |
| View staff | Yes | No |
| Update staff | Yes | No |
| Delete staff | Yes | No |

Staff shipment access is restricted using the logged-in user's `UserId` stored in Session. The API determines ownership on the server rather than trusting a UserId submitted by the browser.

## Main Features

### 1. Authentication and User Management

- User registration and login.
- Password hashing using BCrypt.
- Session-based user authentication.
- Role-based access for Admin and Staff.
- Session-based user identification.

### 2. Shipment Management

- Create courier shipments.
- Automatically generate unique tracking numbers.
- View shipment records.
- Retrieve individual shipments by ID.
- Update shipment details.
- Delete shipment records.
- Associate shipments with their creator.

### 3. Shipment Status Management

Supported shipment statuses:

- Pending
- PickedUp
- InTransit
- Delivered
- Cancelled

### 4. Staff Management

- Register staff accounts.
- Retrieve staff records.
- Update staff information.
- Delete staff accounts.

### 5. AJAX-Based Operations

jQuery AJAX is used to communicate with Web API endpoints.

This provides:

- Asynchronous data retrieval.
- Dynamic table updates.
- Form submissions without full-page reloads.
- API-based CRUD operations.
- JSON request and response handling.

## Database Design

The application uses Microsoft SQL Server for persistent data storage.

### Roles Table

Stores the available application roles.

| Column | Description |
|---|---|
| RoleId | Primary key |
| RoleName | Role name |

### Users Table

Stores Admin and Staff account information.

| Column | Description |
|---|---|
| UserId | Primary key |
| FullName | User's full name |
| Email | Unique email address |
| PasswordHash | BCrypt password hash |
| RoleId | Foreign key referencing Roles |
| CreatedAt | Account creation date |

### Shipments Table

Stores courier shipment information.

| Column | Description |
|---|---|
| ShipmentId | Primary key, identity column |
| TrackingNumber | Unique shipment tracking number |
| SenderName | Sender's name |
| ReceiverName | Receiver's name |
| ReceiverPhone | Receiver's phone number |
| PickupAddress | Shipment pickup address |
| DeliveryAddress | Shipment delivery address |
| Weight | Shipment weight |
| Status | Current shipment status |
| CreatedAt | Shipment creation date |
| UserId | Foreign key referencing Users |

### Database Relationships

- Roles to Users: One-to-Many.
- Users to Shipments: One-to-Many.
- Each shipment is associated with the user who created it.

## API Endpoints

The Shipment API uses the following endpoints:

| HTTP Method | Endpoint | Description |
|---|---|---|
| GET | `/api/ShipmentApi` | Retrieve shipments based on user role |
| GET | `/api/ShipmentApi/{id}` | Retrieve shipment by ID |
| POST | `/api/ShipmentApi` | Create shipment |
| PUT | `/api/ShipmentApi/{id}` | Update shipment |
| DELETE | `/api/ShipmentApi/{id}` | Delete shipment |

### Example: Create Shipment

**Request**

`POST /api/ShipmentApi`

```json
{
  "senderName": "Kamran Khan",
  "receiverName": "Arham Hussain",
  "receiverPhone": "7255982472",
  "pickupAddress": "Gurugram",
  "deliveryAddress": "Delhi",
  "weight": 5
}
```

The API obtains the logged-in user's `UserId` from Session and generates the tracking number on the server.

### Example: Successful Response

```json
{
  "success": true,
  "message": "Shipment created successfully!",
  "shipmentId": 1,
  "trackingNumber": "SHP1234567890123"
}
```

*The response above is an illustrative example.*

## Project Structure

```text
SagarCourierApp/
│
├── ApiController/
│   ├── AccountApiController.cs
│   ├── ShipmentApiController.cs
│   └── StaffApiController.cs
│
├── Controllers/
│   ├── AccountController.cs
│   ├── DashboardController.cs
│   ├── ShipmentController.cs
│   └── StaffController.cs
│
├── Models/
│   ├── Shipment.cs
│   ├── User.cs
│   └── ErrorViewModel.cs
│
├── Views/
│   ├── Account/
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   │
│   ├── Dashboard/
│   │   ├── AdminDashboard.cshtml
│   │   └── StaffDashboard.cshtml
│   │
│   ├── Shipment/
│   │   ├── CreateShipment.cshtml
│   │   ├── GetShipments.cshtml
│   │   └── UpdateShipment.cshtml
│   │
│   ├── Staff/
│   │   ├── GetStaff.cshtml
│   │   └── UpdateStaff.cshtml
│   │
│   └── Shared/
│       ├── _Layout.cshtml
│       └── Error.cshtml
│
├── wwwroot/
├── appsettings.json
├── Program.cs
└── SagarCourierApp.csproj
```

*The structure above represents the main application files; additional generated or supporting files may exist.*

## Getting Started

### Prerequisites

Install the following:

- Visual Studio 2022 or later.
- .NET SDK compatible with the project target framework.
- Microsoft SQL Server.
- SQL Server Management Studio (SSMS).
- Git.

### 1. Clone the Repository

```bash
git clone https://github.com/7788551/sagarcourierapp.git
```

### 2. Open the Project

Open `SagarCourierApp.sln` in Visual Studio.

### 3. Configure Database Connection

Update the `DefaultConnection` value in `appsettings.json` according to your local SQL Server instance.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=SagarCourierDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Do not commit real production credentials or secrets.

### 4. Create the Database

Create the `SagarCourierDB` database in SQL Server and execute the database table creation scripts.

Ensure the following tables exist:

- Roles
- Users
- Shipments

Configure the required foreign keys and constraints.

### 5. Restore Dependencies

```bash
dotnet restore
```

### 6. Build the Project

```bash
dotnet build
```

### 7. Run the Application

```bash
dotnet run
```

Alternatively, run the project using Visual Studio.

## Key Implementation Concepts

The project demonstrates practical implementation of:

- Object-Oriented Programming (OOP).
- MVC architecture.
- RESTful Web API development.
- ADO.NET database connectivity.
- SQL queries and parameterized commands.
- Session-based authentication.
- Role-based access control.
- BCrypt password hashing.
- jQuery AJAX.
- JSON serialization and deserialization.
- CRUD operations.
- Model binding and HTTP status codes.

## Future Enhancements

Potential improvements for future development:

- Shipment tracking through a public tracking number.
- Dashboard statistics and shipment analytics.
- Pagination, sorting and searching.
- Shipment status history and audit logs.
- Email and SMS notifications.
- JWT-based authentication for external API clients.
- Automated unit and integration testing.
- CI/CD pipeline and deployment.
- Background jobs for scheduled shipment operations.

## Author

**Arshad Anwar**

Software Developer | ASP.NET Core | C# | SQL Server | ADO.NET

GitHub: [7788551](https://github.com/7788551)

---

*This project is developed as a practical demonstration of ASP.NET Core MVC, Web API, ADO.NET, SQL Server and role-based business application development.*
