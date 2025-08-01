using System.Threading.Tasks;

namespace RR.Blazor.Interfaces
{
    /// <summary>
    /// Interface for SCSS theme compilation
    /// </summary>
    public interface IThemeCompiler
    {
        /// <summary>
        /// Compile SCSS theme file to CSS
        /// </summary>
        Task<string> CompileThemeAsync(string scssContent, string themeName);
        
        /// <summary>
        /// Validate SCSS syntax
        /// </summary>
        Task<bool> ValidateScssAsync(string scssContent);
        
        /// <summary>
        /// Get compilation errors if any
        /// </summary>
        Task<string[]> GetCompilationErrorsAsync(string scssContent);
    }
}