using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Events
{
    public class IntegrationEvent
    {
        [JsonProperty]
        public Guid Id { get; set; }
        [JsonProperty]
        public DateTime CreatedTime { get; private set; }


        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreatedTime = DateTime.Now;
        }


        [JsonConstructor]
        public IntegrationEvent(Guid id, DateTime createdDate)
        {
            Id =id;
            CreatedTime = createdDate;
        }
    }
}
