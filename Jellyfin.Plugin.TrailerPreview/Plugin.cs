using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.TrailerPreview.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.TrailerPreview;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    private readonly ILogger<Plugin> _logger;

    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer,
        ILogger<Plugin> logger,
        IServerConfigurationManager configurationManager)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        _logger = logger;

        try
        {
            _logger.LogInformation("Trailer Preview Plugin v2 initializing...");

            // Try File Transformation plugin first, fall back to direct injection
            if (!TryRegisterFileTransformation(configurationManager))
            {
                _logger.LogInformation("File Transformation plugin not available, using direct injection");
                InjectClientScript(applicationPaths, configurationManager);
            }

            _logger.LogInformation("Trailer Preview Plugin initialized successfully");
            _logger.LogInformation("  EnableTrailerPreview: {Value}", Configuration.EnableTrailerPreview);
            _logger.LogInformation("  EnableDebugLogging: {Value}", Configuration.EnableDebugLogging);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during plugin initialization");
            // Don't throw - allow plugin to load so config page is accessible
        }
    }

    private bool TryRegisterFileTransformation(IServerConfigurationManager configurationManager)
    {
        try
        {
            _logger.LogDebug("Attempting to register with File Transformation plugin...");

            var fileTransformationAssembly = AssemblyLoadContext.All
                .SelectMany(x => x.Assemblies)
                .FirstOrDefault(x => x.FullName?.Contains("FileTransformation", StringComparison.OrdinalIgnoreCase) ?? false);

            if (fileTransformationAssembly == null)
            {
                return false;
            }

            _logger.LogDebug("Found File Transformation assembly: {AssemblyName}", fileTransformationAssembly.FullName);

            var pluginInterfaceType = fileTransformationAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");
            if (pluginInterfaceType == null)
            {
                return false;
            }

            var registerMethod = pluginInterfaceType.GetMethod("RegisterTransformation");
            if (registerMethod == null)
            {
                return false;
            }

            var basePath = GetBasePath(configurationManager);
            var version = GetType().Assembly.GetName().Version?.ToString() ?? "2.0.0";

            _transformBasePath = basePath;
            _transformVersion = version;

            var transformationPayload = new JObject
            {
                ["id"] = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                ["fileNamePattern"] = "index.html",
                ["callbackAssembly"] = GetType().Assembly.FullName,
                ["callbackClass"] = GetType().FullName,
                ["callbackMethod"] = nameof(TransformIndexHtml)
            };

            registerMethod.Invoke(null, new object[] { transformationPayload });

            _logger.LogInformation("Successfully registered with File Transformation plugin");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to register with File Transformation plugin");
            return false;
        }
    }

    private static string _transformBasePath = string.Empty;
    private static string _transformVersion = "2.0.0";

    public static string TransformIndexHtml(dynamic payload)
    {
        try
        {
            var content = payload?.Contents ?? string.Empty;

            if (string.IsNullOrEmpty(content))
            {
                return content;
            }

            string scriptReplace = "<script plugin=\"TrailerPreview\".*?></script>";
            string scriptElement = string.Format(
                CultureInfo.InvariantCulture,
                "<script plugin=\"TrailerPreview\" version=\"{1}\" src=\"{0}/TrailerPreview/ClientScript\" defer></script>",
                _transformBasePath,
                _transformVersion);

            if (content.Contains(scriptElement, StringComparison.Ordinal))
            {
                return content;
            }

            content = Regex.Replace(content, scriptReplace, string.Empty);

            int bodyClosing = content.LastIndexOf("</body>", StringComparison.Ordinal);
            if (bodyClosing == -1)
            {
                return content;
            }

            content = content.Insert(bodyClosing, scriptElement);

            return content;
        }
        catch
        {
            return payload?.Contents ?? string.Empty;
        }
    }

    private void InjectClientScript(IApplicationPaths applicationPaths, IServerConfigurationManager configurationManager)
    {
        try
        {
            var indexFile = Path.Combine(applicationPaths.WebPath, "index.html");
            if (!File.Exists(indexFile))
            {
                _logger.LogWarning("Index file not found at {IndexFile}", indexFile);
                return;
            }

            var basePath = GetBasePath(configurationManager);
            var version = GetType().Assembly.GetName().Version?.ToString() ?? "2.0.0";

            string indexContents = File.ReadAllText(indexFile);
            string scriptReplace = "<script plugin=\"TrailerPreview\".*?></script>";
            string scriptElement = $"<script plugin=\"TrailerPreview\" version=\"{version}\" src=\"{basePath}/TrailerPreview/ClientScript\" defer></script>";

            if (indexContents.Contains(scriptElement))
            {
                _logger.LogInformation("TrailerPreview script already injected in {IndexFile}", indexFile);
                return;
            }

            _logger.LogInformation("Injecting TrailerPreview script into {IndexFile}", indexFile);

            indexContents = Regex.Replace(indexContents, scriptReplace, "");

            int bodyClosing = indexContents.LastIndexOf("</body>");
            if (bodyClosing == -1)
            {
                _logger.LogWarning("Could not find closing body tag in {IndexFile}", indexFile);
                return;
            }

            indexContents = indexContents.Insert(bodyClosing, scriptElement);
            File.WriteAllText(indexFile, indexContents);

            _logger.LogInformation("Successfully injected TrailerPreview script");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error injecting client script");
        }
    }

    private string GetBasePath(IServerConfigurationManager configurationManager)
    {
        try
        {
            var networkConfig = configurationManager.GetConfiguration("network");
            var configType = networkConfig.GetType();
            var basePathField = configType.GetProperty("BaseUrl");
            var confBasePath = basePathField?.GetValue(networkConfig)?.ToString()?.Trim('/');

            return string.IsNullOrEmpty(confBasePath) ? "" : "/" + confBasePath;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to get base path from network configuration");
            return "";
        }
    }

    public override string Name => "Trailer Preview";

    public override Guid Id => Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    public static Plugin? Instance { get; private set; }

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
