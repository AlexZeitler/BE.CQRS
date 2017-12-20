﻿using BE.CQRS.Data.MongoDb.Streams;
using BE.CQRS.Domain.Configuration;
using BE.FluentGuard;
using MongoDB.Driver;

namespace BE.CQRS.Data.MongoDb
{
    public static class MongoEventSourceStartup
    {
        public static EventSourceConfiguration SetMongoDomainObjectRepository(this EventSourceConfiguration config, IMongoDatabase db)
        {
            Precondition.For(() => db).NotNull();
            Precondition.For(() => config).NotNull();
            Precondition.For(() => config.Activator).NotNull();

            var repo = new MongoDomainObjectRepository(config.Activator, db, config.ProtectorFactory);

            config.DomainObjectRepository = repo;

            return config;
        }

        public static DenormalizerConfiguration SetMongoEventPositionGateway(this DenormalizerConfiguration config, IMongoDatabase db)
        {
            Precondition.For(() => config).NotNull();
            Precondition.For(() => db).NotNull();

            config.StreamPositionGateway = new MongoStreamPositionGateway(db, null);
            return config;
        }

        public static DenormalizerConfiguration SetMongoDbEventSubscriber(this DenormalizerConfiguration config, IMongoDatabase db)
        {
            Precondition.For(() => config).NotNull();
            Precondition.For(() => db).NotNull();

            config.Subscriber = new MongoEventSubscriber(db, null);
            return config;
        }
    }
}