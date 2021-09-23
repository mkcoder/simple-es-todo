using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace simple_es_core
{
    public class EventStore
    {
        private static ConcurrentDictionary<string, List<IEvent>> _eventStore = new ConcurrentDictionary<string, List<IEvent>>();

        public ImmutableDictionary<string, List<IEvent>> Store => _eventStore.ToImmutableDictionary();

        public List<IEvent> GetEvents(string key)
        {
            if (_eventStore.ContainsKey(key))
            {
                return _eventStore[key];
            }
            return null;
        }

        public void CreateStream(string key)
        {
            if (!_eventStore.ContainsKey(key))
            {
                _eventStore[key] = new List<IEvent>();
            }
        }

        public void StoreEvent(string key, IEvent @event)
        {
            if (!_eventStore.ContainsKey(key))
            {
                throw new Exception("You must create the stream before you add new Events to it");
            }
            _eventStore[key].Add(@event);
        }

        public void StoreEvents(string key, List<IEvent> events)
        {
            if (_eventStore.ContainsKey(key))
            {
                _eventStore[key].AddRange(events);
            }
            throw new Exception("You must create the stream before you add new Events to it");
        }
    }

    public abstract class Projector<Model> where Model : class, IModel, new()
    {
        private readonly EventStore eventStore;
        protected Model model;

        public Projector(EventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public Model Replay(string stream, Guid aggregateId)
        {
            model = new Model();
            var events = eventStore.GetEvents($"{stream}.{aggregateId}");
            foreach (var evt in events)
            {
                Handle(evt);
            }
            return model;
        }

        private void Handle(IEvent command)
        {
            var mi = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var method in mi)
            {
                var p = method.GetParameters();
                if (p.Any(p => p.ParameterType == command.GetType()))
                {
                    method.Invoke(this, new object[] { command });
                    return;
                }
            }
        }
    }

    public interface IModel { }

    public interface IEvent
    {
        public string EventName { get; set; }
        public Guid Id { get; set; }
    }
}
