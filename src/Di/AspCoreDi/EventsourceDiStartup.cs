﻿using System;
using BE.CQRS.Domain.Configuration;

namespace BE.CQRS.Di.AspCore
{
    public static class EventsourceDiStartup
    {
        public static void SetServiceCollectionActivator(this EventSourceConfiguration config, IServiceProvider serivces)
        {
            var activator = new ServiceCollectionActivator();
            activator.UseProvider(serivces);
            config.Activator = activator;
        }
    }
}