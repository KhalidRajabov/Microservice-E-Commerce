using EventBus.Base;
using EventBus.Base.Events;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.AzureServiceBus
{
    public class EventBusServiceBus : BaseEventBus
    {
        private ITopicClient topicClient;
        private ManagementClient managementClient;
        private ILogger logger;

        public EventBusServiceBus(EventBusConfig config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {
            logger = serviceProvider.GetService(typeof(ILogger<EventBusServiceBus>)) as ILogger<EventBusServiceBus>;
            managementClient = new ManagementClient(config.EventBusConnectionString);
            topicClient = createTopicClient();
        }


        private  ITopicClient createTopicClient()
        {
            if (topicClient==null||topicClient.IsClosedOrClosing)
            {
                topicClient = new TopicClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, RetryPolicy.Default);
            }


            //ensure that topic already exists
            if (!managementClient.TopicExistsAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult())
                managementClient.CreateQueueAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult();
            return topicClient;
        }

        public override void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name; //eventin adını verəcək. Məsələn: OrderCreatedİntegrationEvent
            eventName = ProcessEventName(eventName);  //eventin adını daha oxunaqlı edəcək. Məsələn: OrderCreated

            var eventStr = JsonConvert.SerializeObject(@event);
            var bodyArr = Encoding.UTF8.GetBytes(eventStr);
            var message = new Message()
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = bodyArr,
                Label =eventName
            };
            topicClient.SendAsync(message).GetAwaiter().GetResult();
        }

        public override void Subscribe<T, TH>()
        {
            var eventName = typeof(T).Name;
            eventName = ProcessEventName(eventName);

            //Azure servis bus tərəfdə subscription olub olmadığını yoxlayırıq
            if (!SubsManager.HasSubscriptionForEvent(eventName))
            {
                var subscriptionClient = CreateSubscriptionClientIfNotExists(eventName);
                RegisterSubscriptionMessageHandler(subscriptionClient);
            }
            logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);
            SubsManager.AddSubscription<T, TH>();
        }

        public override void UnSubscribe<T, TH>()
        {
            var eventName = typeof(T).Name;
            try
            {
                var subscriptionClient = CreateSubscriptionClient(eventName);
                subscriptionClient.RemoveRuleAsync(eventName).GetAwaiter().GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {
                logger.LogWarning("The messaging entity {EventName} could not be found", eventName);
            }

            logger.LogInformation("Unsubscribing from event {EventName}", eventName);
            SubsManager.RemoveSubscription<T, TH>();
        }

        private void RegisterSubscriptionMessageHandler(ISubscriptionClient subscriptionClient)
        {
            subscriptionClient.RegisterMessageHandler(
                async (message, token) =>
                {
                    var eventName = $"{message.Label}";
                    var messageData = Encoding.UTF8.GetString(message.Body);

                    //Complete the message so that it is not received again
                    if (await ProcessEvent(ProcessEventName(eventName), messageData))
                    {
                        await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                    }
                },
                new MessageHandlerOptions(ExceptionReceivedHandler) { MaxConcurrentCalls=10, AutoComplete = false});
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var ex = exceptionReceivedEventArgs.Exception;
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            logger.LogError(ex, "Error handling message: {ExceptionMessage} - Context: {ExceptionContext}", ex.Message, context);
            return Task.CompletedTask;
        }


        //Subscription yaratmağa çalışırıq
        private ISubscriptionClient CreateSubscriptionClientIfNotExists(string eventName)
        {
            var subClient = CreateSubscriptionClient(eventName);
            var exists = managementClient.SubscriptionExistsAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();
            if (!exists)
            {
                managementClient.CreateSubscriptionAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();
                //əgər yoxdursa və ilk dəfə yaradırıqsa rule`l silirik
                RemoveDefaultRule(subClient);
            }

            //CLient`ın özü ola bilər lakin rul`ı olmaya bilər deyə rul`ı yaradırıq
            CreateRuleIfNotExists(ProcessEventName(eventName), subClient);
            return subClient;

        }

        private void CreateRuleIfNotExists(string eventName, ISubscriptionClient subscriptionClient)
        {
            bool ruleExists;
            try
            {
                var rule = managementClient.GetRuleAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName), eventName).GetAwaiter().GetResult();
                ruleExists = rule == null;
            }
            catch (MessagingEntityNotFoundException)
            {
                //Azure managament cliend does not have a rule exists method
                ruleExists = false;
            }

            if (!ruleExists)
            {
                subscriptionClient.AddRuleAsync(new RuleDescription
                {
                    Filter = new CorrelationFilter { Label = eventName}, 
                    Name = eventName
                }).GetAwaiter().GetResult();
            }
        }

        private void RemoveDefaultRule(SubscriptionClient subscriptionClient)
        {
            try
            {
                subscriptionClient.
                    RemoveRuleAsync(RuleDescription.DefaultRuleName)
                    .GetAwaiter().GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {

                logger.LogWarning("The message entity {DefaultRuleName} could not be found.",RuleDescription.DefaultRuleName);
            }
        }

        private SubscriptionClient CreateSubscriptionClient(string eventName)
        {
            return new SubscriptionClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, GetSubName(eventName));
        }

        public override void Dispose()
        {
            base.Dispose();
            topicClient.CloseAsync().GetAwaiter().GetResult();
            managementClient.CloseAsync().GetAwaiter().GetResult();
            topicClient = null;
            managementClient = null;
        }
    }
}
