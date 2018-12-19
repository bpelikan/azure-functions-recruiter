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
            [QueueTrigger("generateappointmentreminderqueue", Connection = "queueConnectionString")]string myQueueItem,
            [Queue("processappointmentreminderqueue", Connection = "queueConnectionString")]CloudQueue outputQueue,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function GenerateAppointmentReminder processed:\n{myQueueItem}");

            var data = JsonConvert.DeserializeObject<AppointmentReminderMessage>(myQueueItem);
            log.LogInformation( $"\nEmail:              {data.Email}" +
                                $"\nJobPositionName:    {data.JobPositionName}" +
                                $"\nNowTime:            {DateTime.UtcNow.ToLocalTime()}" +
                                $"\nNotificationTime:   {data.NotificationTime.ToLocalTime()}" +
                                $"\nStartTime:          {data.StartTime.ToLocalTime()}");

            TimeSpan invisibleTime = TimeSpan.FromMinutes(0);
            if (DateTime.UtcNow < data.NotificationTime)
            {
                if ((data.NotificationTime - DateTime.UtcNow) < TimeSpan.FromMinutes(StaticValue.maxInvisibleTimeInMinute))
                {
                    invisibleTime = data.NotificationTime - DateTime.UtcNow;
                    log.LogInformation($"\n------------------1------------------");
                }
                else
                {
                    invisibleTime = TimeSpan.FromMinutes(StaticValue.maxInvisibleTimeInMinute);
                    log.LogInformation($"\n------------------2------------------");
                }
            }
            else
            {
                log.LogInformation($"\n------------------3------------------");
            }

            log.LogInformation($"\n------------------invisibleTime: {invisibleTime}------------------");
            await outputQueue.CreateIfNotExistsAsync();
            var queueMessage = new CloudQueueMessage(myQueueItem);
            await outputQueue.AddMessageAsync(
                queueMessage,
                timeToLive: null,
                initialVisibilityDelay: invisibleTime,
                options: null,
                operationContext: null);

            //dynamic data = JsonConvert.DeserializeObject(myQueueItem);
            //var data = JsonConvert.DeserializeObject<AppointmentReminderMessage>(myQueueItem);
            //log.LogInformation($@"NotificationTime: {data.NotificationTime}/n
            //                      StartTime: {data.StartTime}");
        }
    }
}
