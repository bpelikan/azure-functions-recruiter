using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace NotificationFunctions
{
    public static class ProcessAppointmentReminder
    {
        [FunctionName("ProcessAppointmentReminder")]
        public async static Task Run(
            [QueueTrigger("processappointmentreminderqueue", Connection = "ProcessAppointmentReminderQueuequeueConnectionString")]string processAppointmentReminderItem, 
            [Queue("checkappointmentstatusqueue", Connection = "CheckAppointmentStatusQueueConnectionString")]CloudQueue checkAppointmentStatusQueue,
            [Queue("generateappointmentreminderqueue", Connection = "GenerateAppointmentReminderQueueConnectionString")]CloudQueue generateAppointmentReminderQueue,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function ProcessAppointmentReminder processed:\n{processAppointmentReminderItem}");

            await checkAppointmentStatusQueue.CreateIfNotExistsAsync();
            await generateAppointmentReminderQueue.CreateIfNotExistsAsync();

            var notificationData = JsonConvert.DeserializeObject<AppointmentReminderMessage>(processAppointmentReminderItem);
            var notificationMessage = new CloudQueueMessage(processAppointmentReminderItem);
            TimeSpan invisibleTime = TimeSpan.FromMinutes(0);
            if (DateTime.UtcNow <= notificationData.NotificationTime)
            {
                if ((notificationData.NotificationTime - DateTime.UtcNow) <= TimeSpan.FromDays(StaticValue.maxInvisibleTime))
                {
                    invisibleTime = notificationData.NotificationTime - DateTime.UtcNow;
                    await checkAppointmentStatusQueue.AddMessageAsync(notificationMessage, null, invisibleTime, null, null);
                }
                else
                {
                    await generateAppointmentReminderQueue.AddMessageAsync(notificationMessage);
                }
            }
            else
            {
                if (DateTime.UtcNow < notificationData.StartTime)
                {
                    await checkAppointmentStatusQueue.AddMessageAsync(notificationMessage);
                }
            }
        }
    }
}
