using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace NotificationFunctions
{
    public static class GenerateAppointmentReminder
    {
        [FunctionName("GenerateAppointmentReminder")]
        public async static Task Run(
            [QueueTrigger("generateappointmentreminderqueue", Connection = "GenerateAppointmentReminderQueueConnectionString")]string generateAppointmentReminderQueueItem, 
            [Queue("processappointmentreminderqueue", Connection = "ProcessAppointmentReminderQueuequeueConnectionString")]CloudQueue processAppointmentReminderQueue, 
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function GenerateAppointmentReminder processed:\n{generateAppointmentReminderQueueItem}");

            var notificationData = JsonConvert.DeserializeObject<AppointmentReminderMessage>(generateAppointmentReminderQueueItem);
            TimeSpan invisibleTime = TimeSpan.FromMinutes(0);
            if (DateTime.UtcNow <= notificationData.NotificationTime)
                if ((notificationData.NotificationTime - DateTime.UtcNow) <= TimeSpan.FromDays(StaticValue.maxInvisibleTime))
                    invisibleTime = notificationData.NotificationTime - DateTime.UtcNow;
                else
                    invisibleTime = TimeSpan.FromDays(StaticValue.maxInvisibleTime);

            await processAppointmentReminderQueue.CreateIfNotExistsAsync();
            var notificationMessage = new CloudQueueMessage(generateAppointmentReminderQueueItem);
            await processAppointmentReminderQueue.AddMessageAsync(
                notificationMessage,
                timeToLive: null,
                initialVisibilityDelay: invisibleTime,
                options: null,
                operationContext: null);
        }
    }
}