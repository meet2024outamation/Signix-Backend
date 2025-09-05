# Signix API - Digital Signing Platform

This document describes the Signix API endpoints for managing signing rooms and documents using the Ardalis.ApiEndpoints architecture with **Ardalis Result pattern**, **PostgreSQL database**, and **RabbitMQ messaging**.

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

### 2. RabbitMQ Setup (Optional but Recommended)
For document signing events:

1. **Install RabbitMQ** or run with Docker:
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```

2. **Configure RabbitMQ** in `appsettings.json`:
   ```json
   {
     "RabbitMQ": {
       "Hostname": "localhost",
       "Port": 5672,
       "Username": "guest",
       "Password": "guest"
     }
   }
   ```

### 3. Run the Application
```bash
dotnet run --project Signix.API
```
The application will automatically:
- ? Test PostgreSQL connection
- ? Setup RabbitMQ infrastructure (if available)
- ? Apply pending migrations
- ? Seed initial sample data
- ? Open your browser to **Swagger UI** at the root URL

## ??? Architecture

The API follows a clean architecture pattern with:

- **Database**: PostgreSQL with Entity Framework Core
- **Messaging**: RabbitMQ for document signing events ? **NEW**
- **Entities**: Located in `Signix.Entities` project - used directly in API responses
- **Requests**: Request models in `Signix.API/Models/Requests`
- **Services**: Business logic in `Signix.API/Infrastructure`
- **Endpoints**: Ardalis API endpoints in `Signix.API/Endpoints`
- **Result Pattern**: Using `Ardalis.Result` for consistent error handling

## ?? API Endpoints

### Signing Rooms API

#### 1. Get All Signing Rooms
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

#### 2. Get Signing Room by ID
- **URL**: `GET /api/signing-rooms/{id}`
- **Response**: `Result<SigningRoom>`

#### 3. Create Signing Room
- **URL**: `POST /api/signing-rooms`
- **Body**: `SigningRoomCreateRequest`
- **Response**: `Result<SigningRoom>` (201 Created)

#### 4. Update Signing Room
- **URL**: `PUT /api/signing-rooms/{id}`
- **Body**: `UpdateSigningRoomBody`
- **Response**: `Result<SigningRoom>`

#### 5. Delete Signing Room
- **URL**: `DELETE /api/signing-rooms/{id}`
- **Response**: `Result` (200 OK)

### Documents API

#### 1. List Documents
- **URL**: `GET /api/documents`
- **Key Features**:
  - ? **Optional SigningRoomId filtering** - Filter by specific signing room or get all documents
  - ? **Multiple filter options** - Client, status, file type, tags, name search
  - ? **Pagination support** - Efficient handling of large document collections
- **Query Parameters**:
  - `signingRoomId` (optional, int): **Filter documents by signing room ID**
  - `clientId` (optional, int): Filter by client
  - `documentStatusId` (optional, int): Filter by document status
  - `name` (optional, string): Search in document name
  - `fileType` (optional, string): Filter by file type
  - `docTags` (optional, string): Search in document tags
  - `page` (default: 1): Page number
  - `pageSize` (default: 10): Page size
- **Response**: `PagedResult<List<Document>>`

#### 2. Get Document by ID
- **URL**: `GET /api/documents/{id}`
- **Response**: `Result<Document>`

#### 3. Create Document
- **URL**: `POST /api/documents`
- **Body**: `DocumentCreateRequest`
- **Response**: `Result<Document>` (201 Created)

#### 4. Update Document
- **URL**: `PUT /api/documents/{id}`
- **Body**: `UpdateDocumentBody`
- **Response**: `Result<Document>`

#### 5. Delete Document
- **URL**: `DELETE /api/documents/{id}`
- **Response**: `Result` (200 OK)

#### 6. Sign Documents ? **NEW WITH RABBITMQ**
- **URL**: `POST /api/sign-documents`
- **Description**: Sign multiple documents and publish event to RabbitMQ
- **Body**: 
  ```json
  {
    "signningRoomId": 1,
    "documentId": [1, 2, 3]
  }
  ```
- **Response**: `Result<int>` (number of documents signed)
- **RabbitMQ Event**: Publishes `DocumentSignedMessage` to `document.signed.queue`

#### Example RabbitMQ Message:
```json
{
  "signingRoomId": 1,
  "documentIds": [1, 2, 3],
  "signedDocuments": [
    {
      "id": 1,
      "name": "NDA_Template.pdf",
      "clientName": "Acme Corporation",
      "previousStatusId": 1,
      "newStatusId": 2,
      "statusName": "Signed"
    }
  ],
  "timestamp": "2024-08-25T10:30:00.000Z",
  "eventType": "DocumentSigned",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

## ?? Data Models

### SigningRoom Entity
```json
{
  "id": 1,
  "name": "Contract Signing",
  "description": "NDA signing session",
  "originalPath": "/documents/original/contract.pdf",
  "signedPath": "/documents/signed/contract.pdf",
  "notaryId": 1,
  "createdAt": "2024-01-01T00:00:00Z",
  "notary": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "notary@example.com"
  }
}
```

### Document Entity
```json
{
  "id": 1,
  "name": "NDA_Template.pdf",
  "description": "Standard non-disclosure agreement template",
  "clientId": 1,
  "fileSize": 524288,
  "fileType": "application/pdf",
  "docTags": "NDA,Legal,Template",
  "signingRoomId": 1,
  "documentStatusId": 1,
  "documentStatus": {
    "id": 1,
    "name": "Pending",
    "description": "Document is pending review"
  },
  "client": {
    "id": 1,
    "name": "Acme Corporation"
  }
}
```

## ?? Configuration

### Database Connection
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SignixDb;Username=postgres;Password=your_password;Port=5432;"
  }
}
```

### RabbitMQ Configuration ? **NEW**
```json
{
  "RabbitMQ": {
    "Hostname": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "ExchangeName": "signix.documents.exchange",
    "DocumentSignedQueueName": "document.signed.queue"
  }
}
```

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

### Messaging ? **NEW**
- **RabbitMQ.Client** 7.1.2 - RabbitMQ messaging client

### Documentation
- **Swashbuckle.AspNetCore** 9.0.3 - Swagger/OpenAPI

## ??? Database Schema

The application creates these tables in PostgreSQL:
- **`users`** - Notary users with email uniqueness
- **`clients`** - Client organizations
- **`signing_rooms`** - Main signing sessions
- **`documents`** - Documents within signing rooms
- **`document_statuses`** - Document workflow statuses (Pending, Signed, Completed)
- **`signers`** - People who sign documents
- **`designations`** - User role designations

## ?? Sample Data

After successful startup, the application automatically creates:
- **2 Users** (Notaries): John Doe, Jane Smith
- **1 Client**: Acme Corporation
- **2 Signing Rooms**: NDA Signing Session, Contract Signing
- **2 Documents**: NDA_Template.pdf, Service_Agreement.pdf
- **3 Document Statuses**: Pending, Signed, Completed

## ?? RabbitMQ Integration Features ? **NEW**

### Event-Driven Architecture
- **Document Signing Events**: Automatic message publication when documents are signed
- **Rich Message Content**: Includes all document details, client info, and status changes
- **Correlation Tracking**: Unique correlation IDs for event tracking
- **Resilient Messaging**: Persistent messages with automatic retry

### Use Cases
- **Real-time Notifications**: Email/SMS notifications when documents are signed
- **Audit Logging**: Centralized audit trail for compliance
- **Workflow Automation**: Trigger downstream processes
- **Reporting**: Real-time analytics and reporting
- **Integration**: Connect with external systems (CRM, ERP, etc.)

### Consumer Examples
- **.NET Applications**: Direct RabbitMQ client integration
- **Microservices**: Event-driven microservice communication
- **External Systems**: Any system that can consume AMQP messages

## ?? Production Considerations

1. **Database**: Use proper SSL connections and connection pooling
2. **RabbitMQ**: Configure clustering, SSL, and proper credentials
3. **Monitoring**: Set up health checks and logging
4. **Security**: Implement authentication and authorization
5. **Scaling**: Configure horizontal scaling for high load

## ?? Project Structure
```
Signix.API/
??? Endpoints/
?   ??? SigningRooms/           # Signing room endpoints
?   ??? Documents/              # Document endpoints
??? Infrastructure/
?   ??? Messaging/              # RabbitMQ services ? NEW
?   ??? ISigningRoomService.cs
?   ??? SigningRoomService.cs
?   ??? IDocumentService.cs
?   ??? DocumentService.cs
??? Models/
?   ??? Requests/              # Request DTOs
?   ??? Messages/              # RabbitMQ message models ? NEW
??? Extensions/               # Result extensions
??? Program.cs               # Application configuration
```

## ?? Additional Documentation

- **`RABBITMQ_INTEGRATION.md`** ? **NEW** - Detailed RabbitMQ integration guide
- **`DOCUMENTS_API.md`** - Detailed Documents API documentation
- **`POSTGRESQL_SETUP.md`** - PostgreSQL setup with pgAdmin
- **`MIGRATIONS.md`** - Entity Framework migration commands

## ?? Key Features Summary

### ? **Document Management with Messaging**
- Complete CRUD operations for documents
- Optional signing room filtering
- **RabbitMQ event publishing** for document signing
- Rich event data with correlation tracking

### ? **Enterprise-Ready Architecture**
- Consistent error handling with Ardalis.Result
- PostgreSQL with JSONB support
- Event-driven design with RabbitMQ
- Comprehensive logging and monitoring

### ? **Developer Experience**
- Swagger UI with detailed documentation
- Automatic database setup and seeding
- Graceful degradation if RabbitMQ is unavailable
- Rich error messages and correlation IDs

---

**?? Ready for enterprise use!** Complete API with database persistence, event-driven architecture, and comprehensive documentation!