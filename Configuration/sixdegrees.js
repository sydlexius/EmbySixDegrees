(function () {
    var pluginId = "6D6D6D6D-5D5D-4D4D-3D3D-2D2D2D2D2D2D";

    function loadStats() {
        ApiClient.getJSON(ApiClient.getUrl('/SixDegrees/Statistics')).then(function (stats) {
            var html = '';
            html += '<p><strong>People:</strong> ' + (stats.PeopleCount || 0) + '</p>';
            html += '<p><strong>Media Items:</strong> ' + (stats.MediaCount || 0) + '</p>';
            html += '<p><strong>Connections:</strong> ' + (stats.ConnectionCount || 0) + '</p>';
            html += '<p><strong>Last Build:</strong> ' + (stats.LastBuildTime ? new Date(stats.LastBuildTime).toLocaleString() : 'Never') + '</p>';
            document.getElementById('statsContainer').innerHTML = html;
        }).catch(function (error) {
            console.error('Error loading stats:', error);
            document.getElementById('statsContainer').innerHTML = '<p style="color: #e74c3c;">Error loading statistics. Make sure the plugin is properly configured.</p>';
        });
    }

    function rebuildGraph() {
        var btn = document.getElementById('btnRebuildGraph');
        btn.disabled = true;
        btn.querySelector('span').textContent = 'Rebuilding...';

        ApiClient.ajax({
            type: 'POST',
            url: ApiClient.getUrl('/SixDegrees/Rebuild')
        }).then(function (result) {
            Dashboard.alert('Graph rebuild initiated. This may take several minutes for large libraries.');
            loadStats();
        }).catch(function (error) {
            console.error('Error rebuilding graph:', error);
            Dashboard.alert('Error rebuilding graph. Check server logs for details.');
        }).finally(function () {
            btn.disabled = false;
            btn.querySelector('span').textContent = 'Rebuild Graph';
        });
    }

    // Initialize page
    var page = document.querySelector('.sixDegreesConfigurationPage');

    page.addEventListener('pageshow', function () {
        loadStats();
    });

    document.getElementById('btnRebuildGraph').addEventListener('click', rebuildGraph);

    console.log('Six Degrees plugin page loaded');
})();
