# LTS styles

All application CSS lives here, one concern per file. There is no global
`app.css`. Files are linked, in cascade order, from `Layout/StyleSheets.razor`.

| Folder        | Purpose                                                          |
|---------------|------------------------------------------------------------------|
| `base/`       | design tokens (`tokens.css`), element defaults, helper utilities |
| `layout/`     | app shell, sidebar, topbar, public navbar/footer, auth carousel  |
| `components/` | reusable pieces: buttons, cards, tables, forms, modals, ...      |
| `features/`   | page/module specific: auth, dashboard, master data, marketing, firm-admin-requests |

## Adding styles
1. Put them in the file that matches the component/feature (or create one).
2. Register a new file in `Layout/StyleSheets.razor`.
3. Keep that file's responsive rules at the bottom, in its own `@media` blocks.

## Breakpoints (Bootstrap 5 aligned)
XS < 576 · SM >= 576 · MD >= 768 · LG >= 992 · XL >= 1200 · XXL >= 1400.
Navigation becomes an off-canvas drawer below 992px; tables become stacked
cards below 768px; modals become bottom sheets below 576px.
