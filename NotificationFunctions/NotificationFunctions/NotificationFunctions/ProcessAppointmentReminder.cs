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
            [QueueTrigger("processappointmentreminderqueue", Connection = "queueConnectionString")]string myQueueItem,
            [Queue("sendappointmentreminderqueue", Connection = "queueConnectionString")]CloudQueue outputQueue,
            [Queue("generateappointmentreminderqueue", Connection = "queueConnectionString")]CloudQueue reprocessQueue,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function ProcessAppointmentReminder processed:\n{myQueueItem}");

            var data = JsonConvert.DeserializeObject<AppointmentReminderMessage>(myQueueItem);
            log.LogInformation($"\nEmail:              {data.Email}" +
                                $"\nJobPositionName:    {data.JobPositionName}" +
                                $"\nNowTime:            {DateTime.UtcNow.ToLocalTime()}" +
                                $"\nNotificationTime:   {data.NotificationTime.ToLocalTime()}" +
                                $"\nStartTime:          {data.StartTime.ToLocalTime()}");

            var queueMessage = new CloudQueueMessage(myQueueItem);
            TimeSpan invisibleTime = TimeSpan.FromMinutes(0);
            if (DateTime.UtcNow < data.NotificationTime)
            {
                if ((data.NotificationTime - DateTime.UtcNow) < TimeSpan.FromMinutes(StaticValue.maxInvisibleTimeInMinute))
                {
                    invisibleTime = data.NotificationTime - DateTime.UtcNow;
                    log.LogInformation($"\n------------------1-output-send-delay------------------");
                    log.LogInformation($"\n------------------invisibleTime: {invisibleTime}------------------");
                    await outputQueue.CreateIfNotExistsAsync();
                    await outputQueue.AddMessageAsync(
                        queueMessage,
                        timeToLive: null,
                        initialVisibilityDelay: invisibleTime,
                        options: null,
                        operationContext: null);
                }
                else
                {
                    invisibleTime = TimeSpan.FromMinutes(StaticValue.maxInvisibleTimeInMinute);
                    log.LogInformation($"\n------------------2-reprocess------------------");
                    log.LogInformation($"\n------------------invisibleTime: {invisibleTime}------------------");
                    await reprocessQueue.CreateIfNotExistsAsync();
                    await reprocessQueue.AddMessageAsync(
                        queueMessage,
                        timeToLive: null,
                        initialVisibilityDelay: invisibleTime,
                        options: null,
                        operationContext: null);
                }
            }
            else
            {
                if (DateTime.UtcNow < data.StartTime)
                {
                    log.LogInformation($"\n------------------3-output-send-no-delay------------------");
                    log.LogInformation($"\n------------------invisibleTime: null------------------");
                    await outputQueue.CreateIfNotExistsAsync();
                    await outputQueue.AddMessageAsync(
                        queueMessage,
                        timeToLive: null,
                        initialVisibilityDelay: null,
                        options: null,
                        operationContext: null);
                }
                else
                {
                    log.LogInformation($"\n------------------4-discard------------------");
                    log.LogError($"\n------------------StartTime is in past------------------");
                }
            }

        }
    }
}
