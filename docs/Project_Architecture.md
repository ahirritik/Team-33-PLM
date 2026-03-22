# PLM System Project Documentation

This document explains the architecture, core functionalities, and key files of the Product Lifecycle Management (PLM) system.

## 1. Architecture Overview
The project follows **Clean Architecture** principles, separated into four layers to ensure maintainability and scalability.

*   **PLM.Domain**: The core layer containing business entities, enums, and repository interfaces. No external dependencies.
*   **PLM.Application**: Contains business logic, DTOs (Data Transfer Objects), and service implementations. It coordinates tasks between the UI and Domain.
*   **PLM.Infrastructure**: Handles data persistence (SQL Server via EF Core), database migrations, and external services like auditing.
*   **PLM.Web**: The Blazor Server user interface, including Razor components, layouts, and page routing.

---

## 2. Core Functionalities & Key Files

### A. Authentication & User Management
Handles secure login, role-based access control, and user sessions.
*   **Logic**: Cookie-based authentication using ASP.NET Core Identity.
*   **Key Files**:
    *   `Program.cs`: Configures Identity and Auth endpoints (`/Account/PerformLogin`).
    *   `Login.razor`: The sign-in interface.
    *   `ApplicationUser.cs`: Custom user entity with department/role info.
    *   `DataSeeder.cs`: Seeds default accounts (Admin, Engineer, etc.).

### B. Product Management & Versioning
Manages the lifecycle of products, ensuring every change is version-tracked.
*   **Logic**: Products have a global identity but versioned snapshots for prices and attachments.
*   **Key Files**:
    *   `Product.cs` & `ProductVersion.cs`: The core database models.
    *   `ProductService.cs`: Logic for creating products and fetching history.
    *   `ProductsList.razor`: Searchable list of all products.
    *   `ProductDetails.razor`: View active state vs. version history.
    *   `ProductVersionCompareDialog.razor`: Side-by-side version comparison logic.

### C. Bill of Materials (BoM)
Defines the components and manufacturing operations required to build a product.
*   **Logic**: Linked to a specific product; also version-controlled via ECO workflows.
*   **Key Files**:
    *   `BoM.cs`, `BoMVersion.cs`, `BoMComponent.cs`: The BoM data structure.
    *   `BoMsList.razor` & `BoMDetails.razor`: Management of assembly structures.
    *   `BoMCreate.razor`: Interface for adding components and routing operations.

### D. Engineering Change Order (ECO) Workflow
The "Control Center" for all modifications. No direct edits are allowed to Products/BoMs without an ECO.
*   **Logic**: ECO -> Submission -> Approval -> Automatic Version Upgrade.
*   **Key Files**:
    *   `ECO.cs` & `ECOApproval.cs`: Workflow tracking entities.
    *   `ECOCreate.razor`: Complex form for proposing changes (prices, BoM parts).
    *   `ECOApprovals.razor`: Interface for managers to approve/reject changes.
    *   `ECOService.cs`: Orchestrates the transition from "Proposed" to "Released".

---

## 3. Data Flow Example: Creating a Product
1.  **User UI**: User fills out `ProductCreate.razor`.
2.  **Application Layer**: `ProductService.CreateAsync` maps the input to a new `Product` and initial `ProductVersion`.
3.  **Infrastructure Layer**: EF Core saves these to the database.
4.  **Audit**: `AuditService` logs the activity for traceability.
5.  **UI Feedback**: User is redirected to `ProductDetails.razor` to see the newly created version.

## 4. Global Settings
*   **Currency**: Managed via `en-IN` culture in `Program.cs` and `₹` symbols in UI components.
*   **Layout**: `MainLayout.razor` handles the sidebar visibility and top navigation bar.
