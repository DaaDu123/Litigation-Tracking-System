# LTS Update Package — "Create Firm from an existing account" + Auth page pager

This zip contains ONLY the files that are new or changed. Drop each file into
your project at the same relative path (overwriting the existing one), then
follow the "Apply" steps below.

## What changed, and why

### 1. Backend — registered users can now request Firm Admin access
without registering again with the same email.

- `Models/Security/FirmAdminRequest.cs` — added a nullable `UserID`/`User`
  link (mirrors `UserJoinRequest.UserID`), made `AdminPasswordHash` nullable.
- `Data/AppDbContext.cs` — matching EF entity configuration for the new FK/index.
- `Features/FirmAdminRequests/Commands/SubmitFirmAdminRequestFromAccount/*`
  (NEW) — authenticated endpoint, no email/password input. Validates the
  caller isn't already in a firm and has no other pending request, then
  links the request to their existing `UserID`.
- `Features/FirmAdminRequests/Queries/GetMyFirmAdminRequest/*` (NEW) — lets
  the UI show "pending" / "previously rejected" state.
- `Features/FirmAdminRequests/Controllers/FirmAdminRequestsController.cs` —
  added `POST /api/FirmAdminRequests/from-account` and
  `GET /api/FirmAdminRequests/mine` (both `[Authorize]`).
- `Features/FirmAdminRequests/Commands/ApproveFirmAdminRequest/ApproveFirmAdminRequestCommandHandler.cs` —
  now branches on `firmAdminRequest.UserID`: if set, it **promotes the
  requester's existing account in place** (RoleID → FirmAdmin, sets FirmID,
  flips `IsProfileCompleted` back to `false` just so they fill in the firm's
  real name/address) instead of creating a second account. The legacy
  anonymous flow (email+password, no account yet) is untouched.
- `Migrations/20260926080000_AddFirmAdminRequestFromAccount.cs` +
  `.Designer.cs`, and `Migrations/AppDbContextModelSnapshot.cs` — schema
  migration for the above (adds `FirmAdminRequests.UserID`, makes
  `AdminPasswordHash` nullable).

  **These migration files were hand-written** (this sandbox has no `dotnet`
  CLI / NuGet access to run `dotnet ef migrations add`). Before deploying:
  1. Drop all 3 files into place.
  2. Run `dotnet build` to confirm everything compiles.
  3. Run `dotnet ef migrations list` — it should show
     `AddFirmAdminRequestFromAccount` as the latest, with no "pending model
     changes" warning. If EF complains about a mismatch, safest fix is to
     delete the 2 new migration files and run
     `dotnet ef migrations add AddFirmAdminRequestFromAccount` yourself so
     it re-generates them from your actual current model — the model files
     (`FirmAdminRequest.cs`, `AppDbContext.cs`) are the source of truth and
     don't need regenerating.
  4. `dotnet ef database update`.

### 2. Frontend — "Create Firm" in the sidebar

- `Core/Http/ApiEndpoints.cs` — added `FromAccount` / `Mine` routes.
- `Features/FirmAdminRequests/Services/IFirmAdminRequestService.cs` +
  `FirmAdminRequestService.cs` — added `SubmitFromAccountAsync()` /
  `GetMineAsync()`.
- `Features/FirmAdminRequests/Components/CreateFirmModal.razor` (NEW) —
  self-contained modal (styled like `RejectFirmAdminRequestModal.razor`).
  Checks `GetMineAsync()` on open to show a pending/rejected state;
  otherwise a one-click "Submit Request" with no email/password fields.
- `Layout/NavMenu.razor` — added a "Create Firm" item next to "Join a Firm"
  (shown only when `Session.Role` is empty, i.e. a plain registered user
  with no firm yet), which opens `CreateFirmModal`.

### 3. Frontend — animated bottom pager on the 3 auth pages

- `Layout/LoginLayout.razor` — renders a "‹ Sign In · 1 of 3 ›" pill plus
  small jump-dots under the auth card, driven by `NavigationManager` (Prev/
  Next/dot clicks all just call `NavigateTo`). Hidden on the other pages
  that share this layout (Forgot/Reset Password, Verify OTP) since those
  aren't part of the 3-way choice.
- `Layout/LoginLayout.razor.css` (NEW, CSS-isolated — only affects
  `LoginLayout.razor`) — pill/dots styling using your existing `--lts-*`
  theme tokens, so it matches both light and dark mode automatically. Includes
  a one-time pop-in and a delayed "Explore more options" fade-in.

## Apply steps

1. Copy every file in this zip into your project at the matching path.
2. Backend: `dotnet build`, then the migration steps above, then
   `dotnet ef database update`.
3. Frontend: `dotnet build` (Blazor Server) — no migration needed here.
4. Smoke test:
   - Register a new Firm User account, log in, open the sidebar → "Create
     Firm" → submit. Confirm no email/password was asked and you were not
     sent back to registration.
   - As Super Admin, approve that request from "Firm Admin Requests" —
     confirm the SAME user account (same email) becomes the Firm Admin
     rather than a new one being created, and they're routed to complete
     their firm's profile on next login.
   - Visit `/login`, `/register`, `/request-firm-admin` and confirm the
     pill nav shows the right page/count and Prev/Next/dots all navigate
     correctly, and does NOT show on `/forgot-password`, `/verify-otp`, or
     `/reset-password`.
