using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RR.Blazor.Attributes;
using System.Threading;

namespace RR.Blazor.Components.Base
{
    /// <summary>
    /// Base class for interactive components that can be clicked and have loading states
    /// </summary>
    public abstract class RInteractiveComponentBase : RComponentBase, IDisposable
    {
        #region Private Fields

        private Timer _debounceTimer;
        private MouseEventArgs _pendingClickEvent;
        private bool _hasPendingClick = false;

        #endregion

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
        /// Text to display when component is in loading state
        /// </summary>
        [Parameter]
        [AIParameter("Loading text to display")]
        public string LoadingText { get; set; } = "Loading...";

        /// <summary>
        /// Enable automatic debounce and loading state management to prevent duplicate clicks
        /// </summary>
        [Parameter]
        [AIParameter("Enable debounce and auto-loading")]
        public bool EnableDebounce { get; set; } = false;

        /// <summary>
        /// Debounce timeout in milliseconds
        /// </summary>
        [Parameter]
        [AIParameter("Debounce timeout in milliseconds")]
        public int DebounceTimeoutMs { get; set; } = 300;
        
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
                if (EnableDebounce)
                {
                    _pendingClickEvent = e;
                    _hasPendingClick = true;

                    _debounceTimer?.Dispose();
                    _debounceTimer = new Timer(async _ =>
                    {
                        if (_hasPendingClick)
                        {
                            _hasPendingClick = false;
                            await InvokeAsync(async () =>
                            {
                                await OnClick.InvokeAsync(_pendingClickEvent);
                            });
                        }
                    }, null, DebounceTimeoutMs, Timeout.Infinite);
                }
                else
                {
                    await OnClick.InvokeAsync(e);
                }
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
        /// Gets accessibility and interactive attributes
        /// </summary>
        protected virtual Dictionary<string, object> GetAccessibilityAttributes()
        {
            var attributes = new Dictionary<string, object>();
            
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

        #region IDisposable

        /// <summary>
        /// Disposes the debounce timer
        /// </summary>
        public virtual void Dispose()
        {
            _debounceTimer?.Dispose();
            _debounceTimer = null;
        }

        #endregion
    }
}