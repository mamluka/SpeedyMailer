using System;

namespace SpeedyMailer.Core.Settings
{
    public class DefaultAttribute : Attribute
    {
        public DefaultAttribute(string david)
        {
            Text = david;
        }

        public string Text { get; set; }
    }
}