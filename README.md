# Mika Memorial Rideout

A respectful information and registration platform for a memorial rideout in remembrance of **Mika Nevio Teichmann**.

The application provides a central place for participants to find verified event information and register for the rideout. Its purpose is to support safe and reliable planning while treating the memorial and everyone involved with dignity and respect.

## In Remembrance

This project was created following the tragic death of Mika Nevio Teichmann in a motorcycle accident in Uelzen on 2 August 2026.

Further information is available from the following public sources:

- [Memorial page for Mika Nevio Teichmann](https://trauer.az-online.de/traueranzeige/mika-nevio-teichmann)
- [AZ Online report about the accident](https://www.az-online.de/uelzen/stadt-uelzen/toedlicher-unfall-in-uelzen-motorradfahrer-19-prallt-gegen-geparkten-lkw-und-stirbt-94425249.html)
- [Mika Memorial Rideout](https://www.mikamemorial.dedyn.io)

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
C:.
│   .gitattributes
│   .gitignore
│   LICENSE.txt
│   MikaMemorialRideout.slnLaunch.user
│   MikaMemorialRideout.slnx
│   README.md
│
├───.github
│   └───workflows
│           azure-static-web-apps-orange-field-0302d2c10.yml
│
├───MarcusRunge.MikaMemorialRideout
│   │   App.razor
│   │   MarcusRunge.MikaMemorialRideout.csproj
│   │   MarcusRunge.MikaMemorialRideout.csproj.user
│   │   Program.cs
│   │   _Imports.razor
│   │   
│   ├───Components
│   │   │   PublicFooter.razor
│   │   │   PublicFooter.razor.css
│   │   │   PublicNavigation.razor
│   │   │   PublicNavigation.razor.css
│   │   │ 
│   │   └───Registration
│   │       GroupRegistrationForm.razor
│   │       IndividualRegistrationForm.razor
│   ├───Contracts
│   │       AdminRegistrationEditorItem.cs
│   │       AdminRegistrationMutationResponse.cs
│   │       AdminRegistrationResponse.cs
│   │       AdminRegistrationsResponse.cs
│   │       AdminRegistrationUpdateRequest.cs
│   │       AdminVerificationResponse.cs
│   │       CreateRegistrationRequest.cs
│   │       CreateRegistrationResponse.cs
│   │       GroupRegistrationInput.cs
│   │       IndividualRegistrationInput.cs
│   │       PlanningStatusEditorItem.cs
│   │       PlanningStatusItemResponse.cs
│   │       PlanningStatusLevel.cs
│   │       PlanningStatusResponse.cs
│   │       PublicSummaryResponse.cs
│   │       UpdatePlanningStatusRequest.cs
│   │       UpdatePlanningStatusResponse.cs
│   │
│   ├───Layout
│   │       AdminLayout.razor
│   │       MainLayout.razor
│   │   
│   ├───Pages
│   │       Admin.razor
│   │       CurrentInformation.razor
│   │       Home.razor
│   │       Imprint.razor
│   │       Mika.razor
│   │       Mika.razor.css
│   │       NotFound.razor
│   │       Organisation.razor
│   │       Organisation.razor.css
│   │       Privacy.razor
│   │       Route.razor
│   │       Route.razor.css
│   │       Structure.razor
│   │
│   ├───Properties
│   │       launchSettings.json
│   │
│   ├───Scripts
│   │       Generate-RideoutAdminCredentials.ps1
│   │       Generate-RideoutRoute.ps1
│   │
│   ├───Services
│   │       IRideoutApiClient.cs
│   │       RideoutApiClient.cs
│   │
│   └───wwwroot
│       │   appsettings.Development.json
│       │   appsettings.json
│       │   icon-192.png
│       │   index.html
│       │   staticwebapp.config.json
│       │
│       ├───assets
│       │       mika-memorial.jpg
│       │       mika-rideout-route.geojson
│       │       mika-rideout-waypoints.geojson
│       │       rideout-structure.svg
│       │
│       ├───css
│       │       app.css
│       │
│       ├───js
│       │       rideout-map.js
│       │
│       └───lib
│           └───leaflet
│               │   leaflet.css
│               │   leaflet.js
│               │
│               └───images
│                       layers-2x.png
│                       layers.png
│                       marker-icon-2x.png
│                       marker-icon.png
│                       marker-shadow.png
│
└───MarcusRunge.MikaMemorialRideout.Api
    │   .gitignore
    │   host.json
    │   local.settings.example.json
    │   local.settings.json
    │   MarcusRunge.MikaMemorialRideout.Api.csproj
    │   Program.cs
    │    
    ├───Contracts
    │       AdminRegistrationMutationResponse.cs
    │       AdminRegistrationResponse.cs
    │       AdminRegistrationsResponse.cs
    │       AdminRegistrationUpdateRequest.cs
    │       AdminVerificationResponse.cs
    │       CreateRegistrationRequest.cs
    │       CreateRegistrationResponse.cs
    │       PlanningStatusItemResponse.cs
    │       PlanningStatusLevel.cs
    │       PlanningStatusResponse.cs
    │       PlanningStatusValidation.cs
    │       PublicSummaryResponse.cs
    │       RegistrationValidation.cs
    │       UpdatePlanningStatusRequest.cs
    │       UpdatePlanningStatusResponse.cs
    │
    ├───Functions
    │       AdminRegistrationFunctions.cs
    │       PlanningStatusFunctions.cs
    │       PublicSummaryFunctions.cs
    │       RegistrationFunctions.cs
    |
    ├───Properties
    │       launchSettings.json
    │       serviceDependencies.json
    │       serviceDependencies.local.json
    │       serviceDependencies.local.json.user
    │
    ├───Security
    │       AdminCodeVerifier.cs
    │       IAdminCodeVerifier.cs
    │
    └───Storage
            AdminRegistrationDeleteResult.cs
            AdminRegistrationUpdateResult.cs
            IPlanningStatusRepository.cs
            IRegistrationRepository.cs
            PlanningStatusCatalog.cs
            PlanningStatusDefinition.cs
            PlanningStatusEntity.cs
            RegistrationCreateResult.cs
            RegistrationEntity.cs
            TablePlanningStatusRepository.cs
            TableRegistrationRepository.cs
