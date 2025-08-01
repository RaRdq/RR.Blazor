using Microsoft.Extensions.Logging;
using RR.Blazor.Interfaces;
using RR.Blazor.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RR.Blazor.Services
{
    /// <summary>
    /// Default implementation of custom theme provider
    /// </summary>
    public class CustomThemeProvider : ICustomThemeProvider
    {
        private readonly ILogger<CustomThemeProvider> _logger;
        private readonly IThemeCompiler _compiler;
        private readonly ThemeConfiguration _configuration;

        public CustomThemeProvider(
            ILogger<CustomThemeProvider> logger, 
            IThemeCompiler compiler,
            ThemeConfiguration configuration)
        {
            _logger = logger;
            _compiler = compiler;
            _configuration = configuration;
        }

        public async Task<Dictionary<string, string>> GetCustomThemesAsync()
        {
            try
            {
                var validThemes = new Dictionary<string, string>();
                
                if (_configuration.CustomThemes?.Count > 0)
                {
                    foreach (var theme in _configuration.CustomThemes)
                    {
                        if (await ValidateThemeAsync(theme.Key, theme.Value))
                        {
                            validThemes[theme.Key] = theme.Value;
                        }
                        else
                        {
                            _logger.LogWarning("Theme '{ThemeName}' at '{FilePath}' is invalid and will be skipped", 
                                theme.Key, theme.Value);
                        }
                    }
                }
                
                return validThemes;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error loading custom themes");
                return new Dictionary<string, string>();
            }
        }

        public async Task<bool> ValidateThemeAsync(string themeName, string scssFilePath)
        {
            try
            {
                // Basic name validation
                if (string.IsNullOrWhiteSpace(themeName) || string.IsNullOrWhiteSpace(scssFilePath))
                    return false;

                // Check if file exists
                if (!File.Exists(scssFilePath))
                {
                    _logger.LogWarning("Theme file not found: {FilePath}", scssFilePath);
                    return false;
                }

                // Read and validate SCSS content
                var content = await File.ReadAllTextAsync(scssFilePath);
                if (string.IsNullOrWhiteSpace(content))
                    return false;

                // Validate SCSS syntax
                var isValidScss = await _compiler.ValidateScssAsync(content);
                if (!isValidScss)
                {
                    var errors = await _compiler.GetCompilationErrorsAsync(content);
                    _logger.LogWarning("Theme '{ThemeName}' has SCSS errors: {Errors}", 
                        themeName, string.Join(", ", errors));
                    return false;
                }

                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error validating theme '{ThemeName}' at '{FilePath}'", themeName, scssFilePath);
                return false;
            }
        }

        public async Task<string?> GetCompiledThemeAsync(string themeName)
        {
            try
            {
                if (_configuration.CustomThemes?.TryGetValue(themeName, out var filePath) == true)
                {
                    if (await ValidateThemeAsync(themeName, filePath))
                    {
                        var scssContent = await File.ReadAllTextAsync(filePath);
                        return await _compiler.CompileThemeAsync(scssContent, themeName);
                    }
                }
                
                return null;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error compiling theme '{ThemeName}'", themeName);
                return null;
            }
        }
    }
}