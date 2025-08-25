using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Models;
using RR.Blazor.Templates.Actions;
using System.Linq.Expressions;

namespace RR.Blazor.Services;

/// <summary>
/// Renders column content based on template type
/// </summary>
public class ColumnTemplateRenderer<TItem>(ColumnDefinition<TItem> column)
  where TItem : class
{
  private static readonly Dictionary<Type, ColumnTemplate> _typeTemplateMap = new()
    {
        { typeof(bool), ColumnTemplate.Boolean },
        { typeof(bool?), ColumnTemplate.Boolean },
        { typeof(DateTime), ColumnTemplate.DateTime },
        { typeof(DateTime?), ColumnTemplate.DateTime },
        { typeof(DateOnly), ColumnTemplate.Date },
        { typeof(DateOnly?), ColumnTemplate.Date },
        { typeof(TimeOnly), ColumnTemplate.Time },
        { typeof(TimeOnly?), ColumnTemplate.Time },
        { typeof(int), ColumnTemplate.Number },
        { typeof(int?), ColumnTemplate.Number },
        { typeof(long), ColumnTemplate.Number },
        { typeof(long?), ColumnTemplate.Number },
        { typeof(decimal), ColumnTemplate.Number },
        { typeof(decimal?), ColumnTemplate.Number },
        { typeof(double), ColumnTemplate.Number },
        { typeof(double?), ColumnTemplate.Number },
        { typeof(float), ColumnTemplate.Number },
        { typeof(float?), ColumnTemplate.Number }
    };

    public void RenderCell(RenderTreeBuilder builder, TItem item)
    {
        var template = column.Template;
        
        // Auto-detect template if set to Auto
        if (template == ColumnTemplate.Auto)
        {
            template = DetectTemplate();
        }
        
        // Apply cell class
        var cellClass = column.CellClass;
        if (column.CellClassFunc != null)
        {
            var customClass = column.CellClassFunc(item);
            cellClass = string.IsNullOrEmpty(cellClass) 
                ? customClass 
                : $"{cellClass} {customClass}";
        }
        
        if (!string.IsNullOrEmpty(cellClass))
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", cellClass);
        }
        
        // Render based on template
        switch (template)
        {
            case ColumnTemplate.Text:
                RenderText(builder, item, 2);
                break;
            case ColumnTemplate.Number:
                RenderNumber(builder, item, 2);
                break;
            case ColumnTemplate.Currency:
                RenderCurrency(builder, item, 2);
                break;
            case ColumnTemplate.Percentage:
                RenderPercentage(builder, item, 2);
                break;
            case ColumnTemplate.Date:
                RenderDate(builder, item, 2);
                break;
            case ColumnTemplate.DateTime:
                RenderDateTime(builder, item, 2);
                break;
            case ColumnTemplate.Time:
                RenderTime(builder, item, 2);
                break;
            case ColumnTemplate.Boolean:
                RenderBoolean(builder, item, 2);
                break;
            case ColumnTemplate.Status:
                RenderStatus(builder, item, 2);
                break;
            case ColumnTemplate.Progress:
                RenderProgress(builder, item, 2);
                break;
            case ColumnTemplate.Rating:
                RenderRating(builder, item, 2);
                break;
            case ColumnTemplate.Email:
                RenderEmail(builder, item, 2);
                break;
            case ColumnTemplate.Phone:
                RenderPhone(builder, item, 2);
                break;
            case ColumnTemplate.Link:
                RenderLink(builder, item, 2);
                break;
            case ColumnTemplate.Image:
                RenderImage(builder, item, 2);
                break;
            case ColumnTemplate.Avatar:
                RenderAvatar(builder, item, 2);
                break;
            case ColumnTemplate.Tags:
                RenderTags(builder, item, 2);
                break;
            case ColumnTemplate.Actions:
                RenderActions(builder, item, 2);
                break;
            default:
                RenderText(builder, item, 2);
                break;
        }
        
        if (!string.IsNullOrEmpty(cellClass))
        {
            builder.CloseElement(); // div
        }
    }
    
    private ColumnTemplate DetectTemplate()
    {
        // Check property name patterns
        var propertyName = column.Key?.ToLowerInvariant() ?? "";
        
        if (propertyName.Contains("email"))
            return ColumnTemplate.Email;
        if (propertyName.Contains("phone") || propertyName.Contains("mobile"))
            return ColumnTemplate.Phone;
        if (propertyName.Contains("url") || propertyName.Contains("link") || propertyName.Contains("website"))
            return ColumnTemplate.Link;
        if (propertyName.Contains("status") || propertyName.Contains("state"))
            return ColumnTemplate.Status;
        if (propertyName.Contains("price") || propertyName.Contains("amount") || 
            propertyName.Contains("cost") || propertyName.Contains("salary") || 
            propertyName.Contains("payment") || propertyName.Contains("fee"))
            return ColumnTemplate.Currency;
        if (propertyName.Contains("percent") || propertyName.Contains("rate"))
            return ColumnTemplate.Percentage;
        if (propertyName.Contains("progress"))
            return ColumnTemplate.Progress;
        if (propertyName.Contains("rating") || propertyName.Contains("score"))
            return ColumnTemplate.Rating;
        if (propertyName.Contains("image") || propertyName.Contains("photo") || 
            propertyName.Contains("picture") || propertyName.Contains("avatar"))
            return ColumnTemplate.Image;
        if (propertyName.Contains("tag") || propertyName.Contains("label") || 
            propertyName.Contains("category"))
            return ColumnTemplate.Tags;
        
        // Check property type
        if (column.Property != null)
        {
            var memberExpr = GetMemberExpression(column.Property.Body);
            if (memberExpr != null)
            {
                var propertyType = memberExpr.Type;
                if (_typeTemplateMap.TryGetValue(propertyType, out var template))
                    return template;
            }
        }
        
        return ColumnTemplate.Text;
    }
    
    #region Template Renderers
    
    private void RenderText(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetFormattedValue(item);
        
        if (string.IsNullOrEmpty(value) || value == column.EmptyText)
        {
            builder.OpenElement(sequence, "span");
            builder.AddAttribute(sequence + 1, "class", "text-muted");
            builder.AddContent(sequence + 2, column.EmptyText);
            builder.CloseElement();
        }
        else
        {
            builder.AddContent(sequence, value);
        }
    }
    
    private void RenderNumber(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetFormattedValue(item);
        builder.OpenElement(sequence, "span");
        builder.AddAttribute(sequence + 1, "class", "font-mono");
        builder.AddContent(sequence + 2, value);
        builder.CloseElement();
    }
    
    private void RenderCurrency(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        string formatted = column.EmptyText;
        
        if (value != null)
        {
            var format = column.Format ?? "C";
            if (value is IFormattable formattable)
            {
                formatted = formattable.ToString(format, null);
            }
            else
            {
                formatted = $"${value:N2}";
            }
        }
        
        builder.OpenElement(sequence, "span");
        builder.AddAttribute(sequence + 1, "class", "font-mono text-right");
        builder.AddContent(sequence + 2, formatted);
        builder.CloseElement();
    }
    
    private void RenderPercentage(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        string formatted = column.EmptyText;
        
        if (value != null)
        {
            var format = column.Format ?? "P0";
            if (value is IFormattable formattable)
            {
                formatted = formattable.ToString(format, null);
            }
            else if (TryConvertToDouble(value, out var doubleValue))
            {
                formatted = $"{doubleValue * 100:N0}%";
            }
        }
        
        builder.OpenElement(sequence, "span");
        builder.AddAttribute(sequence + 1, "class", "font-mono");
        builder.AddContent(sequence + 2, formatted);
        builder.CloseElement();
    }
    
    private void RenderDate(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        string formatted = column.EmptyText;
        
        if (value is DateTime dt)
            formatted = dt.ToString(column.Format ?? "MMM dd, yyyy");
        else if (value is DateOnly d)
            formatted = d.ToString(column.Format ?? "MMM dd, yyyy");
        
        builder.AddContent(sequence, formatted);
    }
    
    private void RenderDateTime(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        string formatted = column.EmptyText;
        
        if (value is DateTime dt)
            formatted = dt.ToString(column.Format ?? "MMM dd, yyyy HH:mm");
        
        builder.AddContent(sequence, formatted);
    }
    
    private void RenderTime(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        string formatted = column.EmptyText;
        
        if (value is DateTime dt)
            formatted = dt.ToString(column.Format ?? "HH:mm");
        else if (value is TimeOnly t)
            formatted = t.ToString(column.Format ?? "HH:mm");
        
        builder.AddContent(sequence, formatted);
    }
    
    private void RenderBoolean(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        var isTrue = value is bool b && b;
        
        builder.OpenElement(sequence, "span");
        builder.AddAttribute(sequence + 1, "class", 
            isTrue ? "badge-success" : "badge-secondary");
        
        builder.OpenElement(sequence + 2, "i");
        builder.AddAttribute(sequence + 3, "class", "icon text-sm mr-1");
        builder.AddContent(sequence + 4, isTrue ? "check" : "close");
        builder.CloseElement();
        
        builder.AddContent(sequence + 5, isTrue ? "Yes" : "No");
        builder.CloseElement();
    }
    
    private void RenderStatus(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        var text = value?.ToString() ?? column.EmptyText;
        var (variant, icon) = GetStatusInfo(text);
        
        builder.OpenElement(sequence, "span");
        builder.AddAttribute(sequence + 1, "class", $"badge-{variant}");
        
        if (!string.IsNullOrEmpty(icon))
        {
            builder.OpenElement(sequence + 2, "i");
            builder.AddAttribute(sequence + 3, "class", "icon text-sm mr-1");
            builder.AddContent(sequence + 4, icon);
            builder.CloseElement();
        }
        
        builder.AddContent(sequence + 5, text);
        builder.CloseElement();
    }
    
    private void RenderProgress(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        var progress = 0;
        
        if (TryConvertToDouble(value, out var doubleValue))
        {
            progress = (int)Math.Round(doubleValue * 100);
        }
        
        progress = Math.Max(0, Math.Min(100, progress));
        
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "d-flex items-center gap-2");
        
        // Progress bar
        builder.OpenElement(sequence + 2, "div");
        builder.AddAttribute(sequence + 3, "class", "flex-1 h-2 bg-surface-secondary rounded-full overflow-hidden");
        builder.OpenElement(sequence + 4, "div");
        builder.AddAttribute(sequence + 5, "class", "h-full bg-primary transition-all");
        builder.AddAttribute(sequence + 6, "style", $"width: {progress}%");
        builder.CloseElement();
        builder.CloseElement();
        
        // Value
        builder.OpenElement(sequence + 7, "span");
        builder.AddAttribute(sequence + 8, "class", "text-sm font-mono");
        builder.AddContent(sequence + 9, $"{progress}%");
        builder.CloseElement();
        
        builder.CloseElement();
    }
    
    private void RenderRating(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        var rating = 0;
        
        if (TryConvertToDouble(value, out var doubleValue))
        {
            rating = (int)Math.Round(doubleValue);
        }
        
        rating = Math.Max(0, Math.Min(5, rating));
        
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "d-flex items-center gap-1");
        
        for (int i = 1; i <= 5; i++)
        {
            builder.OpenElement(sequence + i * 10, "i");
            builder.AddAttribute(sequence + i * 10 + 1, "class", 
                $"icon text-sm {(i <= rating ? "text-warning" : "text-muted")}");
            builder.AddContent(sequence + i * 10 + 2, i <= rating ? "star" : "star_outline");
            builder.CloseElement();
        }
        
        builder.CloseElement();
    }
    
    private void RenderEmail(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        var email = value?.ToString();
        
        if (string.IsNullOrEmpty(email))
        {
            builder.AddContent(sequence, column.EmptyText);
            return;
        }
        
        builder.OpenElement(sequence, "a");
        builder.AddAttribute(sequence + 1, "href", $"mailto:{email}");
        builder.AddAttribute(sequence + 2, "class", "text-primary hover:underline");
        builder.AddContent(sequence + 3, email);
        builder.CloseElement();
    }
    
    private void RenderPhone(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        var phone = value?.ToString();
        
        if (string.IsNullOrEmpty(phone))
        {
            builder.AddContent(sequence, column.EmptyText);
            return;
        }
        
        builder.OpenElement(sequence, "a");
        builder.AddAttribute(sequence + 1, "href", $"tel:{phone}");
        builder.AddAttribute(sequence + 2, "class", "text-primary hover:underline");
        builder.AddContent(sequence + 3, phone);
        builder.CloseElement();
    }
    
    private void RenderLink(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        var url = value?.ToString();
        
        if (string.IsNullOrEmpty(url))
        {
            builder.AddContent(sequence, column.EmptyText);
            return;
        }
        
        builder.OpenElement(sequence, "a");
        builder.AddAttribute(sequence + 1, "href", url);
        builder.AddAttribute(sequence + 2, "target", "_blank");
        builder.AddAttribute(sequence + 3, "rel", "noopener noreferrer");
        builder.AddAttribute(sequence + 4, "class", "text-primary hover:underline d-inline-flex items-center gap-1");
        builder.AddContent(sequence + 5, GetDisplayUrl(url));
        
        builder.OpenElement(sequence + 6, "i");
        builder.AddAttribute(sequence + 7, "class", "icon text-sm");
        builder.AddContent(sequence + 8, "open_in_new");
        builder.CloseElement();
        
        builder.CloseElement();
    }
    
    private void RenderImage(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        var imageUrl = value?.ToString();
        
        if (string.IsNullOrEmpty(imageUrl))
        {
            builder.OpenElement(sequence, "div");
            builder.AddAttribute(sequence + 1, "class", "w-12 h-12 bg-surface-secondary rounded d-flex items-center justify-center");
            builder.OpenElement(sequence + 2, "i");
            builder.AddAttribute(sequence + 3, "class", "icon text-muted");
            builder.AddContent(sequence + 4, "image");
            builder.CloseElement();
            builder.CloseElement();
            return;
        }
        
        builder.OpenElement(sequence, "img");
        builder.AddAttribute(sequence + 1, "src", imageUrl);
        builder.AddAttribute(sequence + 2, "alt", "");
        builder.AddAttribute(sequence + 3, "class", "w-12 h-12 object-cover rounded");
        builder.AddAttribute(sequence + 4, "loading", "lazy");
        builder.CloseElement();
    }
    
    private void RenderAvatar(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        var text = value?.ToString() ?? "";
        var initials = GetInitials(text);
        
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "w-10 h-10 rounded-full bg-primary text-primary-contrast d-flex items-center justify-center font-medium text-sm");
        builder.AddContent(sequence + 2, initials);
        builder.CloseElement();
    }
    
    private void RenderTags(RenderTreeBuilder builder, TItem item, int sequence)
    {
        var value = column.GetValue(item);
        var tags = new List<string>();
        
        if (value is IEnumerable<string> stringList)
        {
            tags = stringList.ToList();
        }
        else if (value is string str && !string.IsNullOrEmpty(str))
        {
            tags = str.Split(',').Select(t => t.Trim()).ToList();
        }
        
        if (!tags.Any())
        {
            builder.AddContent(sequence, column.EmptyText);
            return;
        }
        
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "d-flex items-center gap-1 flex-wrap");
        
        var index = sequence + 2;
        foreach (var tag in tags.Take(3))
        {
            builder.OpenElement(index++, "span");
            builder.AddAttribute(index++, "class", "badge-secondary badge-sm");
            builder.AddContent(index++, tag);
            builder.CloseElement();
        }
        
        if (tags.Count > 3)
        {
            builder.OpenElement(index++, "span");
            builder.AddAttribute(index++, "class", "text-sm text-muted");
            builder.AddContent(index++, $"+{tags.Count - 3} more");
            builder.CloseElement();
        }
        
        builder.CloseElement();
    }
    
    private void RenderActions(RenderTreeBuilder builder, TItem item, int sequence)
    {
        // Check if column has ActionsTemplate configured
        if (column.ActionsTemplate != null)
        {
            // Use the configured ActionsTemplate
            var fragment = column.ActionsTemplate.Render(item);
            fragment(builder);
            return;
        }
        
        // Default implementation with standard actions
        var defaultTemplate = CreateDefaultActionsTemplate();
        var fragment2 = defaultTemplate.Render(item);
        fragment2(builder);
    }
    
    private ActionsTemplate<TItem> CreateDefaultActionsTemplate()
    {
        var template = new ActionsTemplate<TItem>
        {
            DisplayStyle = ActionsDisplayStyle.Inline,
            Size = RR.Blazor.Enums.SizeType.Small,
            Density = RR.Blazor.Enums.DensityType.Compact,
            ShowTooltips = true,
            Actions = new List<ActionButton<TItem>>
            {
                new ActionButton<TItem>
                {
                    Id = "view",
                    Text = "View",
                    Icon = "visibility",
                    Style = ButtonStyle.Ghost,
                    Variant = RR.Blazor.Enums.VariantType.Secondary,
                    IconOnly = true
                },
                new ActionButton<TItem>
                {
                    Id = "edit",
                    Text = "Edit",
                    Icon = "edit",
                    Style = ButtonStyle.Ghost,
                    Variant = RR.Blazor.Enums.VariantType.Secondary,
                    IconOnly = true
                },
                new ActionButton<TItem>
                {
                    Id = "delete",
                    Text = "Delete",
                    Icon = "delete",
                    Style = ButtonStyle.Ghost,
                    Variant = RR.Blazor.Enums.VariantType.Danger,
                    IconOnly = true,
                    RequiresConfirmation = true,
                    ConfirmationMessage = "Are you sure you want to delete this item?"
                }
            }
        };
        
        return template;
    }
    
    #endregion
    
    #region Helper Methods
    
    private static MemberExpression GetMemberExpression(Expression expression)
    {
        if (expression is MemberExpression memberExpr)
            return memberExpr;
            
        if (expression is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert)
            return GetMemberExpression(unaryExpr.Operand);
            
        return null;
    }
    
    private static bool TryConvertToDouble(object value, out double result)
    {
        result = 0;
        if (value == null) return false;
        
        return value switch
        {
            double d => (result = d) == d,
            float f => (result = f) == f,
            decimal dec => (result = (double)dec) == (double)dec,
            int i => (result = i) == i,
            long l => (result = l) == l,
            _ => double.TryParse(value.ToString(), out result)
        };
    }
    
    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "?";
            
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        return name[0].ToString().ToUpper();
    }
    
    private static string GetDisplayUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri.Host;
        }
        return url.Length > 30 ? url.Substring(0, 30) + "..." : url;
    }
    
    private static (string variant, string icon) GetStatusInfo(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "active" or "enabled" or "success" or "completed" or "approved" => 
                ("success", "check_circle"),
            "inactive" or "disabled" or "failed" or "error" or "rejected" => 
                ("danger", "cancel"),
            "pending" or "processing" or "in progress" or "awaiting" => 
                ("warning", "schedule"),
            "draft" or "new" or "created" => 
                ("info", "edit_note"),
            "cancelled" or "canceled" => 
                ("secondary", "block"),
            _ => ("secondary", "circle")
        };
    }
    
    #endregion
}