using System;
using Newtonsoft.Json;

namespace Products.Domain
{
    public class ProductSnapshot
    {
        internal ProductSnapshot(Guid id, string name, string clientId)
        {
            Id = id;
            Name = name;
            ClientId = clientId;
        }

        [JsonConstructor]
        private ProductSnapshot()
        {
        }

        [JsonProperty("id")]
        public Guid Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("clientId")]
        public string ClientId { get; private set; }
    }
}

