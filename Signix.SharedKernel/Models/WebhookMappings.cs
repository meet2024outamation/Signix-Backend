using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Models
{
    public class WebhookMapping
    {
        public string Endpoint { get; set; }
        public string HeaderName { get; set; }
        public string SecretKey { get; set; }
    }
    public class WebhookSettings
    {
        public List<WebhookMapping> WebhookMappings { get; set; }
    }
}
