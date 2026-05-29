// wwwroot/js/baocao.js
// Script quản lý form lọc báo cáo: validate, set ngày mặc định, submit (GET) và AJAX partial fetch.
// Progressive enhancement: nếu server trả partial HTML (chỉ table) thì replace trực tiếp,
// nếu server trả full page thì sẽ lấy phần .table-container từ HTML trả về.

(function () {
    'use strict';

    // Cấu hình
    const formSelector = '#filterForm';
    const tableContainerSelector = '.table-container';
    const loadingHtml = '<div class="loading text-center py-4">Đang tải dữ liệu...</div>';
    const debounceMs = 500;

    // Trạng thái cho debounce/abort
    let searchDebounceTimer = null;
    let currentFetchController = null;

    // Helpers
    function qs(selector, root = document) {
        return root.querySelector(selector);
    }
    function qsa(selector, root = document) {
        return Array.from(root.querySelectorAll(selector));
    }
    function formatDateInput(date) {
        return date.toISOString().slice(0, 10);
    }

    // Set ngày mặc định: từ đầu tháng đến hôm nay nếu input rỗng
    function initDefaultDates() {
        const tuInput = qs('input[name="TuNgay"]');
        const denInput = qs('input[name="DenNgay"]');
        const now = new Date();

        if (tuInput && !tuInput.value) {
            const firstOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
            tuInput.value = formatDateInput(firstOfMonth);
        }
        if (denInput && !denInput.value) {
            denInput.value = formatDateInput(now);
        }
    }

    // Validate ngày: TuNgay <= DenNgay
    function validateDates(fromStr, toStr) {
        if (!fromStr || !toStr) return true;
        const from = new Date(fromStr);
        const to = new Date(toStr);
        return from <= to;
    }

    // Build URL with query string from form
    function buildUrlFromForm(form) {
        const params = new URLSearchParams(new FormData(form));
        // Ensure empty values are not included (optional)
        // for (const [k, v] of Array.from(params.entries())) {
        //   if (!v) params.delete(k);
        // }
        const base = form.getAttribute('action') || window.location.pathname;
        return base + (params.toString() ? ('?' + params.toString()) : '');
    }

    // Replace table container content with HTML (partial or extracted)
    function replaceTableContainer(html) {
        const container = qs(tableContainerSelector);
        if (!container) return;

        // Try to detect if html is a partial (contains table) or full page
        // If partial, just set innerHTML; if full page, extract .table-container
        if (/<table[\s>]/i.test(html) && !/<html[\s>]/i.test(html)) {
            container.innerHTML = html;
            return;
        }

        // Parse full HTML and extract .table-container
        try {
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            const newContainer = doc.querySelector(tableContainerSelector);
            if (newContainer) {
                container.innerHTML = newContainer.innerHTML;
            } else {
                // Fallback: replace whole body content if nothing found
                container.innerHTML = html;
            }
        } catch (err) {
            container.innerHTML = '<div class="text-danger">Lỗi khi xử lý dữ liệu trả về.</div>';
            console.error(err);
        }
    }

    // Fetch partial via AJAX (GET). Returns true if success, false if fallback needed.
    async function fetchAndReplace(url) {
        const container = qs(tableContainerSelector);
        if (!container) return false;

        // Abort previous fetch if any
        if (currentFetchController) {
            try { currentFetchController.abort(); } catch (e) { /* ignore */ }
        }
        currentFetchController = new AbortController();
        const signal = currentFetchController.signal;

        container.innerHTML = loadingHtml;

        try {
            const res = await fetch(url, {
                method: 'GET',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                signal
            });

            if (!res.ok) {
                // Non-200: let caller fallback to normal navigation
                console.warn('Fetch failed with status', res.status);
                return false;
            }

            const text = await res.text();
            replaceTableContainer(text);
            return true;
        } catch (err) {
            if (err.name === 'AbortError') {
                // aborted by new request; ignore
                return true;
            }
            console.error('AJAX fetch error', err);
            return false;
        } finally {
            currentFetchController = null;
        }
    }

    // Submit handler: validate then try AJAX, fallback to normal submit if AJAX not supported/failed
    async function handleSubmit(event) {
        event.preventDefault();
        const form = event.target;
        const from = form.querySelector('input[name="TuNgay"]')?.value;
        const to = form.querySelector('input[name="DenNgay"]')?.value;

        if (!validateDates(from, to)) {
            alert('Từ ngày không được lớn hơn Đến ngày.');
            return;
        }

        const url = buildUrlFromForm(form);

        // Try AJAX fetch; if fails, do normal navigation
        const ok = await fetchAndReplace(url);
        if (!ok) {
            // fallback: navigate to URL (full page load)
            window.location.href = url;
        } else {
            // Update browser URL without reloading
            try {
                window.history.replaceState({}, '', url);
            } catch (e) { /* ignore */ }
        }
    }

    // Debounced auto-submit for search input
    function handleSearchInput(e) {
        const form = qs(formSelector);
        if (!form) return;

        if (searchDebounceTimer) clearTimeout(searchDebounceTimer);
        searchDebounceTimer = setTimeout(() => {
            // submit via AJAX
            form.requestSubmit();
        }, debounceMs);
    }

    // Auto-submit when Kho select changes
    function handleKhoChange(e) {
        const form = qs(formSelector);
        if (!form) return;
        // immediate submit
        form.requestSubmit();
    }

    // Attach events
    function attachEvents() {
        const form = qs(formSelector);
        if (!form) return;

        // Ensure form uses GET
        form.method = 'get';

        // Submit event
        form.addEventListener('submit', handleSubmit);

        // Debounced search on TuKhoa
        const searchInput = form.querySelector('input[name="TuKhoa"]');
        if (searchInput) {
            searchInput.addEventListener('input', handleSearchInput);
            // Enter key should submit immediately
            searchInput.addEventListener('keydown', function (ev) {
                if (ev.key === 'Enter') {
                    ev.preventDefault();
                    form.requestSubmit();
                }
            });
        }

        // Auto submit on Kho change
        const khoSelect = form.querySelector('select[name="KhoId"]');
        if (khoSelect) {
            khoSelect.addEventListener('change', handleKhoChange);
        }

        // Optional: attach click handlers for export/print buttons to preserve filters
        qsa('button[asp-controller][asp-action]').forEach(btn => {
            btn.addEventListener('click', function (ev) {
                // If button is not inside form, build URL with current filters and navigate
                const action = btn.getAttribute('asp-action');
                const controller = btn.getAttribute('asp-controller');
                if (!action || !controller) return;
                ev.preventDefault();
                const base = '/' + controller + '/' + action;
                const params = new URLSearchParams(new FormData(form));
                const url = base + (params.toString() ? ('?' + params.toString()) : '');
                window.location.href = url;
            });
        });
    }

    // Initialize script
    function init() {
        initDefaultDates();
        attachEvents();
    }

    // DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();
