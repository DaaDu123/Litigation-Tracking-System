










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



    scheduleLabelAll();

    window.ltsResponsiveTable = { labelAll: labelAll };
})();
