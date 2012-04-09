using System;

namespace SpeedyMailer.Settings
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