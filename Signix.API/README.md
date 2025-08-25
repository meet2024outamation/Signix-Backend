# Signix API - SigningRoom Endpoints

This document describes the Signix API endpoints for managing signing rooms using the Ardalis.ApiEndpoints architecture with **Ardalis Result pattern** and **PostgreSQL database**.

## ?? Quick Start

### 1. Database Setup
Since you have pgAdmin running, follow these steps:

1. **Create Database in pgAdmin:**
   - Database name: `SignixDb`
   - Owner: `postgres`

2. **Update Connection String** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=SignixDb;Username=postgres;Password=your_password;Port=5432;"
     }
   }
   ```

3. **Create and Apply Migrations:**
   ```bash
   dotnet ef migrations add InitialCreate --project Signix.API
   dotnet ef database update --project Signix.API
   ```

### 2. Run the Application
```bash
dotnet run --project Signix.API
```
The application will automatically:
- ? Test PostgreSQL connection
- ? Apply pending migrations
- ? Seed initial sample data
- ? Open your browser to **Swagger UI** at the root URL

## ??? Architecture

The API follows a clean architecture pattern with:

- **Database**: PostgreSQL with Entity Framework Core
- **Entities**: Located in `Signix.Entities` project - used directly in API responses
- **Requests**: Request models in `Signix.API/Models/Requests`
- **Services**: Business logic in `Signix.API/Infrastructure`
- **Endpoints**: Ardalis API endpoints in `Signix.API/Endpoints/SigningRooms`
- **Result Pattern**: Using `Ardalis.Result` for consistent error handling

## ?? API Endpoints

### 1. Get All Signing Rooms
- **URL**: `GET /api/signing-rooms`
- **Query Parameters**: 
  - `NotaryId` (optional): Filter by notary ID
  - `StatusId` (optional): Filter by status ID
  - `Name` (optional): Filter by name (contains)
  - `CreatedAfter` (optional): Filter by creation date
  - `CreatedBefore` (optional): Filter by creation date
  - `Page` (default: 1): Page number
  - `PageSize` (default: 10): Page size
- **Response**: `PagedResult<List<SigningRoom>>`

### 2. Get Signing Room by ID
- **URL**: `GET /api/signing-rooms/{id}`
- **Path Parameters**: 
  - `id`: Signing room ID
- **Response**: `Result<SigningRoom>`

### 3. Create Signing Room
- **URL**: `POST /api/signing-rooms`
- **Body**: `CreateSigningRoomRequest`
- **Response**: `Result<SigningRoom>` (201 Created)

### 4. Update Signing Room
- **URL**: `PUT /api/signing-rooms/{id}`
- **Path Parameters**: 
  - `id`: Signing room ID
- **Body**: `UpdateSigningRoomBody`
- **Response**: `Result<SigningRoom>`

### 5. Delete Signing Room
- **URL**: `DELETE /api/signing-rooms/{id}`
- **Path Parameters**: 
  - `id`: Signing room ID
- **Response**: `Result` (200 OK)

## ?? Data Models

### SigningRoom Entity (Direct API Response)
```json
{
  "id": 1,
  "name": "Contract Signing",
  "description": "NDA signing session",
  "originalPath": "/documents/original/contract.pdf",
  "signedPath": "/documents/signed/contract.pdf",
  "notaryId": 1,
  "createdAt": "2024-01-01T00:00:00Z",
  "startedAt": "2024-01-01T10:00:00Z",
  "completedAt": "2024-01-01T11:00:00Z",
  "createdBy": 1,
  "modifiedBy": 1,
  "statusId": 1,
  "metaData": {},
  "notary": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "notary@example.com",
    "metaData": {}
  }
}
```

### Ardalis Result Response Format
```json
{
  "status": 200,
  "success": true,
  "value": { /* Your data here */ },
  "errors": [],
  "validationErrors": [],
  "correlationId": "unique-id"
}
```

### Paged Result Response
```json
{
  "status": 200,
  "success": true,
  "value": [/* Array of items */],
  "pagedInfo": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 100,
    "totalPages": 10
  },
  "errors": [],
  "validationErrors": [],
  "correlationId": "unique-id"
}
```

## ?? Configuration

### Database Connection
Update `appsettings.json` with your PostgreSQL credentials:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SignixDb;Username=postgres;Password=your_password;Port=5432;"
  }
}
```

### Swagger Configuration
- **Development**: Automatically opens at root URL (`/`)
- **Production**: Swagger is disabled by default
- **Customization**: See `Program.cs` for Swagger configuration

## ?? Dependencies

### Core Packages
- **Ardalis.ApiEndpoints** 4.1.0 - Clean endpoint architecture
- **Ardalis.Result** 10.1.0 - Result pattern for error handling
- **Ardalis.Result.AspNetCore** 10.1.0 - ASP.NET Core integration

### Entity Framework & Database
- **Microsoft.EntityFrameworkCore** 9.0.8 - ORM
- **Npgsql.EntityFrameworkCore.PostgreSQL** 9.0.4 - PostgreSQL provider
- **Microsoft.EntityFrameworkCore.Tools** 9.0.8 - CLI tools
- **Microsoft.EntityFrameworkCore.Design** 9.0.8 - Design-time tools

### Documentation
- **Swashbuckle.AspNetCore** 9.0.3 - Swagger/OpenAPI

## ?? Development Features

### Database
- **PostgreSQL** with JSONB support for metadata
- **Automatic migrations** on startup in development
- **Connection testing** with detailed error logging
- **Sample data seeding** for immediate testing

### JSON Serialization
- `ReferenceHandler.IgnoreCycles` - Handles circular references
- `JsonIgnoreCondition.WhenWritingNull` - Excludes null values
- `[JsonIgnore]` attributes on navigation properties

### Code First Migrations
- Full migration support with EF Core CLI
- See `POSTGRESQL_SETUP.md` for detailed setup instructions
- Production-ready migration scripts

## ??? Database Schema

The application creates these tables in PostgreSQL:
- **`users`** - Notary users with email uniqueness
- **`clients`** - Client organizations
- **`signing_rooms`** - Main signing sessions
- **`signers`** - People who sign documents
- **`documents`** - Documents to be signed
- **`document_statuses`** - Status tracking for documents
- **`designations`** - User role designations

All tables use PostgreSQL-specific features like JSONB for metadata storage.

## ?? Production Considerations

1. **Use proper SSL** connections in production
2. **Configure connection pooling** for performance
3. **Set up database backups** and monitoring
4. **Use migration scripts** instead of automatic migration
5. **Configure proper logging** and error handling
6. **Disable Swagger** in production environment

## ?? Project Structure
```
Signix.API/
??? Endpoints/SigningRooms/     # Ardalis API endpoints
??? Infrastructure/             # Services and implementations
??? Models/Requests/           # Request DTOs
??? Extensions/               # Result extensions
??? Migrations/              # EF Core migrations (generated)
??? POSTGRESQL_SETUP.md      # Database setup guide
??? Program.cs              # Application configuration
```

## ?? Why Ardalis Result?

- ? **Consistent Error Handling** - Standardized error responses
- ? **Type Safety** - Compile-time error checking
- ? **HTTP Status Mapping** - Automatic status code handling
- ? **Validation Support** - Built-in validation error handling
- ? **Correlation IDs** - Request tracking and debugging

## ?? Additional Documentation

- **`POSTGRESQL_SETUP.md`** - Step-by-step PostgreSQL setup with pgAdmin
- **`MIGRATIONS.md`** - Entity Framework migration commands and best practices

---

**?? Ready to start!** Create your database in pgAdmin, update the connection string, run migrations, and start developing!