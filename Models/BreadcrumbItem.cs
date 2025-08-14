namespace RR.Blazor.Models
{
    /// <summary>
    /// Represents a single breadcrumb item
    /// </summary>
    public class BreadcrumbItem
    {
        /// <summary>Display text for the breadcrumb</summary>
        public string Text { get; set; } = "";
        
        /// <summary>URL for the breadcrumb link (null for current page)</summary>
        public string Href { get; set; } = "";
        
        /// <summary>Optional icon to display before text</summary>
        public string Icon { get; set; } = "";
        
        /// <summary>Whether this item is disabled</summary>
        public bool IsDisabled { get; set; }
        
        /// <summary>Additional data for custom rendering</summary>
        public object Data { get; set; }

        public BreadcrumbItem() { }

        public BreadcrumbItem(string text, string href = "", string icon = "", bool isDisabled = false)
        {
            Text = text;
            Href = href;
            Icon = icon;
            IsDisabled = isDisabled;
        }
    }
}