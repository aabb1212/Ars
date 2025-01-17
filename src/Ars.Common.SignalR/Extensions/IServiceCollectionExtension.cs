﻿using Ars.Commom.Core;
using Ars.Common.SignalR.Caches;
using Ars.Common.SignalR.Hubs;
using Ars.Common.SignalR.Sender;
using Autofac.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Ars.Common.SignalR.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static IArsWebApplicationBuilder AddArsSignalR(
            this IArsWebApplicationBuilder arsServiceBuilder,
            Action<ArsSignalRConfiguration>? action = null,
            Action<HubOptions>? action1 = null) 
        {
            ArsSignalRConfiguration config = new ArsSignalRConfiguration();
            action?.Invoke(config);

            var service = arsServiceBuilder.Services;
            if (config.CacheType == 0)
                service.AddScoped<IHubCacheManager, MemoryHubCacheManager>();
            else
                service.AddScoped<IHubCacheManager, RedisHubCacheManager>();

            service.AddSingleton<IHubSenderProvider, HubSenderProvider>();

            service.AddScoped<IHubSendMessage, ArsWebHub>();
            service.AddScoped<IHubSendMessage, ArsAndroidHub>();

            if (null == action1)
                action1 = r => { };

            if (config.UseMessagePackProtocol)
                service.AddSignalR(action1).AddMessagePackProtocol();
            else
                service.AddSignalR(action1);

            return arsServiceBuilder;
        }
    }
}
