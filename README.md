# LTS inline master-data entry improvements

## Changes in this source package

### New Case page
- Case Category/Case Type uses one searchable text field with browser suggestions and a `Use / Add` action.
- Court uses one searchable text field with suggestions and `Use / Add`.
- Responsible Department uses one searchable text field with suggestions and `Use / Add`; `None` clears the assignment.
- Exact existing names are reused case-insensitively. When the typed value does not match an existing item, the selected ID is cleared to avoid silently keeping an older selection.
- Successful additions refresh the options and select the new item. If a concurrent request created the same name, the refreshed existing record is selected instead.

### Workflow Template editor
- Default Department, Initial Status, Workflow Stage, and Document Type now have searchable/typeable fields with `Use / Add` actions.
- Existing names are reused; new values are created through the existing master-data API services.
- A new document type appears in the list; choose Required or Optional in its row after adding it.
- A department is optional. `None` means the case creator chooses the department. It is not an error when no active departments exist.
- Workflow template save still requires an Initial Status and at least one stage. A stage must be added to the ordered stage list before saving.

## Validation notes
- Backend source is included unchanged from the uploaded backend ZIP.
- Frontend source was edited in `CreateCase.razor` and `CaseWorkflowTemplateFormModal.razor`.
- The current environment does not have the .NET SDK, so `dotnet build` and live SQL/API tests could not be run here. Build and test in Visual Studio before replacing production files.
- Create a backup before merging these source files into the active solution. Existing database migrations are not modified by these UI changes.
