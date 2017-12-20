﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using BE.CQRS.Data.MongoDb.Commits;
using BE.CQRS.Data.MongoDb.Repositories;
using BE.CQRS.Data.MongoDb.Streams;
using BE.CQRS.Domain;
using BE.CQRS.Domain.DataProtection;
using BE.CQRS.Domain.DomainObjects;
using BE.CQRS.Domain.Events;
using BE.CQRS.Domain.Serialization;
using BE.FluentGuard;
using MongoDB.Driver;

namespace BE.CQRS.Data.MongoDb
{
    public sealed class MongoDomainObjectRepository : DomainObjectRepositoryBase
    {
        private readonly MongoCommitRepository repository;
        private readonly StreamNamer namer = new StreamNamer();
        private readonly EventMapper mapper = new EventMapper(new JsonEventSerializer(new EventTypeResolver()));
        private readonly IEventDataProtectorFactory protectorFactory;

        public MongoDomainObjectRepository(IDomainObjectActivator activator, IMongoDatabase db, IEventDataProtectorFactory protectorFactory) : base(activator)
        {
            Precondition.For(db, nameof(db)).NotNull("MongoDatabase has to be set!");

            repository = new MongoCommitRepository(db);
            this.protectorFactory = protectorFactory;
        }

        public Task<long> GetCommitCount()
        {
            return repository.Count();
        }

        protected override Task<long> GetVersion(string streamName)
        {
            string id = namer.IdByStreamName(streamName);
            string type = namer.TypeNameByStreamName(streamName);

            return repository.GetVersion(type, id);
        }

        protected override Task<bool> ExistsStream(string streamName)
        {
            string id = namer.IdByStreamName(streamName);
            string type = namer.TypeNameByStreamName(streamName);

            return repository.Exists(type, id);
        }

        protected override string ResolveStreamName(string id, Type aggregateType)
        {
            return namer.ResolveStreamName(id, aggregateType);
        }

        protected override Task<AppendResult> SaveUncomittedEventsAsync<T>(T domainObject, bool versionCheck)
        {
            return repository.SaveAsync(domainObject, versionCheck, GetProtector());
        }

        protected override IObservable<IEvent> ReadEvents(string streamName, CancellationToken token)
        {
            string id = namer.IdByStreamName(streamName);
            string type = namer.TypeNameByStreamName(streamName);

            IEventDataProtector protector = GetProtector();
            return Observable.Create<IEvent>(async observer =>
            {
                await repository.EnumerateCommits(type, id,
                    x =>
                    {
                        IEnumerable<IEvent> events = mapper.ExtractEvents(x, protector);

                        foreach (IEvent @event in events)
                        {
                            observer.OnNext(@event);
                        }
                    }, observer.OnCompleted);
                return () =>
                {
                };
            });
        }

        private IEventDataProtector GetProtector()
        {
            IEventDataProtector protector = null;

            if (protectorFactory != null)
            {
                protector = protectorFactory.CreateProtector("eventsource");
            }
            return protector;
        }
    }
}