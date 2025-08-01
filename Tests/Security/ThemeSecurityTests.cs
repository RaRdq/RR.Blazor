using Microsoft.Extensions.DependencyInjection;
using RR.Blazor.Extensions;
using System;
using Xunit;

namespace RR.Blazor.Tests.Security
{
    public class ThemeSecurityTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void WithCustomTheme_EmptyThemeName_ThrowsArgumentException(string themeName)
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new RRBlazorOptions();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                options.WithCustomTheme(themeName, "valid.scss"));
        }
        
        [Theory]
        [InlineData("theme_with_dots../../../etc/passwd")]
        [InlineData("theme/with/slashes")]
        [InlineData("theme\\with\\backslashes")]
        [InlineData("theme:with:colons")]
        [InlineData("theme with spaces")]
        [InlineData("theme@with#special$characters")]
        public void WithCustomTheme_InvalidThemeName_ThrowsArgumentException(string themeName)
        {
            // Arrange
            var options = new RRBlazorOptions();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                options.WithCustomTheme(themeName, "valid.scss"));
        }
        
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void WithCustomTheme_EmptyFilePath_ThrowsArgumentException(string filePath)
        {
            // Arrange
            var options = new RRBlazorOptions();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                options.WithCustomTheme("valid-theme", filePath));
        }
        
        [Theory]
        [InlineData("theme.css")]
        [InlineData("theme.txt")]
        [InlineData("theme")]
        [InlineData("theme.SCSS")]
        public void WithCustomTheme_InvalidFileExtension_ThrowsArgumentException(string filePath)
        {
            // Arrange
            var options = new RRBlazorOptions();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                options.WithCustomTheme("valid-theme", filePath));
        }
        
        [Theory]
        [InlineData("../../../etc/passwd.scss")]
        [InlineData("..\\..\\..\\windows\\system32\\config\\sam.scss")]
        [InlineData("/absolute/path/theme.scss")]
        [InlineData("C:\\absolute\\path\\theme.scss")]
        public void WithCustomTheme_PathTraversal_ThrowsArgumentException(string filePath)
        {
            // Arrange
            var options = new RRBlazorOptions();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                options.WithCustomTheme("valid-theme", filePath));
        }
        
        [Theory]
        [InlineData("valid-theme-123")]
        [InlineData("theme_with_underscores")]
        [InlineData("theme-with-hyphens")]
        [InlineData("UPPERCASE")]
        [InlineData("MixedCase")]
        public void WithCustomTheme_ValidThemeName_Success(string themeName)
        {
            // Arrange
            var options = new RRBlazorOptions();
            
            // Act & Assert - Should not throw
            var result = options.WithCustomTheme(themeName, "valid.scss");
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }
        
        [Theory]
        [InlineData("Themes/corporate.scss")]
        [InlineData("styles/themes/modern.scss")]
        [InlineData("custom-theme.scss")]
        public void WithCustomTheme_ValidFilePath_Success(string filePath)
        {
            // Arrange
            var options = new RRBlazorOptions();
            
            // Act & Assert - Should not throw
            var result = options.WithCustomTheme("valid-theme", filePath);
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }
        
        [Fact]
        public void WithCustomTheme_ThemeNameTooLong_ThrowsArgumentException()
        {
            // Arrange
            var options = new RRBlazorOptions();
            var longThemeName = new string('a', 51); // 51 characters, exceeds limit of 50
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                options.WithCustomTheme(longThemeName, "valid.scss"));
        }
        
        [Fact]
        public void WithCustomTheme_MaxLengthThemeName_Success()
        {
            // Arrange
            var options = new RRBlazorOptions();
            var maxLengthThemeName = new string('a', 50); // Exactly 50 characters
            
            // Act & Assert - Should not throw
            var result = options.WithCustomTheme(maxLengthThemeName, "valid.scss");
            Assert.NotNull(result);
        }
        
        [Fact]
        public void WithCustomThemes_NullDictionary_ThrowsArgumentNullException()
        {
            // Arrange
            var options = new RRBlazorOptions();
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                options.WithCustomThemes(null));
        }
        
        [Fact]
        public void WithCustomThemes_ValidThemes_Success()
        {
            // Arrange
            var options = new RRBlazorOptions();
            var themes = new System.Collections.Generic.Dictionary<string, string>
            {
                ["corporate"] = "themes/corporate.scss",
                ["modern"] = "themes/modern.scss"
            };
            
            // Act & Assert - Should not throw
            var result = options.WithCustomThemes(themes);
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }
    }
}