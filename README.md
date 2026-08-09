# Mika Memorial Rideout

A respectful information and registration platform for a memorial rideout in remembrance of **Mika Nevio Teichmann**.

The application provides a central place for participants to find verified event information and register for the rideout. Its purpose is to support safe and reliable planning while treating the memorial and everyone involved with dignity and respect.

## In Remembrance

This project was created following the tragic death of Mika Nevio Teichmann in a motorcycle accident in Uelzen on 2 August 2026.

Further information is available from the following public sources:

- [Memorial page for Mika Nevio Teichmann](https://trauer.az-online.de/traueranzeige/mika-nevio-teichmann)
- [AZ Online report about the accident](https://www.az-online.de/uelzen/stadt-uelzen/toedlicher-unfall-in-uelzen-motorradfahrer-19-prallt-gegen-geparkten-lkw-und-stirbt-94425249.html)

Please treat these links, the memorial, the family, and all participants with respect.

## Purpose

The platform is intended to support the organization of a memorial rideout by providing:

- A central source of official event information
- Date, time, meeting point, route, and schedule information
- Participant registration
- Registration of accompanying passengers where applicable
- Organizer announcements and important updates
- Safety and conduct information
- An overview of expected participant numbers
- Privacy-conscious processing of registration data

The application is an organizational tool. It is not intended to replace instructions from the event organizers, authorities, emergency services, or traffic control personnel.

## Project Status

The project is currently under active development.

Event information displayed during development may be incomplete, provisional, or test data unless it is explicitly marked as confirmed by the organizers.

## Technology

The complete application is built with **.NET 10**.

### Frontend

- .NET 10
- Blazor WebAssembly
- Azure Static Web Apps
- Responsive user interface for desktop and mobile devices

### Backend

- .NET 10
- Azure Functions
- HTTP-triggered API endpoints
- Asynchronous request processing
- Cancellation support for longer-running operations
- Server-side validation of registration data

### Hosting

- Azure Static Web Apps
- Azure Functions API
- GitHub Actions for build and deployment

### Optional Azure Services

Depending on the final deployment configuration, the application may also use:

- Azure Table Storage or Azure Cosmos DB for registrations
- Azure Key Vault for protected configuration
- Azure Application Insights for technical monitoring
- Azure Communication Services for confirmation messages

Any optional service must be configured in accordance with the project's privacy and data-minimization requirements.

## Solution Structure

The exact project names may differ, but the solution follows this general structure:

```text
src/
├── MemorialRideout.Web/
│   ├── Components/
│   ├── Layout/
│   ├── Pages/
│   ├── Services/
│   ├── Properties/
│   │   ├── Resources.resx
│   │   └── Resources.de.resx
│   ├── Program.cs
│   └── MemorialRideout.Web.csproj
│
├── MemorialRideout.Api/
│   ├── Functions/
│   ├── Services/
│   ├── Validation/
│   ├── Persistence/
│   ├── Properties/
│   │   ├── Resources.resx
│   │   └── Resources.de.resx
│   ├── Program.cs
│   └── MemorialRideout.Api.csproj
│
├── MemorialRideout.Contracts/
│   ├── Requests/
│   ├── Responses/
│   ├── Models/
│   └── MemorialRideout.Contracts.csproj
│
└── MemorialRideout.Tests/
    ├── Api/
    ├── Services/
    ├── Validation/
    └── MemorialRideout.Tests.csproj
