using ASI.Basecode.WebApp.Models.Api;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Services
{
    public sealed class NotificationStreamManager
    {
        private readonly ConcurrentDictionary<Guid, NotificationSubscription> _subscriptions = new();

        public NotificationSubscription Subscribe(NotificationSubscriber subscriber)
        {
            var channel = Channel.CreateUnbounded<NotificationPayload>();
            var subscription = new NotificationSubscription(Guid.NewGuid(), subscriber, channel);
            _subscriptions[subscription.Id] = subscription;
            return subscription;
        }

        public void Unsubscribe(Guid id)
        {
            if (_subscriptions.TryRemove(id, out var subscription))
            {
                subscription.Channel.Writer.TryComplete();
            }
        }

        public async Task PublishAsync(NotificationPayload payload, NotificationAudience audience)
        {
            foreach (var subscription in _subscriptions.Values)
            {
                if (!MatchesAudience(subscription.Subscriber, audience))
                {
                    continue;
                }

                if (!subscription.Channel.Writer.TryWrite(payload))
                {
                    Unsubscribe(subscription.Id);
                }
            }
        }

        private static bool MatchesAudience(NotificationSubscriber subscriber, NotificationAudience audience)
        {
            if (subscriber == null)
            {
                return false;
            }

            if (string.Equals(subscriber.Role, "CHAIRMAN", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!string.Equals(subscriber.Role, "ADVISER", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (audience?.AdviserId.HasValue == true && subscriber.AdviserId.HasValue)
            {
                return subscriber.AdviserId.Value == audience.AdviserId.Value;
            }

            if (audience?.YearLevelId.HasValue == true && subscriber.YearLevelIds != null)
            {
                return subscriber.YearLevelIds.Contains(audience.YearLevelId.Value);
            }

            return audience?.AdviserId.HasValue != true && audience?.YearLevelId.HasValue != true;
        }
    }

    public sealed class NotificationAudience
    {
        public int? AdviserId { get; set; }
        public int? YearLevelId { get; set; }
    }

    public sealed class NotificationSubscriber
    {
        public string Role { get; set; }
        public int? AdviserId { get; set; }
        public HashSet<int> YearLevelIds { get; set; }
    }

    public sealed class NotificationSubscription
    {
        public NotificationSubscription(Guid id, NotificationSubscriber subscriber, Channel<NotificationPayload> channel)
        {
            Id = id;
            Subscriber = subscriber;
            Channel = channel;
        }

        public Guid Id { get; }
        public NotificationSubscriber Subscriber { get; }
        public Channel<NotificationPayload> Channel { get; }
        public ChannelReader<NotificationPayload> Reader => Channel.Reader;
    }
}
