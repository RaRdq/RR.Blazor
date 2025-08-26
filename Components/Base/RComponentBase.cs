using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RR.Blazor.Attributes;
using RR.Blazor.Enums;
using RR.Blazor.Services;

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
        [Inject] protected IJavaScriptInteropService? JSInterop { get; set; }
        
        private readonly List<CancellationTokenSource> _cancellationTokenSources = new();
        private bool _disposed;
        private bool _jsInitialized;
        private bool? _isWebAssembly;

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

        #region Universal JavaScript Interop Support
        
        /// <summary>
        /// Override this to implement component-specific JavaScript initialization
        /// Called when JavaScript interop is safe and available
        /// </summary>
        protected virtual Task InitializeJavaScriptAsync()
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Detects if running in WebAssembly mode
        /// </summary>
        protected bool IsWebAssembly
        {
            get
            {
                if (!_isWebAssembly.HasValue)
                {
                    _isWebAssembly = JSRuntime is IJSInProcessRuntime || OperatingSystem.IsBrowser();
                }
                return _isWebAssembly.Value;
            }
        }
        
        protected async Task<T> SafeInvokeAsync<T>(string identifier, params object[] args)
        {
            if (_disposed) return default(T);
            
            try
            {
                if (!IsWebAssembly)
                {
                    T result = default(T);
                    await InvokeAsync(async () =>
                    {
                        await Task.Yield();
                        result = await JSInterop.TryInvokeAsync<T>(identifier, args);
                    });
                    return result;
                }
                return await JSInterop.TryInvokeAsync<T>(identifier, args);
            }
            catch (Exception ex) when (_disposed)
            {
                Logger?.LogDebug(ex, "JavaScript call ignored during component disposal: {Identifier}", identifier);
                return default(T);
            }
        }
        
        /// <summary>
        /// Safely invokes JavaScript function without expecting return value - optimized for void calls
        /// </summary>
        protected async Task SafeInvokeAsync(string identifier, params object[] args)
        {
            if (_disposed) return;
            
            try
            {
                if (!IsWebAssembly)
                {
                    await InvokeAsync(async () =>
                    {
                        await Task.Yield();
                        await JSInterop.TryInvokeVoidAsync(identifier, args);
                    });
                }
                else
                {
                    await JSInterop.TryInvokeVoidAsync(identifier, args);
                }
            }
            catch (Exception ex) when (_disposed)
            {
            }
        }

        /// <summary>
        /// Safely initializes JavaScript for the component - called automatically
        /// Only called when interactive rendering is available
        /// </summary>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !_jsInitialized && !_disposed)
            {
                if (!IsWebAssembly)
                {
                    await Task.Yield();
                }
                
                await InitializeJavaScriptAsync();
                _jsInitialized = true;
                Logger?.LogDebug("Component {ComponentType} JavaScript initialized (Mode: {Mode})", 
                    GetType().Name, IsWebAssembly ? "WebAssembly" : "Server");
            }
            
            await base.OnAfterRenderAsync(firstRender);
        }

        #endregion

        /// <summary>
        /// Virtual method for component-specific disposal logic
        /// </summary>
        protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            
            try
            {
                _disposed = true;

                foreach (var cts in _cancellationTokenSources)
                {
                    try
                    {
                        cts.Cancel();
                        cts.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogDebug(ex, "Error disposing cancellation token source");
                    }
                }
                _cancellationTokenSources.Clear();

                await DisposeAsyncCore();
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error during component disposal");
            }
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
                
            classes.Add(GetDensityClasses());
            
            if (FullWidth)
                classes.Add("w-full");
            
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
            if (AdditionalAttributes == null && string.IsNullOrEmpty(Style))
                return new Dictionary<string, object>();
            
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