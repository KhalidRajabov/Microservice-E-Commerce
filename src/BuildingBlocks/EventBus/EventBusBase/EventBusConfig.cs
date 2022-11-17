using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base
{
    public class EventBusConfig
    {
        //sistemə bağlanarkən xəta olduqda neçə dəfə təkrar qoşulmağı yoxlasın.
        public int ConnectionRetryCount { get; set; } = 5;


        //RabbitMQ və ya Azure sorğu zamanı topic adı istənilir, problem olmaması üçün default topic adı veririk
        public string DefaultTopicName { get; set; } = "E-CommerceMicroserviceEventBus";

        //adi (bildiyimiz) connection string
        public string EventBusConnectionString { get; set; } = String.Empty;


        //rabbitMQ və ya Azure`da queue yaradacaq servisimizin adı. Məsələn order, notification və.s.
        //Eventi hansı servisin dinlədiyini göstərir. 
        public string SubscribeClientAppName { get; set; } = String.Empty;


        //Eventi implement edərkən daha oxunaqlı olması üçün adı trim ediləcək.
        public string EventNameProfix { get; set; } = String.Empty;

        public string EventNameSuffix { get; set; } = "Integration Event";


        //Əgər tip adı göndərilməsə default olaraq qoşulacağımız servis RMQ olacaq.
        public EventBusType EventBusType { get; set; } = EventBusType.RabbitMQ;

        public object? Connection { get; set; }

        public bool DeleteEventProfix => !String.IsNullOrEmpty(EventNameProfix);
        public bool DeleteEventSuffix => !String.IsNullOrEmpty(EventNameSuffix);
    }

    public enum EventBusType
    {
        RabbitMQ = 0,
        AzureServiceBus=1
    }
}
