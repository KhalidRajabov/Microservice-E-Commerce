using EventBus.Base;
using EventBus.Base.Abstractions;
using EventBus.Factory;
using EventBus.UnitTest.Events.EventHandlers;
using EventBus.UnitTest.Events.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EventBus.UnitTest
{
    [TestClass]
    public class EventBusTests
    {
        private ServiceCollection service;

        public EventBusTests()
        {
            service = new ServiceCollection();
            service.AddLogging(configure=>configure.AddConsole());
        }

        [TestMethod]
        public void subscribe_event_on_rabbitmq_test()
        {
            service.AddSingleton<IEventBus>(sp =>
            {
                EventBusConfig config = new()
                {
                    ConnectionRetryCount=5,
                    SubscribeClientAppName="EventBus.UnitTest",
                    DefaultTopicName = "E-CommerceMicroserviceEventBus",
                    EventBusType= EventBusType.RabbitMQ,
                    EventNameSuffix = "IntegrationEvent",
                    
                };
                return EventBusFactory.Create(config, sp);
            });
            var sp = service.BuildServiceProvider();

            var eventBus = sp.GetRequiredService<IEventBus>();

            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
        }
    }
}