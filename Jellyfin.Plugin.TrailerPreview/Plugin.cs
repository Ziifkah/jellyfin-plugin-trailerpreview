using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.TrailerPreview.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TrailerPreview;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    private readonly ILogger<Plugin> _logger;
    private readonly IApplicationPaths _applicationPaths;

    public override string Name => "Trailer Preview";

    public override Guid Id => Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    public static Plugin? Instance { get; private set; }

    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer,
        ILogger<Plugin> logger)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        _logger = logger;
        _applicationPaths = applicationPaths;

        _logger.LogInformation("Trailer Preview Plugin initializing...");

        try
        {
            InitializeScriptInjection();
            _logger.LogInformation("Trailer Preview Plugin initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Trailer Preview Plugin");
        }
    }

    private void InitializeScriptInjection()
    {
        try
        {
            // Try to use File Transformation plugin first (for Docker compatibility)
            if (TryRegisterFileTransformation())
            {
                _logger.LogInformation("Registered with File Transformation plugin");
                return;
            }

            // Fallback to direct injection
            _logger.LogInformation("File Transformation plugin not found, using direct injection");
            InjectScriptDirectly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to inject client script");
        }
    }

    private bool TryRegisterFileTransformation()
    {
        try
        {
            // Try to find the File Transformation plugin via reflection
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name?.Contains("FileTransformation") == true);

            if (assembly == null)
            {
                return false;
            }

            // Find the registration method
            var pluginType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name.Contains("Plugin"));

            if (pluginType == null)
            {
                return false;
            }

            var registerMethod = pluginType.GetMethod("RegisterTransformation");
            if (registerMethod == null)
            {
                return false;
            }

            // Register our transformation callback
            registerMethod.Invoke(null, new object[]
            {
                "index.html",
                new Func<string, string>(TransformIndexHtml)
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to register with File Transformation plugin");
            return false;
        }
    }

    private void InjectScriptDirectly()
    {
        var webPath = Path.Combine(_applicationPaths.WebPath, "index.html");

        if (!File.Exists(webPath))
        {
            _logger.LogWarning("Could not find index.html at {Path}", webPath);
            return;
        }

        try
        {
            var content = File.ReadAllText(webPath);
            var transformedContent = TransformIndexHtml(content);

            if (content != transformedContent)
            {
                File.WriteAllText(webPath, transformedContent);
                _logger.LogInformation("Successfully injected client script into index.html");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to modify index.html");
        }
    }

    private string TransformIndexHtml(string content)
    {
        const string scriptTag = @"<script src=""/TrailerPreview/ClientScript""></script>";
        const string marker = "<!-- Trailer Preview Plugin -->";
        const string fullInjection = marker + "\n    " + scriptTag;

        // Remove existing injection if present
        content = Regex.Replace(
            content,
            @"<!-- Trailer Preview Plugin -->[\s\S]*?<script src=""/TrailerPreview/ClientScript""></script>",
            string.Empty,
            RegexOptions.Multiline
        );

        // Inject before closing body tag
        if (!content.Contains(scriptTag))
        {
            content = content.Replace("</body>", $"    {fullInjection}\n</body>");
        }

        return content;
    }

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
            }
        };
    }
}
