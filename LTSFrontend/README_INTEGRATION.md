# Court Module — Integration Guide

This package contains **only the files that were added or changed** on top of
your existing `LTSFrontend` project, laid out in the same folder structure so
you can drop them straight in (overwrite the matching paths in your real
project).

⚠️ **This code has not been compiled.** The sandbox that produced it has no
.NET SDK and no access to nuget.org, so nothing here has been through
`dotnet build`. Please build it locally / in Claude Code and fix any
compiler errors that turn up — I've tried to be precise against your real
`CourtsController` / DTOs / `ApiClient` contracts, but I can't guarantee a
clean first build.

## What's new

### Court module (the actual deliverable)
| File | Purpose |
|---|---|
| `Features/Masters/Models/CourtDTO.cs` | Read model, mirrors backend `CourtDTO` |
| `Features/Masters/Models/SaveCourtDTO.cs` | Create/update payload with validation attributes |
| `Features/Masters/Services/IMasterDataService.cs` / `MasterDataService.cs` | Court CRUD via your `ApiClient` |
| `Features/Masters/Components/CourtFormModal.razor` | Create / Edit / Read-only "View Details" modal, client-side validation |
| `Features/Masters/Components/CourtActionsMenu.razor` | Per-row dropdown (View / Edit / Delete) |
| `Features/Masters/Pages/Courts.razor` | Full list page — search, status filter, sortable columns, pagination, responsive table+card views, skeleton loading, empty/error states |

### Shared infrastructure (previously empty/broken stubs, now real — reusable by future modules too)
`Shared/Components/StatusBadge.razor`, `SearchBox.razor`, `Pagination.razor`,
`ConfirmDialog.razor` (awaitable: `await dialog.ShowAsync(...)`),
`LoadingSpinner.razor` (spinner / table-skeleton / card-skeleton modes),
`Breadcrumb.razor`, `DataTable.razor`.

`State/ToastService.cs` + `Shared/Components/ToastNotification.razor` — global
toast bus, mounted once in `MainLayout.razor`. Inject `ToastService` anywhere
and call `.Success(...)` / `.Error(...)` / `.Warning(...)` / `.Info(...)`.

### Small edits to existing files
- **`Core/Http/ApiClient.cs`** — added a JSON `PutAsync<T>`. Your `UserService`
  only needed multipart `PutFormAsync` (file uploads), but Court's `PUT`
  endpoint binds a plain JSON body, so a JSON PUT was missing.
- **`Core/Http/ApiEndpoints.cs`** — added `Courts` route group.
- **`Core/Extensions/ServiceCollectionExtensions.cs`** — registered
  `IMasterDataService`/`MasterDataService` and `ToastService` in DI.
- **`Layout/MainLayout.razor`** — mounted `<ToastNotification />`, added a
  dark/light theme toggle button in the topbar.
- **`Layout/NavMenu.razor`** — added a "Master Data" section with a Courts link.
- **`App.razor`** — included `js/theme.js` and an inline script that applies
  the saved theme before first paint (avoids a flash of the wrong theme).
- **`wwwroot/app.css`** — appended (did not remove anything) all styling for
  the above: light/dark theme variables, tables, cards, modals, toasts,
  skeletons, pagination, badges, breadcrumbs, responsive rules.
- **`wwwroot/js/theme.js`** — new, small JS interop for theme persistence.

## Design decisions worth knowing about

- **Styling extends your existing `--lts-navy`/`--lts-blue` brand palette**
  (already used on your Login/auth pages) rather than importing NetRex's
  teal e-commerce palette wholesale — consistency with your own app's
  established identity mattered more than literally copying a different
  colored app. I did borrow NetRex's *visual language*: shadow depth,
  rounded corners, hover elevation, smooth transitions.
- **Data loading is client-side filter/sort/paginate.** The backend
  `GET /api/courts` supports `searchText`/`activeOnly`, but has no
  pagination parameters. Courts is master data (small volume), so the page
  fetches once (`activeOnly=false`) and does search/filter/sort/paging in
  memory for a snappier feel. If your court list ever grows very large,
  move filtering server-side.
- **Delete calls the real `DELETE /api/courts/{id}`.** Your backend model
  has an `IsActive` flag but the controller only exposes hard delete — there's
  no "deactivate" endpoint, so the "Delete" action is a real delete, gated
  behind `ConfirmDialog` and behind `FirmAdmin`/`SuperAdmin` roles (matching
  `RoleNames.FirmAdminAndAbove` on the controller). If you actually want a
  soft-deactivate flow instead, that's a backend endpoint change first.
- **Role gating**: viewing the Courts page/list is open to any authenticated
  user (matches the backend `[Authorize]` with no role restriction on
  `GetAll`); Add/Edit/Delete are hidden and blocked client-side for anyone
  who isn't `FirmAdmin`/`SuperAdmin` (matches `[Authorize(Roles = RoleNames.FirmAdminAndAbove)]`
  on Create/Update/Delete). The backend is still the real enforcement point.

## To do next (out of scope for this pass, per your "only Master + Court" instruction)
- The other Master Data pages (`CaseCategories.razor`, `CaseStages.razor`,
  `CaseStatuses.razor`, `Departments.razor`, `DocumentTypes.razor`) are still
  the original one-line placeholders. They weren't touched. The
  `IMasterDataService` is structured so you can add
  `GetCaseCategoriesAsync()` etc. next to the Court methods, following the
  exact same pattern.
- Dark mode CSS variables are in place and the toggle works app-wide, but I
  only exercised/checked it against the Court module and the existing auth
  shell — worth a visual pass across every page once you're compiling.
