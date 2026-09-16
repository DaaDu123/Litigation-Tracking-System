# Dashboard Module — Integration Guide

This package contains **only the files that were added or changed** on top of
your existing `LTSFrontend` project. Drop them into the matching paths in
your real project (overwrite).

⚠️ **Not compiled.** This sandbox has no .NET SDK / nuget access, so it
hasn't been through `dotnet build`. Please build locally / in Claude Code
and fix any compiler errors that come up.

## What's new

| File | Purpose |
|---|---|
| `Features/Dashboard/Models/DashboardDTO.cs` | Now has real fields (`TotalUsers`, `ActiveUsers`, `TotalRoles`, `TotalPermissions`, `TotalAuditLogs`, `TotalRefreshTokens`, `RecentActivities`) + `RecentActivityDTO`, mirroring the backend's `GET /api/dashboard` response exactly. |
| `Features/Dashboard/Services/IDashboardService.cs` / `DashboardService.cs` | Calls `GET /api/dashboard` via your `ApiClient`. |
| `Features/Dashboard/Components/KpiCard.razor` | Reusable stat card (icon, value, label, optional subtext, 5 accent colors). |
| `Features/Dashboard/Components/ChartWidget.razor` | Dependency-free horizontal bar breakdown (pure CSS, no chart library) — used for an Active vs Inactive users split. |
| `Features/Dashboard/Components/RecentActivityPanel.razor` | Renders the 10 most recent audit log entries with a relative timestamp ("5m ago") and an icon guessed from the action text. |
| `Features/Dashboard/Pages/Home.razor` | The actual dashboard at `/` — see "Design decisions" below, this is a full rewrite. |

## Small edits to existing files
- **`Core/Http/ApiEndpoints.cs`** — added a `Dashboard.Stats` route (`api/dashboard`).
- **`Core/Extensions/ServiceCollectionExtensions.cs`** — registered `IDashboardService`/`DashboardService` in DI.
- **`wwwroot/app.css`** — appended (nothing removed) styling for KPI cards, the chart widget, the activity list, and a quick-links grid. Reuses your existing `--lts-*` theme variables so it respects the dark/light toggle.

## Deleted (do this manually in your project)
Your original zip had six per-role dashboard stub pages:
`AdminDashboard.razor`, `LawyerDashboard.razor`, `LegalOfficerDashboard.razor`,
`ManagementDashboard.razor`, `SupervisorDashboard.razor`,
`ExternalCounselDashboard.razor` (all under `Features/Dashboard/Pages/`).

**Please delete these six files.** None of them had a `@page` directive, so
they were never actually routed/reachable — and their names (Admin/Lawyer/
Supervisor/Management/LegalOfficer/ExternalCounsel) don't match your real
6-role model (`SuperAdmin`, `FirmAdmin`, `Partner`, `AssociateLawyer`,
`Moharrir`, `InternParalegal` — confirmed from `Comman/Enum/UserRole.cs` and
the seed data in `AppDbContext.cs` on the backend). Keeping them would just
be confusing dead code.

## ⚠️ Important backend finding — please double check

`DashboardController.GetStats()` is gated by `[HasPermission("ViewDashboard")]`.
I checked your backend's `SeedRolePermissions()` in `AppDbContext.cs` and
**`ViewDashboard` is never granted to any role** — it isn't even in
`PermissionEnum.cs`. `PermissionService.HasPermissionAsync` only auto-passes
for `SuperAdmin` (hard-coded bypass); every other role (`FirmAdmin`,
`Partner`, `AssociateLawyer`, `Moharrir`, `InternParalegal`) will get a
**403** from this endpoint as things stand today.

I designed the frontend to handle this gracefully rather than pretend it
doesn't exist:
- A 403 is treated as "no access to system stats" (not an error) — those
  roles see a lighter welcome panel + Quick Links instead of a broken page
  or a scary error banner.
- Only `SuperAdmin` will currently see the KPI cards / chart / activity feed.

**If you want other roles (e.g. `FirmAdmin`) to see dashboard stats too**,
that's a one-line backend fix: add `PermissionEnum.ViewDashboard = ...` and
grant it to the relevant role(s) in `SeedRolePermissions()`. Say the word and
I can do that fix + a new EF Core migration next.

## Design decisions worth knowing about

- **No chart library added.** The backend endpoint returns aggregate counts
  only (no time series), so a real chart library would be overkill. The
  "chart" is a labelled CSS bar breakdown — honest about what data actually
  exists.
- **Recent Activity shows `User #{id}`, not a name.** The backend's
  `RecentActivityDTO` only returns `UserID` (int), not a joined name. Doing
  it properly means an N+1 lookup per row against `UserService` or a backend
  change to include the name in the DTO — flagging this instead of silently
  hiding it or making extra round-trips per row.
- **Quick Links only points at pages that actually work today** (`/users` for
  Partner/FirmAdmin/SuperAdmin, `/master/courts` for everyone) rather than
  linking into the other still-stub pages (Cases, Documents, Roles, the rest
  of Master Data) — better than sending someone into a blank page.
- **Role gating for the Users link** mirrors what's already in `NavMenu.razor`
  (`Partner,FirmAdmin,SuperAdmin`).

## To do next
- Fix the `ViewDashboard` permission gap on the backend if you want
  non-SuperAdmin roles to see real stats (see above).
- Once Case Management is built, a natural next step is a
  `MyAssignedCasesWidget` on this dashboard using
  `GET /api/case-assignments/my-cases` — that endpoint already exists on the
  backend and isn't used by any frontend page yet.
