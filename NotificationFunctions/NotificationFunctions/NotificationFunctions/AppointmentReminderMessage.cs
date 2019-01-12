using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationFunctions
{
    public class AppointmentReminderMessage
    {
        public string Email { get; set; }
        public DateTime NotificationTime { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public DateTime EndTime { get; set; }
        public string InterviewAppointmentId { get; set; }
        public string JobPositionName { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}
