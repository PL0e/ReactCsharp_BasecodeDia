using System;

namespace ASI.Basecode.WebApp.Models.Api
{
    public class NotificationActionResponse
    {
        public string Kind { get; set; }
        public string AppointmentId { get; set; }
        public string AppointmentDate { get; set; }
        public string AppointmentTime { get; set; }
    }

    public class NotificationPayload
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public NotificationActionResponse Action { get; set; }
    }
}
