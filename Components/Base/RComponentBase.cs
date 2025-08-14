using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RR.Blazor.Attributes;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Base
{
    /// <summary>
    /// Base class for all RR.Blazor components providing common properties and functionality
    /// </summary>
    public abstract class RComponentBase : ComponentBase, IAsyncDisposable
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
        public DensityType Density { get; set; } = DensityType.Normal;
        
        /// <summary>
        /// Whether the component should take the full width of its container
        /// </summary>
        [Parameter]
        [AIParameter(Hint = "Make component take full width of container", IsRequired = false)]
        public bool FullWidth { get; set; }
        
        /// <summary>
        /// Elevation level (0-16) - controls shadow depth and prominence
        /// </summary>
        [Parameter]
        [AIParameter(Hint = "Use 0 for flat, 2-4 for standard, 8+ for prominent. -1 uses variant default", 
                     SuggestedValues = new[] { "0", "2", "4", "8", "16" }, IsRequired = false)]
        public int Elevation { get; set; } = -1;
        
        /// <summary>
        /// Captures any additional HTML attributes
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)] 
        public Dictionary<string, object> AdditionalAttributes { get; set; }
        
        #endregion

        #region Services

        [Inject] protected ILogger<RComponentBase>? Logger { get; set; }
        [Inject] protected IJSRuntime? JSRuntime { get; set; }
        
        private readonly List<CancellationTokenSource> _cancellationTokenSources = new();
        private bool _disposed;

        /// <summary>
        /// Get a cancellation token that is cancelled when the component is disposed
        /// </summary>
        protected CancellationToken ComponentCancellationToken 
        { 
            get
            {
                var cts = new CancellationTokenSource();
                _cancellationTokenSources.Add(cts);
                return cts.Token;
            }
        }

        #endregion

        /// <summary>
        /// Virtual method for component-specific disposal logic
        /// </summary>
        protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var cts in _cancellationTokenSources)
            {
                cts.Cancel();
                cts.Dispose();
            }
            _cancellationTokenSources.Clear();

            await DisposeAsyncCore();
        }
        
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
            
            // Add full width class
            if (FullWidth)
                classes.Add("w-full");
            
            // Add elevation shadow class
            if (Elevation >= 0)
                classes.Add($"shadow-{Math.Min(16, Math.Max(0, Elevation))}");
            
            return classes;
        }
        
        /// <summary>
        /// Gets density-specific CSS classes
        /// </summary>
        protected virtual string GetDensityClasses()
        {
            return Density switch
            {
                DensityType.Compact => "density-compact",
                DensityType.Dense => "density-dense",
                DensityType.Normal => "density-normal",
                DensityType.Spacious => "density-spacious",
                _ => "density-normal"
            };
        }
        
        /// <summary>
        /// Gets merged additional attributes with style
        /// </summary>
        protected virtual Dictionary<string, object> GetMergedAttributes()
        {
            var attributes = AdditionalAttributes ?? new Dictionary<string, object>();
            
            if (!string.IsNullOrEmpty(Style))
            {
                if (attributes.ContainsKey("style"))
                    attributes["style"] = $"{attributes["style"]};{Style}";
                else
                    attributes["style"] = Style;
            }
                
            return attributes;
        }
        
        /// <summary>
        /// Returns safely filtered HTML attributes using centralized RAttributeForwarder.
        /// </summary>
        protected virtual Dictionary<string, object> GetSafeAttributes()
        {
            return RAttributeForwarder.GetSafeAttributes(AdditionalAttributes);
        }
        
        #endregion
    }
}