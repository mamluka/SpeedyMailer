using System;

namespace SpeedyMailer.Core.Settings
{
    public class DefaultAttribute : Attribute
    {
        public DefaultAttribute(string value)
        {
            Value = value;
        }

        public DefaultAttribute(int value)
        {
            Value = value;
        }

        public dynamic Value { get; set; }
    }
}