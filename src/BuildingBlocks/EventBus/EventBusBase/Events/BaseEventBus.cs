using EventBus.Base.Abstractions;
using EventBus.Base.SubManagers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EventBus.Base.Events
{
    public abstract class BaseEventBus : IEventBus
    {
        //ProcessEvent metodundakı createScope funksiyası üçün provayderi əldə etmək lazımdı.
        public readonly IServiceProvider ServiceProvider;
        public readonly IEventBusSubscriptionManager SubsManager;
        public EventBusConfig EventBusConfig { get; set; }

        public BaseEventBus(EventBusConfig config, IServiceProvider serviceProvider)
        {
            EventBusConfig = config;
            ServiceProvider=serviceProvider;
            SubsManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);
        }


        //prosesin adını yazarkən lazımsız mesajı çıxarıb GetSubName metodu vasitasi ilə
        //daha oxunaqlı yazacaq. Məsələn "order created integration event" əvəzinə sadəcə "Order created".
        public virtual string ProcessEventName(string eventName)
        {
            if (EventBusConfig.DeleteEventProfix)
                eventName = eventName.TrimStart(EventBusConfig.EventNameProfix.ToArray());

            if (EventBusConfig.DeleteEventSuffix)
                eventName = eventName.TrimEnd(EventBusConfig.EventNameSuffix.ToArray());


            return eventName;
        }


        public virtual string GetSubName(string eventName)
        {
            return $"{EventBusConfig.SubscribeClientAppName}.{ProcessEventName(eventName)}";
        }

        public virtual void Dispose()
        {
            EventBusConfig = null;
            SubsManager.Clear();
        }




        //RMQ və ya Azure tərəfindən bizə göndəriləcək eventin adını və mesajını process edəcək olan metod.
        public async Task<bool> ProcessEvent(string eventName, string Message)
        {

            //gələn adı oxunaqlı olsun deyə lazımsız şeylərini silmək üçün üstdəki metoda göndəririk.
            eventName = ProcessEventName(eventName);

            var processed = false;

            //gələn event dinlənildiyi halda aşağıdakı metod işləyəcək. 
            if (SubsManager.HasSubscriptionForEvent(eventName))
            {
                var subscriptions = SubsManager.GetHandlersForEvent(eventName);
                using(var scope = ServiceProvider.CreateScope())
                {
                    foreach (var subs in subscriptions)
                    {
                        var handler = ServiceProvider.GetService(subs.HandlerType);
                        if (handler == null) continue;
                        var eventType  = SubsManager.GetEventTypeByName($"{EventBusConfig.EventNameProfix}{eventName}{EventBusConfig.EventNameSuffix}");
                        var integrationEvent = JsonConvert.DeserializeObject(Message, eventType);

                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                    }
                }
                processed = true;
            }
            return processed;
        }


        public abstract void Publish(IntegrationEvent @event);

        public abstract void Subscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;

        public abstract void UnSubscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;
    }
}
