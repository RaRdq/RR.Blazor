using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Linq.Expressions;
using System.Collections.Concurrent;

namespace RR.Blazor.Components.Base;

/// <summary>
/// Provides efficient, type-safe attribute forwarding for RR.Blazor components.
/// Uses expression trees and caching for optimal performance.
/// </summary>
public static class RAttributeForwarder
{
    private static readonly ConcurrentDictionary<Type, ForwardingDelegate> _forwarderCache = new();
    
    private static readonly HashSet<string> ValidHtmlAttributePrefixes = new()
    {
        "data-", "aria-", "role", "tabindex", "contenteditable",
        "draggable", "spellcheck", "translate", "accesskey",
        "contextmenu", "dir", "hidden", "lang", "title"
    };
    
    private static readonly HashSet<string> ValidHtmlAttributes = new()
    {
        "id", "class", "style", "name", "value", "type", "href", "src", "alt",
        "width", "height", "disabled", "readonly", "checked", "selected",
        "placeholder", "maxlength", "minlength", "max", "min", "step", "pattern",
        "required", "autofocus", "autocomplete", "multiple", "accept",
        "rows", "cols", "wrap", "size", "form", "for", "label",
        "target", "rel", "download", "media", "action", "method", "enctype",
        "title", "tabindex", "role", "contenteditable", "draggable", "spellcheck",
        "translate", "accesskey", "contextmenu", "dir", "hidden", "lang"
    };
    
    public delegate void ForwardingDelegate(RenderTreeBuilder builder, ref int sequence, object component);
    
    private static bool ShouldForwardAttribute(string propertyName, Type propertyType)
    {
        if (string.IsNullOrEmpty(propertyName)) return false;
        
        // CRITICAL: Skip any property name starting with @ (Blazor directives)
        if (propertyName.StartsWith("@")) return false;
        
        // Skip property names containing @ anywhere (like @bind, @onclick, etc)
        if (propertyName.Contains("@")) return false;
        
        // Skip Blazor-specific property patterns
        if (propertyName.EndsWith("Changed") || 
            propertyName.EndsWith("Expression") ||
            propertyName.EndsWith("Callback") ||
            propertyName == "ChildContent" ||
            propertyName == "RenderFragment" ||
            propertyName == "CaptureUnmatchedValues" ||
            propertyName.StartsWith("bind", StringComparison.OrdinalIgnoreCase) ||
            propertyName.Contains("bind", StringComparison.OrdinalIgnoreCase) ||
            propertyName == "Value" ||  // RR.Blazor component Value properties should not be forwarded
            propertyName == "ValueChanged" ||
            propertyName == "Checked" ||
            propertyName == "CheckedChanged")
            return false;
        
        // Skip any Blazor-specific types
        if (propertyType.IsGenericType)
        {
            var genericTypeDef = propertyType.GetGenericTypeDefinition();
            var genericTypeName = genericTypeDef.Name;
            
            // Skip EventCallback, RenderFragment, Expression, etc.
            if (genericTypeName.Contains("EventCallback") ||
                genericTypeName.Contains("RenderFragment") ||
                genericTypeName.Contains("Expression"))
                return false;
        }
        
        // Skip delegate types
        if (typeof(Delegate).IsAssignableFrom(propertyType))
            return false;
            
        // CRITICAL: Skip properties with Symbol in their type name (fix for Symbol() errors)
        if (propertyType.Name.Contains("Symbol") || propertyType.FullName?.Contains("Symbol") == true)
            return false;
        
        // Skip IEnumerable types (except string)
        if (propertyType != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(propertyType))
            return false;
        
        // Only forward simple value types and strings
        if (!propertyType.IsValueType && propertyType != typeof(string) && !propertyType.IsEnum)
            return false;
        
        var lowerName = propertyName.ToLowerInvariant();
        
        // Check if it's a standard HTML attribute
        if (ValidHtmlAttributes.Contains(lowerName))
            return true;
        
        // Check if it starts with a valid prefix
        if (ValidHtmlAttributePrefixes.Any(prefix => lowerName.StartsWith(prefix)))
            return true;
        
        // Otherwise, don't forward it
        return false;
    }
    
    private static string ToHtmlAttributeName(string propertyName)
    {
        // Convert PascalCase to kebab-case for custom attributes
        if (propertyName.StartsWith("data", StringComparison.OrdinalIgnoreCase) ||
            propertyName.StartsWith("aria", StringComparison.OrdinalIgnoreCase))
        {
            // Convert DataFoo to data-foo, AriaLabel to aria-label
            var result = new System.Text.StringBuilder();
            for (int i = 0; i < propertyName.Length; i++)
            {
                if (i > 0 && char.IsUpper(propertyName[i]))
                {
                    result.Append('-');
                    result.Append(char.ToLower(propertyName[i]));
                }
                else
                {
                    result.Append(char.ToLower(propertyName[i]));
                }
            }
            return result.ToString();
        }
        
        // For standard HTML attributes, use lowercase
        return propertyName.ToLowerInvariant();
    }
    
    /// <summary>
    /// Forwards all valid HTML attributes from [Parameter] properties to the render tree builder.
    /// Filters out Blazor-specific parameters like EventCallbacks, RenderFragments, etc.
    /// Uses cached compiled expressions for optimal performance.
    /// </summary>
    public static void ForwardAllParameters<TComponent>(this RenderTreeBuilder builder, ref int sequence, TComponent component) 
        where TComponent : ComponentBase
    {
        var forwarder = _forwarderCache.GetOrAdd(typeof(TComponent), CreateForwarder);
        forwarder(builder, ref sequence, component);
    }
    
    /// <summary>
    /// Forwards valid HTML attributes with filtering support.
    /// Only forwards simple value types and strings as HTML attributes.
    /// </summary>
    public static void ForwardParameters<TComponent>(
        this RenderTreeBuilder builder, 
        ref int sequence, 
        TComponent component,
        params string[] excludeProperties) 
        where TComponent : ComponentBase
    {
        var componentType = typeof(TComponent);
        var properties = componentType.GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(ParameterAttribute), true).Any() 
                       && !excludeProperties.Contains(p.Name)
                       && ShouldForwardAttribute(p.Name, p.PropertyType))
            .OrderBy(p => p.Name);
        
        foreach (var property in properties)
        {
            var value = property.GetValue(component);
            if (value != null || property.PropertyType.IsValueType)
            {
                var htmlAttributeName = ToHtmlAttributeName(property.Name);
                builder.AddAttribute(++sequence, htmlAttributeName, value);
            }
        }
    }
    
    /// <summary>
    /// Gets safe HTML attributes from AdditionalAttributes dictionary, filtering out Blazor-specific attributes.
    /// Use this for @attributes directive in components that use CaptureUnmatchedValues.
    /// </summary>
    public static Dictionary<string, object> GetSafeAttributes(Dictionary<string, object> additionalAttributes)
    {
        if (additionalAttributes == null) return null;
        
        var filtered = new Dictionary<string, object>();
        
        foreach (var attr in additionalAttributes)
        {
            if (ShouldForwardAttribute(attr.Key, attr.Value?.GetType() ?? typeof(object)))
            {
                filtered[attr.Key] = attr.Value;
            }
        }
        
        return filtered;
    }
    
    /// <summary>
    /// Creates a compiled expression tree for efficient attribute forwarding.
    /// Only includes valid HTML attributes, filtering out Blazor-specific parameters.
    /// </summary>
    private static ForwardingDelegate CreateForwarder(Type componentType)
    {
        var builderParam = Expression.Parameter(typeof(RenderTreeBuilder), "builder");
        var sequenceParam = Expression.Parameter(typeof(int).MakeByRefType(), "sequence");
        var componentParam = Expression.Parameter(typeof(object), "component");
        
        var componentTyped = Expression.Convert(componentParam, componentType);
        var expressions = new List<Expression>();
        
        var properties = componentType.GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(ParameterAttribute), true).Any() &&
                       ShouldForwardAttribute(p.Name, p.PropertyType))
            .OrderBy(p => p.Name);
        
        var addAttributeMethod = typeof(RenderTreeBuilder).GetMethod(
            nameof(RenderTreeBuilder.AddAttribute),
            new[] { typeof(int), typeof(string), typeof(object) });
        
        var toHtmlAttributeNameMethod = typeof(RAttributeForwarder).GetMethod(
            nameof(ToHtmlAttributeName),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        foreach (var property in properties)
        {
            // ++sequence
            var incrementSequence = Expression.PreIncrementAssign(sequenceParam);
            
            // component.PropertyName
            var propertyAccess = Expression.Property(componentTyped, property);
            
            // Convert to object if needed
            var valueAsObject = property.PropertyType.IsValueType 
                ? Expression.Convert(propertyAccess, typeof(object))
                : (Expression)propertyAccess;
            
            // ToHtmlAttributeName(property.Name)
            var htmlAttrName = Expression.Call(
                toHtmlAttributeNameMethod,
                Expression.Constant(property.Name));
            
            // builder.AddAttribute(sequence, htmlAttributeName, value)
            var addAttribute = Expression.Call(
                builderParam,
                addAttributeMethod,
                sequenceParam,
                htmlAttrName,
                valueAsObject);
            
            // Add null check for reference types (strings)
            if (!property.PropertyType.IsValueType)
            {
                var condition = Expression.IfThen(
                    Expression.NotEqual(propertyAccess, Expression.Constant(null)),
                    Expression.Block(incrementSequence, addAttribute));
                expressions.Add(condition);
            }
            else
            {
                expressions.Add(Expression.Block(incrementSequence, addAttribute));
            }
        }
        
        if (expressions.Count == 0)
        {
            // Return empty delegate if no valid attributes to forward
            var emptyBody = Expression.Empty();
            var emptyLambda = Expression.Lambda<ForwardingDelegate>(emptyBody, builderParam, sequenceParam, componentParam);
            return emptyLambda.Compile();
        }
        
        var body = Expression.Block(expressions);
        var lambda = Expression.Lambda<ForwardingDelegate>(body, builderParam, sequenceParam, componentParam);
        
        return lambda.Compile();
    }
}

/// <summary>
/// Base class extension for simplified attribute forwarding in components.
/// </summary>
public abstract class RForwardingComponentBase : ComponentBase
{
    /// <summary>Captures any additional HTML attributes</summary>
    [Parameter(CaptureUnmatchedValues = true)] 
    public Dictionary<string, object> AdditionalAttributes { get; set; }
    
    /// <summary>
    /// Returns safely filtered HTML attributes using centralized RAttributeForwarder.
    /// </summary>
    protected virtual Dictionary<string, object> GetSafeAttributes()
    {
        return RAttributeForwarder.GetSafeAttributes(AdditionalAttributes);
    }
    
    /// <summary>
    /// Forwards all parameters to a child component in BuildRenderTree.
    /// </summary>
    protected void ForwardParameters(RenderTreeBuilder builder, ref int sequence)
    {
        builder.ForwardAllParameters(ref sequence, this);
    }
    
    /// <summary>
    /// Forwards parameters excluding specific properties.
    /// </summary>
    protected void ForwardParametersExcept(RenderTreeBuilder builder, ref int sequence, params string[] excludeProperties)
    {
        builder.ForwardParameters(ref sequence, this, excludeProperties);
    }
}

/// <summary>
/// Attribute to mark properties that should be excluded from automatic forwarding.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NoForwardAttribute : Attribute
{
}

/// <summary>
/// Simplified parameter forwarding using source generators (future enhancement).
/// This would generate optimal forwarding code at compile time.
/// </summary>
public static class RAttributeForwarderExtensions
{
    /// <summary>
    /// Example usage in a component:
    /// builder.OpenComponent<RButton>(0);
    /// builder.ForwardFrom(this, ref seq, except: new[] { "ChildContent" });
    /// builder.CloseComponent();
    /// </summary>
    public static void ForwardFrom<TSource>(
        this RenderTreeBuilder builder, 
        TSource source, 
        ref int sequence,
        string[] except = null) 
        where TSource : ComponentBase
    {
        except ??= Array.Empty<string>();
        builder.ForwardParameters(ref sequence, source, except);
    }
    
    /// <summary>
    /// Fluent API for attribute forwarding with filtering.
    /// </summary>
    public static AttributeForwardingBuilder<TComponent> Forward<TComponent>(
        this RenderTreeBuilder builder, 
        TComponent component) 
        where TComponent : ComponentBase
    {
        return new AttributeForwardingBuilder<TComponent>(builder, component);
    }
}

/// <summary>
/// Fluent builder for attribute forwarding configuration.
/// </summary>
public class AttributeForwardingBuilder<TComponent> where TComponent : ComponentBase
{
    private readonly RenderTreeBuilder _builder;
    private readonly TComponent _component;
    private readonly HashSet<string> _excludedProperties = new();
    private int _startSequence = 0;
    
    public AttributeForwardingBuilder(RenderTreeBuilder builder, TComponent component)
    {
        _builder = builder;
        _component = component;
    }
    
    public AttributeForwardingBuilder<TComponent> StartingAt(int sequence)
    {
        _startSequence = sequence;
        return this;
    }
    
    public AttributeForwardingBuilder<TComponent> Except(params string[] propertyNames)
    {
        foreach (var name in propertyNames)
        {
            _excludedProperties.Add(name);
        }
        return this;
    }
    
    public AttributeForwardingBuilder<TComponent> ExceptChildContent()
    {
        _excludedProperties.Add("ChildContent");
        return this;
    }
    
    public int Apply()
    {
        var sequence = _startSequence;
        _builder.ForwardParameters(ref sequence, _component, _excludedProperties.ToArray());
        return sequence;
    }
}