using System.Collections.Generic;
using System.Threading.Tasks;

namespace RR.Blazor.Interfaces
{
    /// <summary>
    /// Interface for custom theme providers
    /// </summary>
    public interface ICustomThemeProvider
    {
        /// <summary>
        /// Get all available custom themes
        /// </summary>
        Task<Dictionary<string, string>> GetCustomThemesAsync();
        
        /// <summary>
        /// Validate a theme file exists and is valid
        /// </summary>
        Task<bool> ValidateThemeAsync(string themeName, string scssFilePath);
        
        /// <summary>
        /// Get the compiled CSS for a theme
        /// </summary>
        Task<string> GetCompiledThemeAsync(string themeName);
    }
}