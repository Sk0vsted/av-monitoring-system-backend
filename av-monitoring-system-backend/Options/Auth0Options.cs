using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVMonitoring.Functions.Options
{
    public class Auth0Options
    {
        public string Domain { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}
