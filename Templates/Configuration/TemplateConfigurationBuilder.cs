using RR.Blazor.Enums;
using RR.Blazor.Templates.Stack;

namespace RR.Blazor.Templates.Configuration;

/// <summary>
/// Fluent builder for template configuration
/// Used in AddRRBlazor() to configure global template defaults
/// </summary>
public class TemplateConfigurationBuilder
{
    private readonly TemplateConfiguration _configuration = new();


    /// <summary>
    /// Configure currency template defaults
    /// </summary>
    public TemplateConfigurationBuilder WithCurrencyDefaults(Action<CurrencyDefaultsBuilder> configure)
    {
        var builder = new CurrencyDefaultsBuilder(_configuration.Currency);
        configure(builder);
        return this;
    }

    /// <summary>
    /// Configure stack template defaults
    /// </summary>
    public TemplateConfigurationBuilder WithStackDefaults(Action<StackDefaultsBuilder> configure)
    {
        var builder = new StackDefaultsBuilder(_configuration.Stack);
        configure(builder);
        return this;
    }

    /// <summary>
    /// Configure smart detection settings
    /// </summary>
    public TemplateConfigurationBuilder WithSmartDetection(Action<SmartDetectionBuilder> configure)
    {
        var builder = new SmartDetectionBuilder(_configuration.SmartDetection);
        configure(builder);
        return this;
    }

    /// <summary>
    /// Build the final configuration
    /// </summary>
    public TemplateConfiguration Build() => _configuration;
}


/// <summary>
/// Builder for currency template defaults
/// </summary>
public class CurrencyDefaultsBuilder
{
    private readonly CurrencyTemplateDefaults _defaults;

    public CurrencyDefaultsBuilder(CurrencyTemplateDefaults defaults)
    {
        _defaults = defaults;
    }

    public CurrencyDefaultsBuilder WithCurrencyCode(string currencyCode)
    {
        _defaults.DefaultCurrencyCode = currencyCode;
        return this;
    }

    public CurrencyDefaultsBuilder WithFormat(string format)
    {
        _defaults.DefaultFormat = format;
        return this;
    }

    public CurrencyDefaultsBuilder Compact(bool compact = true)
    {
        _defaults.DefaultCompact = compact;
        return this;
    }

    public CurrencyDefaultsBuilder ShowColors(bool showColors = true)
    {
        _defaults.DefaultShowColors = showColors;
        return this;
    }

    public CurrencyDefaultsBuilder WithAutoCompactThreshold(decimal threshold)
    {
        _defaults.AutoCompactThreshold = threshold;
        return this;
    }
}

/// <summary>
/// Builder for stack template defaults
/// </summary>
public class StackDefaultsBuilder
{
    private readonly StackTemplateDefaults _defaults;

    public StackDefaultsBuilder(StackTemplateDefaults defaults)
    {
        _defaults = defaults;
    }

    public StackDefaultsBuilder WithOrientation(StackOrientation orientation)
    {
        _defaults.DefaultOrientation = orientation;
        return this;
    }

    public StackDefaultsBuilder TruncateText(bool truncate = true)
    {
        _defaults.DefaultTruncateText = truncate;
        return this;
    }

    public StackDefaultsBuilder WithMaxLength(int maxLength)
    {
        _defaults.DefaultMaxLength = maxLength;
        return this;
    }

    public StackDefaultsBuilder WithSize(SizeType size)
    {
        _defaults.DefaultSize = size;
        return this;
    }

    public StackDefaultsBuilder WithDensity(DensityType density)
    {
        _defaults.DefaultDensity = density;
        return this;
    }
}

/// <summary>
/// Builder for smart detection settings
/// </summary>
public class SmartDetectionBuilder
{
    private readonly SmartDetectionSettings _settings;

    public SmartDetectionBuilder(SmartDetectionSettings settings)
    {
        _settings = settings;
    }

    public SmartDetectionBuilder Enable(bool enabled = true)
    {
        _settings.Enabled = enabled;
        return this;
    }

    public SmartDetectionBuilder WithAutoApplyThreshold(double threshold)
    {
        _settings.AutoApplyThreshold = Math.Max(0.0, Math.Min(1.0, threshold));
        return this;
    }

    public SmartDetectionBuilder WithSuggestionThreshold(double threshold)
    {
        _settings.SuggestionThreshold = Math.Max(0.0, Math.Min(1.0, threshold));
        return this;
    }

    public SmartDetectionBuilder AnalyzeSampleData(bool analyze = true)
    {
        _settings.AnalyzeSampleData = analyze;
        return this;
    }

    public SmartDetectionBuilder WithSampleDataLimit(int limit)
    {
        _settings.SampleDataLimit = Math.Max(1, Math.Min(100, limit));
        return this;
    }
}