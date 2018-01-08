using System;
using System.Diagnostics;

namespace LoggingModule
{
    public enum SeverityEnum
    {
        Critical,
        Error,
        Warn,
        Info,
        Debug
    }
    
    public class Severity : IComparable<Severity>
    {
        public Severity(SeverityEnum enumValue)
        {
            Value = (int) enumValue;
        }
       
        public int Value { get; }

        public static Severity Debug => new Severity(SeverityEnum.Debug);
        public static Severity Info => new Severity(SeverityEnum.Info);
        public static Severity Warn => new Severity(SeverityEnum.Warn);
        public static Severity Error => new Severity(SeverityEnum.Error);
        public static Severity Critical => new Severity(SeverityEnum.Critical);

        public int CompareTo(Severity other)
        {
            return this.Value.CompareTo(other.Value);
        }
    }

}