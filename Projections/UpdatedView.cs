using Newtonsoft.Json.Linq;

namespace Projections
{
    public class UpdatedView
    {
        public string Name { get; set; }
        public JObject Payload { get; set; }
    }
}