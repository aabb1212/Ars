﻿using Ars.Commom.Core;
using Ars.Common.Redis.CacheConfiguration;
using Ars.Common.Redis.RedisCache;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ars.Common.Redis
{
    public static class IServiceCollectionExtension
    {
        public static IArsServiceBuilder AddArsRedis(this IArsServiceBuilder arsServiceBuilder,Action<ArsCacheOption> action) 
        {
            var service = arsServiceBuilder.Services.ServiceCollection;
            ArsCacheOption arsCacheOption = new ArsCacheOption();
            action?.Invoke(arsCacheOption);
            if (string.IsNullOrEmpty(arsCacheOption.RedisConnection))
                throw new ArgumentNullException(nameof(arsCacheOption.RedisConnection));
            service.AddSingleton<IOptions<ArsCacheOption>>(new OptionsWrapper<ArsCacheOption>(arsCacheOption));

            service.AddSingleton<IArsCacheProvider, ArsRedisCacheProvider>();
            return arsServiceBuilder;
        }

        public static IApplicationBuilder UseArsRedis(this IApplicationBuilder applicationBuilder, 
            Action<ICacheConfigurationProvider>? config = null)
        {
            var provider = applicationBuilder.ApplicationServices;
            if (null != config)
            {
                config.Invoke(provider.GetRequiredService<ICacheConfigurationProvider>());
            }

            var arsCacheOption = provider.GetRequiredService<IOptions<ArsCacheOption>>().Value;
            var csredis = new CSRedis.CSRedisClient($"{arsCacheOption.RedisConnection},defaultDatabase={arsCacheOption.DefaultDB},idleTimeout={arsCacheOption.IdleTimeout},poolsize={arsCacheOption.Poolsize}");
            RedisHelper.Initialization(csredis);

            return applicationBuilder;
        }
    }
}
