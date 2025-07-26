using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RR.Blazor.Attributes;

namespace RR.Blazor.Components.Base
{
    /// <summary>
    /// Base class for interactive components that can be clicked and have loading states
    /// </summary>
    public abstract class RInteractiveComponentBase : RComponentBase
    {
        #region Interactive Properties
        
        /// <summary>
        /// Event callback for click events
        /// </summary>
        [Parameter] 
        [AIParameter("Click event handler")]
        public EventCallback<MouseEventArgs> OnClick { get; set; }
        
        /// <summary>
        /// Whether the component is in a loading state
        /// </summary>
        [Parameter] 
        [AIParameter("Loading state")]
        public bool Loading { get; set; }
        
        /// <summary>
        /// ARIA label for accessibility
        /// </summary>
        [Parameter] 
        [AIParameter("ARIA label for accessibility")]
        public string AriaLabel { get; set; }
        
        /// <summary>
        /// Tab index for keyboard navigation
        /// </summary>
        [Parameter] 
        [AIParameter("Tab index for keyboard navigation")]
        public int TabIndex { get; set; }
        
        #endregion
        
        #region Event Handling
        
        /// <summary>
        /// Handles click events with disabled and loading state checks
        /// </summary>
        protected virtual async Task HandleClick(MouseEventArgs e)
        {
            if (!Disabled && !Loading && OnClick.HasDelegate)
            {
                await OnClick.InvokeAsync(e);
            }
        }
        
        /// <summary>
        /// Handles keyboard events (Enter and Space as click)
        /// </summary>
        protected virtual async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (!Disabled && !Loading && (e.Key == "Enter" || e.Key == " "))
            {
                await HandleClick(new MouseEventArgs());
            }
        }
        
        #endregion
        
        #region Styling Methods
        
        /// <summary>
        /// Gets CSS classes for interactive states
        /// </summary>
        protected override IEnumerable<string> GetBaseCssClasses()
        {
            var classes = base.GetBaseCssClasses().ToList();
            
            if (Loading)
                classes.Add("loading");
                
            if (!Disabled && !Loading)
                classes.Add("interactive");
                
            return classes;
        }
        
        /// <summary>
        /// Gets cursor CSS class based on component state
        /// </summary>
        protected virtual string GetCursorClasses()
        {
            if (Disabled)
                return "cursor-not-allowed";
            if (Loading)
                return "cursor-wait";
            return "cursor-pointer";
        }
        
        /// <summary>
        /// Gets additional HTML attributes for interactive components
        /// </summary>
        protected override Dictionary<string, object> GetAdditionalAttributes()
        {
            var attributes = base.GetAdditionalAttributes();
            
            if (!string.IsNullOrEmpty(AriaLabel))
                attributes["aria-label"] = AriaLabel;
                
            if (TabIndex != 0)
                attributes["tabindex"] = TabIndex;
                
            if (Disabled)
                attributes["aria-disabled"] = "true";
                
            if (Loading)
                attributes["aria-busy"] = "true";
                
            return attributes;
        }
        
        #endregion
    }
}