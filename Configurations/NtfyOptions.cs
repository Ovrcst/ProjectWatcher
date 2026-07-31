using System;
using System.Collections.Generic;
using System.Text;

namespace DAProjectChecker.Configurations
{
    public class NtfyOptions
    {
        public string Server { get; set; } = "https://ntfy.sh";

        public string Topic { get; set; } = string.Empty;
    }
}
