using System;
using System.Text.Json.Serialization;

namespace RR.Blazor.Models
{
    /// <summary>
    /// Represents a time range with start and end times.
    /// Supports various time formats and automatic conversion.
    /// </summary>
    public struct TimeRange : IEquatable<TimeRange>
    {
        /// <summary>
        /// Start time of the range
        /// </summary>
        public DateTime Start { get; }
        
        /// <summary>
        /// End time of the range
        /// </summary>
        public DateTime End { get; }
        
        /// <summary>
        /// Duration of the time range
        /// </summary>
        [JsonIgnore]
        public TimeSpan Duration => End - Start;
        
        /// <summary>
        /// Creates a new time range
        /// </summary>
        public TimeRange(DateTime start, DateTime end)
        {
            if (end < start)
                throw new ArgumentException("End time must be after start time");
                
            Start = start;
            End = end;
        }
        
        /// <summary>
        /// Creates a time range from Unix timestamps
        /// </summary>
        public static TimeRange FromUnix(long startUnix, long endUnix)
        {
            var start = DateTimeOffset.FromUnixTimeSeconds(startUnix).DateTime;
            var end = DateTimeOffset.FromUnixTimeSeconds(endUnix).DateTime;
            return new TimeRange(start, end);
        }
        
        /// <summary>
        /// Creates a time range from duration
        /// </summary>
        public static TimeRange FromDuration(DateTime start, TimeSpan duration)
        {
            return new TimeRange(start, start.Add(duration));
        }
        
        /// <summary>
        /// Parse time range from string format "HH:mm-HH:mm"
        /// Throws ArgumentException if format is invalid
        /// </summary>
        public static TimeRange Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input cannot be null or whitespace", nameof(input));
                
            var parts = input.Split('-');
            if (parts.Length != 2)
                throw new ArgumentException($"Invalid time range format. Expected 'HH:mm-HH:mm', got '{input}'", nameof(input));
                
            if (!TimeSpan.TryParse(parts[0].Trim(), out var startTime))
                throw new ArgumentException($"Invalid start time format: '{parts[0].Trim()}'", nameof(input));
                
            if (!TimeSpan.TryParse(parts[1].Trim(), out var endTime))
                throw new ArgumentException($"Invalid end time format: '{parts[1].Trim()}'", nameof(input));
                
            var today = DateTime.Today;
            var start = today.Add(startTime);
            var end = today.Add(endTime);
            
            // Handle overnight ranges
            if (end < start)
                end = end.AddDays(1);
                
            return new TimeRange(start, end);
        }
        
        /// <summary>
        /// Try to parse time range from string format "HH:mm-HH:mm"
        /// </summary>
        public static bool TryParse(string input, out TimeRange result)
        {
            try
            {
                result = Parse(input);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
        
        /// <summary>
        /// Convert to Unix timestamp range
        /// </summary>
        public (long start, long end) ToUnix()
        {
            var startUnix = ((DateTimeOffset)Start).ToUnixTimeSeconds();
            var endUnix = ((DateTimeOffset)End).ToUnixTimeSeconds();
            return (startUnix, endUnix);
        }
        
        /// <summary>
        /// Convert to time string format "HH:mm-HH:mm"
        /// </summary>
        public string ToTimeString()
        {
            return $"{Start:HH:mm}-{End:HH:mm}";
        }
        
        /// <summary>
        /// Check if this range contains a specific time
        /// </summary>
        public bool Contains(DateTime time)
        {
            return time >= Start && time <= End;
        }
        
        /// <summary>
        /// Check if this range overlaps with another range
        /// </summary>
        public bool Overlaps(TimeRange other)
        {
            return Start <= other.End && End >= other.Start;
        }
        
        public bool Equals(TimeRange other)
        {
            return Start == other.Start && End == other.End;
        }
        
        public override bool Equals(object? obj)
        {
            return obj is TimeRange other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }
        
        public override string ToString()
        {
            return $"{Start:yyyy-MM-dd HH:mm} - {End:yyyy-MM-dd HH:mm}";
        }
        
        public static bool operator ==(TimeRange left, TimeRange right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(TimeRange left, TimeRange right)
        {
            return !left.Equals(right);
        }
    }
}