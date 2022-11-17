using EventBus.AzureServiceBus;
using EventBus.Base;
using EventBus.Base.Abstractions;
using EventBus.RabbitMQ;

namespace EventBus.Factory
{
    //bu klass hem rabbitmq hem de azure servis terefine gede bilmelidi
    public static class EventBusFactory
    {
        public static IEventBus Create(EventBusConfig config, IServiceProvider serviceProvider)
        {
            //switch case'in fərqli növü. Normal switch case'dən fərqi yoxdu. Caselər əvəzinə ", _ " yazaraq ayrılır
            return config.EventBusType switch
            {
                EventBusType.AzureServiceBus => new EventBusServiceBus(config, serviceProvider), _ => new EventBusRabbitMQ(config,serviceProvider)
            };
        }
    }
}
