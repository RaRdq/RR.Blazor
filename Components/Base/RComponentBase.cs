using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Base
{
    /// <summary>
    /// Base class for all RR.Blazor components providing common properties and functionality
    /// </summary>
    public abstract class RComponentBase : ComponentBase
    {
        #region Universal Properties
        
        /// <summary>
        /// Additional CSS classes to apply to the component
        /// </summary>
        [Parameter] 
        public string Class { get; set; } = "";
        
        /// <summary>
        /// Additional CSS styles to apply to the component
        /// </summary>
        [Parameter] 
        public string Style { get; set; } = "";
        
        /// <summary>
        /// Whether the component is disabled
        /// </summary>
        [Parameter] 
        public bool Disabled { get; set; }
        
        /// <summary>
        /// Custom content to render inside the component
        /// </summary>
        [Parameter] 
        public RenderFragment ChildContent { get; set; }
        
        /// <summary>
        /// Component density affecting spacing and sizing
        /// </summary>
        [Parameter] 
        public ComponentDensity Density { get; set; } = ComponentDensity.Normal;
        
        /// <summary>
        /// Captures any additional HTML attributes
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)] 
        public Dictionary<string, object> AdditionalAttributes { get; set; }
        
        #endregion
        
        #region Shared Methods
        
        /// <summary>
        /// Gets the base CSS classes for the component
        /// </summary>
        protected virtual string GetBaseClasses()
        {
            var classes = GetBaseCssClasses().Where(c => !string.IsNullOrEmpty(c));
            return string.Join(" ", classes);
        }
        
        /// <summary>
        /// Override this method to provide component-specific CSS classes
        /// </summary>
        protected virtual IEnumerable<string> GetBaseCssClasses()
        {
            var classes = new List<string>();
            
            if (!string.IsNullOrEmpty(Class))
                classes.Add(Class);
                
            if (Disabled)
                classes.Add("disabled");
                
            // Add density classes
            classes.Add(GetDensityClasses());
            
            return classes;
        }
        
        /// <summary>
        /// Gets density-specific CSS classes
        /// </summary>
        protected virtual string GetDensityClasses()
        {
            return Density switch
            {
                ComponentDensity.Compact => "density-compact",
                ComponentDensity.Dense => "density-dense",
                ComponentDensity.Normal => "density-normal",
                ComponentDensity.Spacious => "density-spacious",
                _ => "density-normal"
            };
        }
        
        /// <summary>
        /// Gets additional HTML attributes as a dictionary
        /// </summary>
        protected virtual Dictionary<string, object> GetAdditionalAttributes()
        {
            var attributes = new Dictionary<string, object>();
            
            if (AdditionalAttributes != null)
            {
                foreach (var attr in AdditionalAttributes)
                {
                    attributes[attr.Key] = attr.Value;
                }
            }
            
            if (!string.IsNullOrEmpty(Style))
                attributes["style"] = Style;
                
            return attributes;
        }
        
        #endregion
    }
}