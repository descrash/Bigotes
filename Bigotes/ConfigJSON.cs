using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bigotes
{
    /// <summary>
    /// Clase de configuración adaptando el JSON
    /// </summary>
    public struct ConfigJSON
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
    }
}
