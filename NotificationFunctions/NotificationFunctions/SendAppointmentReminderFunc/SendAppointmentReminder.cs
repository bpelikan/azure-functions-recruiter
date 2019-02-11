using System;
using System.IO;
using System.Net;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using Shared;

namespace SendAppointmentReminderFunc
{
    public static class SendAppointmentReminder
    {
        [FunctionName("SendAppointmentReminder")]
        public static void Run(
            [QueueTrigger("sendappointmentreminderqueue", Connection = "SendAppointmentReminderQueueConnectionString")]string sendAppointmentReminderItem,
            [SendGrid(ApiKey = "SendGridApiKey")] out SendGridMessage notificationEmail,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {sendAppointmentReminderItem}");

            var notificationData = JsonConvert.DeserializeObject<AppointmentReminderMessage>(sendAppointmentReminderItem);

            notificationEmail = new SendGridMessage();
            notificationEmail.AddTo(notificationData.Email);
            notificationEmail.SetFrom(new EmailAddress("no-reply@recruiterbp.azurewebsites.net", "Recruiter"));
            notificationEmail.SetSubject(notificationData.Subject);
            notificationEmail.AddContent("text/html", notificationData.Content);
        }
    }
}
