# Snipe-IT Complete Migration Plan

This document outlines the systematic plan to complete the migration of the Snipe-IT application from Laravel/PHP to the new decoupled .NET 8 API and Angular 18 frontend architecture. 

The strategy is to build out the backend services and frontend components in phases, starting with foundational reference data and moving towards complex relational features (like checkouts and reporting).

## Open Questions

1. **Testing:** Should we prioritize writing Unit/Integration tests for the backend services alongside each phase, or should we focus purely on scaffolding the functional code first?
2. **Authentication:** The current JWT setup relies on a seeded `testuser`. Should Phase 1 include a robust user registration/password-reset flow, or should we assume users are managed externally (e.g., via LDAP/SAML) for now?
3. **UI/UX:** For the new Angular UI, do you want to strictly match the visual layout of the old AdminLTE interface, or are we free to modernize the UX using Angular Material's native design paradigms?

---

## Proposed Implementation Plan

The backend currently has `Assets` and `Auth` fully established. The database entities (EF Core Models) for all tables already exist. We will follow the established `Controller -> Service -> DTO -> Repository/DbContext` pattern for the backend, and the `Service -> Model -> Component (List/Detail/Form)` pattern for the frontend.

### Phase 1: Foundational Entities (Reference Data)
These are the building blocks that other assets depend on.

#### Backend Changes
* **Models/Manufacturers/Categories/Suppliers:**
  * Create `IReferenceDataService` to handle CRUD for these simpler tables.
  * Create DTOs (`ModelDto`, `CategoryDto`, etc.) and AutoMapper profiles.
  * Create `ReferenceDataController`.
* **Status Labels/Locations/Companies/Departments:**
  * Create `IOrganizationService`.
  * Create corresponding DTOs, Mappings, and `OrganizationController`.

#### Frontend Changes
* Create `reference-data.service.ts` and `organization.service.ts`.
* Scaffold list and form components for Models, Categories, Locations, etc., using Angular Material data tables and reactive forms.

### Phase 2: Core Asset Management (The "Other" Assets)
Bringing the remaining major modules up to par with the existing `Assets` module.

#### Backend Changes
* **Licenses:**
  * Create `ILicenseService`, `LicensesController`.
  * Create DTOs for Licenses and LicenseSeats.
  * Implement Checkout/Checkin logic for License seats.
* **Accessories/Consumables/Components:**
  * Create services (`IAccessoryService`, `IConsumableService`, etc.) and controllers.
  * Implement quantity-based checkout/checkin endpoints.

#### Frontend Changes
* Scaffold `pages/licenses`, `pages/accessories`, `pages/consumables`, `pages/components`.
* Each module will include `-list`, `-detail`, and `-form` components similar to the current `assets` structure.
* Wire up services to the new backend endpoints.

### Phase 3: Identity & User Management
Full control over users, groups, and permissions.

#### Backend Changes
* **Users & Groups:**
  * Create `IUserService` and `UsersController`.
  * Implement CRUD for Users, mapping to the `User` entity.
  * Implement Permissions/Roles logic mapping to `PermissionGroup`.

#### Frontend Changes
* Scaffold `pages/users`.
* Create user list, profile view, and user creation/editing forms.
* Implement role-based UI guards (hiding admin links from standard users).

### Phase 4: Action Logs & History
Tracking who did what, and when.

#### Backend Changes
* **Action Logs:**
  * Create `IActionLogService` to retrieve system logs.
  * Ensure all Checkin/Checkout operations in Phases 1-3 write to the `ActionLog` table.
  * Create `LogsController` for retrieving audit trails.

#### Frontend Changes
* Scaffold `pages/reports/activity`.
* Create a dedicated log viewing component with advanced filtering (by user, by asset, by date).

### Phase 5: Dashboard & Reporting
Bringing the home screen to life.

#### Backend Changes
* **Dashboard Stats:**
  * Create `IDashboardService` and `DashboardController`.
  * Create endpoints to aggregate data: Total Assets, Deployed Assets, Expiring Licenses, Recent Activity.

#### Frontend Changes
* Scaffold `pages/dashboard`.
* Implement chart components (using Chart.js or similar) and stat cards.

---

## Verification Plan

### Automated Tests
- **Backend:** Run `dotnet test` in the `backend` directory to ensure no regressions in existing DI containers or endpoints.
- **Frontend:** Run `ng test` in the `frontend` directory.

### Manual Verification
1. Boot the environment using `docker-compose up`.
2. Open the Angular app at `http://localhost:4200`.
3. Log in with the test user credentials.
4. Verify that the new menu items for Licenses, Users, and Settings appear.
5. Create a new License, check it out to a User, and verify the quantities update correctly in the database and UI.
6. Verify the action is recorded in the Activity Log.
