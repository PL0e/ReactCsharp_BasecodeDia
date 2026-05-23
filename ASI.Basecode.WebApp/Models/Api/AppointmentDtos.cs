using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ASI.Basecode.WebApp.Models.Api
{
    public class AppointmentResponse
    {
        public int AppointmentId { get; set; }
        public int StudentId { get; set; }
        public int AdviserId { get; set; }
        public int SemesterId { get; set; }
        public string AppointmentType { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public TimeSpan? AppointmentTime { get; set; }
        public string Status { get; set; }
        public string CancellationReason { get; set; }
        public DateTime? CancellationDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string DeleteName { get; set; }
    }

    public class UpsertAppointmentRequest
    {
        public int StudentId { get; set; }
        public int AdviserId { get; set; }
        public int SemesterId { get; set; }
        public string AppointmentType { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public TimeSpan? AppointmentTime { get; set; }
        public string Status { get; set; }
        public string CancellationReason { get; set; }

        [JsonConverter(typeof(NullableFlexibleDateTimeJsonConverter))]
        public DateTime? CancellationDate { get; set; }
    }

    public class AppointmentCalendarItemResponse
    {
        public int AppointmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int AdviserId { get; set; }
        public string AdviserName { get; set; }
        public int SemesterId { get; set; }
        public string AppointmentType { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public TimeSpan? AppointmentTime { get; set; }
        public string Status { get; set; }
        public string CancellationReason { get; set; }
    }

    public class AppointmentCalendarResponse
    {
        public List<AppointmentCalendarItemResponse> UpcomingAppointments { get; set; } = new();
        public List<AppointmentCalendarItemResponse> CompletedAppointments { get; set; } = new();
        public List<AppointmentCalendarItemResponse> CancelledAppointments { get; set; } = new();
    }

    public sealed class NullableFlexibleDateTimeJsonConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (string.IsNullOrWhiteSpace(value))
                {
                    return null;
                }

                return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind, out var dateTime)
                    ? dateTime
                    : null;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value);
                return;
            }

            writer.WriteNullValue();
        }
    }
}
