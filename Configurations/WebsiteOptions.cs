using System;
using System.Collections.Generic;
using System.Text;

namespace DAProjectChecker.Configurations
{
    public class WebsiteOptions
    {
        public string Url { get; set; } = string.Empty;

        public int CheckIntervalSeconds { get; set; } = 30;

        public int RefreshIntervalMinutes { get; set; } = 5;
    }
}
