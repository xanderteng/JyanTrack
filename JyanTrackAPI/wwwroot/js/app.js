$(document).ready(function () {
    // 1. Initial Page Load: Support URL query parameters (?id=... or ?accountId=...)
    const urlParams = new URLSearchParams(window.location.search);
    const initialAccountId = urlParams.get('id') || urlParams.get('accountId');
    if (initialAccountId && !isNaN(initialAccountId)) {
        $('#accountIdInput').val(initialAccountId);
        loadDashboard(parseInt(initialAccountId, 10));
    }

    // 2. Event Listeners
    $('#syncBtn').on('click', function () {
        const accountId = getTargetAccountId();
        if (!accountId) return;

        triggerSync(accountId);
    });

    $('#loadBtn').on('click', function () {
        const accountId = getTargetAccountId();
        if (!accountId) return;

        loadDashboard(accountId);
    });

    // Allow pressing "Enter" inside the account ID input
    $('#accountIdInput').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            $('#loadBtn').click();
        }
    });

    // Helper: Read and sanitize account ID input
    function getTargetAccountId() {
        const val = $('#accountIdInput').val().trim();
        if (!val || isNaN(val)) {
            showAlert('Please enter a valid numeric Account ID.', 'warning');
            return null;
        }
        return parseInt(val, 10);
    }

    // 3. API Call: Fetch Dashboard Metrics
    function loadDashboard(accountId) {
        $.ajax({
            url: `/api/MahjongStats/profile/${accountId}`,
            type: 'GET',
            dataType: 'json',
            beforeSend: function () {
                $('#alertBox').addClass('d-none');
                $('#playerName').text('Loading profile...');
                $('#matchTableBody').html('<tr><td colspan="5" class="text-center text-muted py-3"><i class="fas fa-spinner fa-spin mr-1"></i>Loading matches...</td></tr>');
            },
            success: function (data) {
                renderProfileHeader(data);
                renderKpiCards(data);
                renderPlacementDistribution(data);
                renderRecentMatchesTable(data.recentMatches);
            },
            error: function (xhr) {
                if (xhr.status === 404) {
                    $('#playerName').text(`Account ${accountId}`);
                    showAlert(`No local data found for Account ID: ${accountId}. Click "Sync Live" to ingest matches.`, 'warning');
                    $('#matchTableBody').html('<tr><td colspan="5" class="text-center text-muted py-3">No local matches found. Click "Sync Live" to fetch.</td></tr>');
                } else {
                    $('#playerName').text('Profile Error');
                    showAlert('Failed to retrieve player metrics from API.', 'danger');
                    $('#matchTableBody').html('<tr><td colspan="5" class="text-center text-muted py-3 text-danger">Failed to retrieve match history.</td></tr>');
                }
            }
        });
    }

    // 4. API Call: Sync Matches Without Page Reload
    function triggerSync(accountId) {
        const $syncBtn =$('#syncBtn');
        const originalHtml = $syncBtn.html();

        $.ajax({
            url: `/api/MahjongStats/sync/${accountId}`,
            type: 'POST',
            dataType: 'json',
            beforeSend: function () {
                $syncBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin mr-1"></i>Syncing...');
                showAlert('Syncing recent matches from upstream API...', 'info');
            },
            success: function (response) {
                showAlert(`Sync complete: ${response.importedCount} new matches processed.`, 'success');
                if (response.stats) {
                    renderProfileHeader(response.stats);
                    renderKpiCards(response.stats);
                    renderPlacementDistribution(response.stats);
                    renderRecentMatchesTable(response.stats.recentMatches);
                }
            },
            error: function (xhr) {
                if (xhr.status === 429) {
                    showAlert('Upstream rate limit reached (HTTP 429). Loaded fallback cache instead.', 'warning');
                } else {
                    showAlert('Sync operation failed. Please check backend logs.', 'danger');
                }
            },
            complete: function () {
                $syncBtn.prop('disabled', false).html(originalHtml);
            }
        });
    }

    // 5. DOM Renderers
    function renderProfileHeader(data) {
        $('#playerName').text(data.nickname || 'Unknown');
        $('#playerDan').text(`Dan Level: ${data.danLevel}`);
        $('#playerAccount').text(`ID: ${data.accountId}`);
        $('#totalMatches').text(data.totalMatches);
    }

    function renderKpiCards(data) {
        $('#statAvgRank').text(data.averageRank.toFixed(2));
        $('#statLasAvoid').text(`${data.lasAvoidanceRate.toFixed(1)}%`);
        
        const umaPrefix = data.totalUmaDelta > 0 ? '+' : '';
        $('#statUmaDelta').text(`${umaPrefix}${data.totalUmaDelta}`);
        
        $('#statRating').text(data.performanceRating.toFixed(1));
    }

    function renderPlacementDistribution(data) {
        $('#barRank1').css('width', `${data.firstPlaceRate}%`).text(`1st: ${data.firstPlaceRate}%`);
        $('#barRank2').css('width', `${data.secondPlaceRate}%`).text(`2nd: ${data.secondPlaceRate}%`);
        $('#barRank3').css('width', `${data.thirdPlaceRate}%`).text(`3rd: ${data.thirdPlaceRate}%`);
        $('#barRank4').css('width', `${data.fourthPlaceRate}%`).text(`4th: ${data.fourthPlaceRate}%`);

        $('#countRank1').text(`1st Place: ${data.firstPlaceCount}`);
        $('#countRank2').text(`2nd Place: ${data.secondPlaceCount}`);
        $('#countRank3').text(`3rd Place: ${data.thirdPlaceCount}`);
        $('#countRank4').text(`4th Place: ${data.fourthPlaceCount}`);
    }

    function renderRecentMatchesTable(matches) {
        const $tbody =$('#matchTableBody');
        $tbody.empty();

        if (!matches || matches.length === 0) {
            $tbody.html('<tr><td colspan="5" class="text-center text-muted py-3">No match history available.</td></tr>');
            return;
        }

        matches.forEach(function (m) {
            const dateStr = new Date(m.playedAt).toLocaleString([], { 
                month: 'short', 
                day: 'numeric', 
                hour: '2-digit', 
                minute: '2-digit' 
            });

            const rankBadge = `<span class="badge rank-badge-${m.rank} px-2 py-1">${m.rank}</span>`;
            const scoreFormatted = m.finalScore.toLocaleString();
            const umaColorClass = m.umaDelta >= 0 ? 'text-success' : 'text-danger';
            const umaPrefix = m.umaDelta > 0 ? '+' : '';

            const row = `
                <tr>
                    <td>${rankBadge}</td>
                    <td><small class="text-muted">${dateStr}</small></td>
                    <td><code>${m.externalId.substring(0, 16)}...</code></td>
                    <td class="text-right font-weight-bold">${scoreFormatted}</td>
                    <td class="text-right font-weight-bold ${umaColorClass}">${umaPrefix}${m.umaDelta}</td>
                </tr>
            `;

            $tbody.append(row);
        });
    }

    function showAlert(message, type) {
        const $box =$('#alertBox');
        $box.removeClass('d-none alert-info alert-success alert-warning alert-danger')
            .addClass(`alert-${type}`)
            .text(message);

        setTimeout(function () {
            $box.addClass('d-none');
        }, 5000);
    }
});