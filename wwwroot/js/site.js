/**
 * site.js — WorkForce EMS
 * Sidebar, nav, dropdowns, table filters, pagination
 */
(function () {
    'use strict';

    /* ============================================================
       SIDEBAR — collapse / expand
       Bug fixes:
         1. Read current state fresh from classList on every click
         2. Apply saved state via classList directly (not via setSidebar
            which was racing with DOMContentLoaded)
         3. The toggle SVG arrow is CSS-rotated; no JS rotation needed
    ============================================================ */
    const sidebar   = document.getElementById('sidebar');
    const toggleBtn = document.getElementById('sidebarToggle');

    function collapseSidebar()  {
        if (!sidebar) return;
        sidebar.classList.add('collapsed');
        try { localStorage.setItem('ems_sidebar', '1'); } catch (_) {}
    }

    function expandSidebar() {
        if (!sidebar) return;
        sidebar.classList.remove('collapsed');
        try { localStorage.setItem('ems_sidebar', '0'); } catch (_) {}
    }

    // Restore saved state immediately (before paint)
    try {
        if (localStorage.getItem('ems_sidebar') === '1') {
            sidebar && sidebar.classList.add('collapsed');
        }
    } catch (_) {}

    // Toggle on button click — read current state each time
    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            if (sidebar.classList.contains('collapsed')) {
                expandSidebar();
            } else {
                collapseSidebar();
            }
        });
    }

    /* ============================================================
       BOOTSTRAP DROPDOWNS — ensure they work
       Bootstrap 5 auto-initialises dropdowns on DOMContentLoaded,
       but only if bootstrap.bundle.min.js is already loaded.
       We re-initialise here to be safe, after all scripts are done.
    ============================================================ */
    document.addEventListener('DOMContentLoaded', function () {
        // Re-init any dropdown that Bootstrap may have missed
        if (typeof bootstrap !== 'undefined' && bootstrap.Dropdown) {
            document.querySelectorAll('[data-bs-toggle="dropdown"]').forEach(function (el) {
                // Bootstrap auto-inits, but calling getInstance first avoids duplicates
                if (!bootstrap.Dropdown.getInstance(el)) {
                    new bootstrap.Dropdown(el);
                }
            });
        }
    });

    /* ============================================================
       LEAVE SUB-NAV DROPDOWN (sidebar)
    ============================================================ */
    /* ============================================================
       ACTIVE NAV HIGHLIGHT
    ============================================================ */
    const path = window.location.pathname.toLowerCase();

    document.querySelectorAll('.nav-item[href], .nav-sub-item[href]').forEach(function (link) {
        const href = link.getAttribute('href');
        if (href && href !== '#' && path.startsWith(href.toLowerCase())) {
            link.classList.add('active');
            const sub = link.closest('.nav-sub');
            if (sub) {
                sub.classList.add('open');
                const toggle = document.getElementById('leaveToggle');
                const SystemToggle = document.getElementById('SystemToggle');
                if (toggle) toggle.classList.add('open');
                if (SystemToggle) SystemToggle.classList.add('open');
            }
        }
    });

    /* ============================================================
       EMPLOYEE TABLE — filter + search + sort + pagination
       Guard: only runs on pages that have #employeeTable
    ============================================================ */
    const tableEl = document.getElementById('employeeTable');
    if (!tableEl) return;

    const tbody      = tableEl.querySelector('tbody');
    const allRows    = Array.from(tbody ? tbody.querySelectorAll('tr') : []);
    const chips      = document.querySelectorAll('.filter-chip');
    const tableSearch = document.getElementById('tableSearch');
    const paginationEl = document.getElementById('tablePagination');
    const paginationInfo = document.getElementById('paginationInfo');

    const PER_PAGE = 8;
    let currentFilter = 'all';
    let currentSearch  = '';
    let currentPage    = 1;
    let sortCol        = -1;
    let sortDir        = 'asc';

    function getRowText(row, colIdx) {
        const cells = row.querySelectorAll('td');
        return cells[colIdx] ? cells[colIdx].textContent.trim().toLowerCase() : '';
    }

    function getRowStatus(row) {
        const badge = row.querySelector('.status-badge');
        if (!badge) return 'active';
        return badge.classList.contains('inactive') ? 'inactive' : 'active';
    }

    function getFilteredRows() {
        return allRows.filter(row => {
            if (currentFilter !== 'all' && getRowStatus(row) !== currentFilter) return false;
            if (currentSearch) {
                const text = row.textContent.toLowerCase();
                if (!text.includes(currentSearch)) return false;
            }
            return true;
        });
    }

    function sortRows(rows) {
        if (sortCol < 0) return rows;
        return [...rows].sort((a, b) => {
            const ta = getRowText(a, sortCol);
            const tb = getRowText(b, sortCol);
            return sortDir === 'asc' ? ta.localeCompare(tb) : tb.localeCompare(ta);
        });
    }

    function render() {
        const filtered = getFilteredRows();
        const sorted   = sortRows(filtered);
        const totalPages = Math.max(1, Math.ceil(sorted.length / PER_PAGE));
        if (currentPage > totalPages) currentPage = 1;

        const start = (currentPage - 1) * PER_PAGE;
        const page  = sorted.slice(start, start + PER_PAGE);

        allRows.forEach(r => r.style.display = 'none');
        page.forEach((r, i) => {
            r.style.display = '';
            r.style.animationDelay = (i * 0.04) + 's';
        });

        // Pagination info
        if (paginationInfo) {
            if (sorted.length === 0) {
                paginationInfo.textContent = '0 records';
            } else {
                paginationInfo.textContent =
                    `Showing ${start + 1}–${Math.min(start + PER_PAGE, sorted.length)} of ${sorted.length} employees`;
            }
        }

        // Pagination buttons
        renderPagination(totalPages);

        // Empty state
        let emptyRow = tbody.querySelector('.empty-row');
        if (sorted.length === 0) {
            if (!emptyRow) {
                emptyRow = document.createElement('tr');
                emptyRow.className = 'empty-row';
                const colCount = tableEl.querySelectorAll('thead th').length;
                emptyRow.innerHTML = `<td colspan="${colCount}">
                    <div class="empty-state">
                        <div class="empty-icon">◎</div>
                        <div class="empty-title">No data found</div>
                        <div class="empty-sub">Try adjusting your search or filter</div>
                    </div>
                </td>`;
                tbody.appendChild(emptyRow);
            }
            emptyRow.style.display = '';
        } else if (emptyRow) {
            emptyRow.style.display = 'none';
        }

        // Mirror to mobile cards
        renderMobileCards(page);
    }

    function renderPagination(totalPages) {
        if (!paginationEl) return;
        paginationEl.innerHTML = '';

        const prev = makePageBtn('‹', currentPage === 1);
        prev.addEventListener('click', () => { if (currentPage > 1) { currentPage--; render(); } });
        paginationEl.appendChild(prev);

        for (let i = 1; i <= totalPages; i++) {
            const btn = makePageBtn(i);
            if (i === currentPage) btn.classList.add('active');
            btn.addEventListener('click', () => { currentPage = i; render(); });
            paginationEl.appendChild(btn);
        }

        const next = makePageBtn('›', currentPage === totalPages);
        next.addEventListener('click', () => { if (currentPage < totalPages) { currentPage++; render(); } });
        paginationEl.appendChild(next);
    }

    function makePageBtn(label, disabled = false) {
        const btn = document.createElement('button');
        btn.className = 'page-btn';
        btn.textContent = label;
        if (disabled) btn.disabled = true;
        return btn;
    }

    function renderMobileCards(rows) {
        const container = document.getElementById('mobileEmpCards');
        if (!container) return;
        container.innerHTML = '';
        rows.forEach(row => {
            const cells = row.querySelectorAll('td');
            if (!cells.length) return;
            const card = document.createElement('div');
            card.className = 'emp-mobile-card';
            const avatarEl  = row.querySelector('.emp-avatar');
            const nameEl    = row.querySelector('.emp-name');
            const idEl      = row.querySelector('.emp-id');
            const statusEl  = row.querySelector('.status-badge');
            const deptEl    = row.querySelector('.dept-badge');
            const posCell   = cells[3] ? cells[3].textContent.trim() : '';
            const actionsEl = row.querySelector('.action-group');

            card.innerHTML = `
                <div class="emp-mobile-card-header">
                    <div class="emp-cell">
                        ${avatarEl ? avatarEl.outerHTML : ''}
                        <div>
                            <div class="emp-name">${nameEl ? nameEl.textContent : ''}</div>
                            <div class="emp-id">${idEl ? idEl.textContent : ''}</div>
                        </div>
                    </div>
                    ${statusEl ? statusEl.outerHTML : ''}
                </div>
                <div class="emp-mobile-card-meta">
                    ${deptEl ? deptEl.outerHTML : ''}
                    <span class="text-muted" style="font-size:12px">${posCell}</span>
                </div>
                <div class="action-group">${actionsEl ? actionsEl.innerHTML : ''}</div>
            `;
            container.appendChild(card);
        });
    }

    /* Sorting */
    tableEl.querySelectorAll('thead th[data-sort]').forEach((th, idx) => {
        th.style.cursor = 'pointer';
        th.addEventListener('click', () => {
            const col = parseInt(th.dataset.sort, 10);
            if (sortCol === col) {
                sortDir = sortDir === 'asc' ? 'desc' : 'asc';
            } else {
                sortCol = col;
                sortDir = 'asc';
            }
            tableEl.querySelectorAll('thead th').forEach(t => t.classList.remove('sort-asc', 'sort-desc'));
            th.classList.add(sortDir === 'asc' ? 'sort-asc' : 'sort-desc');
            render();
        });
    });

    /* Filter chips */
    chips.forEach(chip => {
        chip.addEventListener('click', () => {
            chips.forEach(c => c.classList.remove('active'));
            chip.classList.add('active');
            currentFilter = chip.dataset.filter || 'all';
            currentPage = 1;
            render();
        });
    });

    /* Search */
    if (tableSearch) {
        tableSearch.addEventListener('input', () => {
            currentSearch = tableSearch.value.toLowerCase().trim();
            currentPage = 1;
            render();
        });
    }

    /* Global header search → mirrors to table search */
    const globalSearch = document.getElementById('globalSearch');
    if (globalSearch && tableSearch) {
        globalSearch.addEventListener('input', () => {
            tableSearch.value = globalSearch.value;
            tableSearch.dispatchEvent(new Event('input'));
        });
    }

    // Initial render
    render();

})();