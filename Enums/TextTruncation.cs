namespace RR.Blazor.Enums
{
    /// <summary>
    /// Text truncation options for table cells with long content
    /// Provides different strategies for handling overflow text
    /// </summary>
    public enum TextTruncation
    {
        /// <summary>No truncation - allow text to wrap naturally</summary>
        None,
        
        /// <summary>Ellipsis truncation (...) - most common truncation style</summary>
        Ellipsis,
        
        /// <summary>Fade out effect - text gradually fades to transparent</summary>
        Fade,
        
        /// <summary>Word truncation - break at word boundaries</summary>
        Word,
        
        /// <summary>Character truncation - break at any character</summary>
        Character,
        
        /// <summary>Tooltip on hover - show full content in tooltip when truncated</summary>
        Tooltip
    }
}