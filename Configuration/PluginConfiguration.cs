// <copyright file="PluginConfiguration.cs" company="Six Degrees">
// Copyright © 2026 - Six Degrees Contributors. All rights reserved.
// </copyright>

namespace SixDegrees.Configuration
{
    using MediaBrowser.Model.Plugins;

    /// <summary>
    /// Plugin configuration class.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Gets or sets the cache refresh interval in minutes.
        /// </summary>
        public int CacheRefreshIntervalMinutes { get; set; } = 60;

        /// <summary>
        /// Gets or sets the maximum number of search results.
        /// </summary>
        public int MaxSearchResults { get; set; } = 50;

        /// <summary>
        /// Gets or sets the maximum number of graph nodes to display.
        /// </summary>
        public int MaxGraphNodes { get; set; } = 500;

        /// <summary>
        /// Gets or sets a value indicating whether to include music albums.
        /// </summary>
        public bool IncludeMusic { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include TV series.
        /// </summary>
        public bool IncludeTV { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include movies.
        /// </summary>
        public bool IncludeMovies { get; set; } = true;
    }
}
