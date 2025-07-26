using Microsoft.AspNetCore.Components;
using RR.Blazor.Attributes;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Base
{
    /// <summary>
    /// Base class for components that display text content with optional icons
    /// </summary>
    public abstract class RTextComponentBase : RInteractiveComponentBase
    {
        #region Text Properties
        
        /// <summary>
        /// Primary text content
        /// </summary>
        [Parameter] 
        [AIParameter("Primary text content", Example = "\"Button Text\"")]
        public string Text { get; set; }
        
        /// <summary>
        /// Material icon name
        /// </summary>
        [Parameter] 
        [AIParameter("Material icon name", Example = "\"star\"")]
        public string Icon { get; set; }
        
        /// <summary>
        /// Subtitle or secondary text
        /// </summary>
        [Parameter] 
        [AIParameter("Subtitle or secondary text", Example = "\"Additional information\"")]
        public string Subtitle { get; set; }
        
        /// <summary>
        /// Title attribute for tooltips
        /// </summary>
        [Parameter] 
        [AIParameter("Title attribute for tooltips")]
        public string Title { get; set; }
        
        #endregion
        
        #region Abstract Methods
        
        /// <summary>
        /// Override this method to provide component-specific text size classes
        /// </summary>
        protected abstract string GetTextSizeClasses();
        
        /// <summary>
        /// Override this method to provide component-specific icon size classes
        /// </summary>
        protected abstract string GetIconSizeClasses();
        
        #endregion
        
        #region Styling Methods
        
        /// <summary>
        /// Gets CSS classes for text content
        /// </summary>
        protected virtual string GetTextClasses()
        {
            var classes = new List<string>();
            
            classes.Add(GetTextSizeClasses());
            classes.Add(GetDensityTextClasses());
            
            return string.Join(" ", classes.Where(c => !string.IsNullOrEmpty(c)));
        }
        
        /// <summary>
        /// Gets CSS classes for icon content
        /// </summary>
        protected virtual string GetIconClasses()
        {
            var classes = new List<string>
            {
                "material-symbols-rounded"
            };
            
            classes.Add(GetIconSizeClasses());
            classes.Add(GetDensityIconClasses());
            
            return string.Join(" ", classes.Where(c => !string.IsNullOrEmpty(c)));
        }
        
        /// <summary>
        /// Gets density-specific text classes
        /// </summary>
        protected virtual string GetDensityTextClasses()
        {
            return Density switch
            {
                ComponentDensity.Compact => "leading-tight",
                ComponentDensity.Dense => "leading-snug",
                ComponentDensity.Normal => "leading-normal",
                ComponentDensity.Spacious => "leading-relaxed",
                _ => "leading-normal"
            };
        }
        
        /// <summary>
        /// Gets density-specific icon classes
        /// </summary>
        protected virtual string GetDensityIconClasses()
        {
            return Density switch
            {
                ComponentDensity.Compact => "icon-compact",
                ComponentDensity.Dense => "icon-dense",
                ComponentDensity.Normal => "icon-normal",
                ComponentDensity.Spacious => "icon-spacious",
                _ => "icon-normal"
            };
        }
        
        /// <summary>
        /// Gets subtitle-specific CSS classes
        /// </summary>
        protected virtual string GetSubtitleClasses()
        {
            var classes = new List<string>
            {
                "text-secondary",
                "text-sm"
            };
            
            return string.Join(" ", classes);
        }
        
        /// <summary>
        /// Gets additional HTML attributes for text components
        /// </summary>
        protected override Dictionary<string, object> GetAdditionalAttributes()
        {
            var attributes = base.GetAdditionalAttributes();
            
            if (!string.IsNullOrEmpty(Title))
                attributes["title"] = Title;
                
            return attributes;
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Checks if the component has any text content
        /// </summary>
        protected bool HasText => !string.IsNullOrEmpty(Text);
        
        /// <summary>
        /// Checks if the component has an icon
        /// </summary>
        protected bool HasIcon => !string.IsNullOrEmpty(Icon);
        
        /// <summary>
        /// Checks if the component has a subtitle
        /// </summary>
        protected bool HasSubtitle => !string.IsNullOrEmpty(Subtitle);
        
        /// <summary>
        /// Checks if the component has any content to display
        /// </summary>
        protected bool HasContent => HasText || HasIcon || HasSubtitle || ChildContent != null;
        
        #endregion
    }
}