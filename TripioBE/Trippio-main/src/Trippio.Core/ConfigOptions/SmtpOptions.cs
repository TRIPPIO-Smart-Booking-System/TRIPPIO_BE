using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trippio.Core.ConfigOptions
{
    public class SmtpOptions
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public string User { get; set; } = "";
        public string Pass { get; set; } = "";        
        public bool UseSsl { get; set; } = false;
        public bool UseStartTls { get; set; } = true;
        public string FromName { get; set; } = "Trippio";
        public string FromEmail { get; set; } = "";
    }
}
