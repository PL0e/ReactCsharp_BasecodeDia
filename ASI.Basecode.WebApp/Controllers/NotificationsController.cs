using ASI.Basecode.Data;
using ASI.Basecode.WebApp.Models.Api;
using ASI.Basecode.WebApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("notifications")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NotificationsController : ControllerBase
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
        private readonly AsiBasecodeDBContext _context;
        private readonly NotificationStreamManager _notificationStreamManager;

        public NotificationsController(AsiBasecodeDBContext context, NotificationStreamManager notificationStreamManager)
        {
            _context = context;
            _notificationStreamManager = notificationStreamManager;
        }

        [HttpGet("stream")]
        public async Task Stream(CancellationToken cancellationToken)
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");
            Response.Headers.Append("X-Accel-Buffering", "no");

            var subscriber = await BuildSubscriberAsync();
            var subscription = _notificationStreamManager.Subscribe(subscriber);

            try
            {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
                var tickTask = timer.WaitForNextTickAsync(cancellationToken).AsTask();

                while (!cancellationToken.IsCancellationRequested)
                {
                    var readTask = subscription.Reader.ReadAsync(cancellationToken).AsTask();
                    var completed = await Task.WhenAny(readTask, tickTask);

                    if (completed == tickTask)
                    {
                        if (!await tickTask)
                        {
                            break;
                        }

                        await WriteCommentAsync("heartbeat", cancellationToken);
                        tickTask = timer.WaitForNextTickAsync(cancellationToken).AsTask();
                        continue;
                    }

                    var payload = await readTask;
                    await WriteEventAsync("notification", payload, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _notificationStreamManager.Unsubscribe(subscription.Id);
            }
        }

        private async Task<NotificationSubscriber> BuildSubscriberAsync()
        {
            var role = User?.FindFirst(ClaimTypes.Role)?.Value?.ToUpperInvariant();
            var username = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User?.Identity?.Name
                           ?? User?.FindFirst("UserName")?.Value;

            var subscriber = new NotificationSubscriber
            {
                Role = role
            };

            if (string.IsNullOrWhiteSpace(username))
            {
                return subscriber;
            }

            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == username && x.IsActive);
            if (user == null)
            {
                return subscriber;
            }

            var adviser = await _context.Advisers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == user.Id && !x.IsDeleted);
            if (adviser == null)
            {
                return subscriber;
            }

            subscriber.AdviserId = adviser.Id;
            subscriber.YearLevelIds = new HashSet<int>(await _context.AdviserAssignments.AsNoTracking()
                .Where(x => x.AdviserId == adviser.Id && !x.IsDeleted)
                .Select(x => x.YearLevelId)
                .Distinct()
                .ToListAsync());

            return subscriber;
        }

        private async Task WriteEventAsync(string eventName, NotificationPayload payload, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(payload, SerializerOptions);
            await Response.WriteAsync($"event: {eventName}\n", cancellationToken);
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        private async Task WriteCommentAsync(string comment, CancellationToken cancellationToken)
        {
            await Response.WriteAsync($": {comment}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}
