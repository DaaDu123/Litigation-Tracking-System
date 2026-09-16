// Auto-labels every ".lts-table" on the page so the CSS responsive-table
// rule (see app.css, @media max-width:640px) can turn each row into a
// stacked "label: value" card on small screens instead of needing a
// horizontal scroll. This runs for every list in the app (Users, Cases,
// Firms, Hearings, Documents, Master Data, etc.) with zero changes needed
// per-page: it just reads each table's own <thead> and mirrors the text
// onto the matching <td data-label="..."> in every row.
//
// A MutationObserver re-applies labels whenever Blazor re-renders a table
// (new data loaded, page changed, filters applied, etc.) since those
// re-renders happen without a full page reload.
(function () {
    function labelTable(table) {
        var headerCells = table.querySelectorAll('thead th');
        if (!headerCells.length) return;

        var labels = [];
        for (var i = 0; i < headerCells.length; i++) {
            labels.push(headerCells[i].textContent.trim());
        }

        var rows = table.querySelectorAll('tbody tr');
        for (var r = 0; r < rows.length; r++) {
            var cells = rows[r].children;
            for (var c = 0; c < cells.length; c++) {
                var label = labels[c] || '';
                if (label && cells[c].getAttribute('data-label') !== label) {
                    cells[c].setAttribute('data-label', label);
                }
            }
        }
    }

    function labelAll() {
        // Skip the Permission Matrix grid - it intentionally keeps its
        // normal table layout (sticky column + horizontal scroll) rather
        // than stacking into label/value rows on mobile.
        var tables = document.querySelectorAll('table.lts-table:not(.lts-matrix-table)');
        for (var i = 0; i < tables.length; i++) {
            labelTable(tables[i]);
        }
    }

    var scheduled = false;
    function scheduleLabelAll() {
        if (scheduled) return;
        scheduled = true;
        requestAnimationFrame(function () {
            scheduled = false;
            labelAll();
        });
    }

    document.addEventListener('DOMContentLoaded', labelAll);

    var observer = new MutationObserver(scheduleLabelAll);
    observer.observe(document.documentElement, { childList: true, subtree: true, characterData: true });

    // In case this script runs after DOMContentLoaded already fired
    // (e.g. Blazor's own render finished first).
    scheduleLabelAll();

    window.ltsResponsiveTable = { labelAll: labelAll };
})();
