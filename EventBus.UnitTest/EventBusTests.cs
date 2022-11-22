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
                return EventBusFactory.Create(GetRabbitMQConfig(), sp);
            });
            var sp = service.BuildServiceProvider();

            var eventBus = sp.GetRequiredService<IEventBus>();

            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            //eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
        }



        //This DOES work, but I am not gonna use Azure Service Bus, but only RabbitMQ
        [TestMethod]
        public void subscribe_event_on_azure_test()
        {
            service.AddSingleton<IEventBus>(sp =>
            {
                return EventBusFactory.Create(GetAzureConfig(), sp);
            });
            var sp = service.BuildServiceProvider();

            var eventBus = sp.GetRequiredService<IEventBus>();

            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
        }

        [TestMethod]
        public void send_message_to_rabbitmq_test()
        {
            service.AddSingleton<IEventBus>(sp =>
            {
                return EventBusFactory.Create(GetRabbitMQConfig(), sp);
            });
            var sp = service.BuildServiceProvider();
            var eventBus = sp.GetRequiredService<IEventBus>();
            eventBus.Publish(new OrderCreatedIntegrationEvent(1));
        }

        [TestMethod]
        public void send_message_to_azure_test()
        {
            service.AddSingleton<IEventBus>(sp =>
            {
                return EventBusFactory.Create(GetAzureConfig(), sp);
            });
            var sp = service.BuildServiceProvider();
            var eventBus = sp.GetRequiredService<IEventBus>();
            eventBus.Publish(new OrderCreatedIntegrationEvent(1));
        }


        private EventBusConfig GetAzureConfig()
        {
            return new EventBusConfig()
            {
                ConnectionRetryCount = 5,
                SubscribeClientAppName = "EventBus.UnitTest",
                DefaultTopicName = "E-CommerceMicroserviceEventBus",
                EventBusType = EventBusType.AzureServiceBus,
                EventNameSuffix = "IntegrationEvent",
                EventBusConnectionString = ""
            };
        }

        private EventBusConfig GetRabbitMQConfig()
        {
            return new EventBusConfig()
            {
                ConnectionRetryCount = 5,
                SubscribeClientAppName = "EventBus.UnitTest",
                DefaultTopicName = "E-CommerceMicroserviceEventBus",
                EventBusType = EventBusType.RabbitMQ,
                EventNameSuffix = "IntegrationEvent",
                /*Connection = new ConnectionFactory()
                {
                    HostName= "localhost",
                    Port=15672,
                    UserName="guest",
                    Password="guest"
                }*/
            };
        }
    }
}