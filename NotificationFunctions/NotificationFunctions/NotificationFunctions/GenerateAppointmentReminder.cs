using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace NotificationFunctions
{
    public static class GenerateAppointmentReminder
    {
        private static int maxInvisibleTime = 5;

        [FunctionName("GenerateAppointmentReminder")]
        public async static void Run(
            [QueueTrigger("generateappointmentreminderqueue", Connection = "queueConnectionString")]string myQueueItem,
            [Queue("appointmentreminderprocessqueue", Connection = "queueConnectionString")]CloudQueue outputQueue,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed:\n{myQueueItem}");

            var data = JsonConvert.DeserializeObject<AppointmentReminderMessage>(myQueueItem);
            log.LogInformation( $"\nEmail:              {data.Email}" +
                                $"\nJobPositionName:    {data.JobPositionName}" +
                                $"\nNowTime:            {DateTime.UtcNow.ToLocalTime()}" +
                                $"\nNotificationTime:   {data.NotificationTime.ToLocalTime()}" +
                                $"\nStartTime:          {data.StartTime.ToLocalTime()}");

            TimeSpan invisibleTime = TimeSpan.FromMinutes(0);
            if (DateTime.UtcNow < data.NotificationTime)
            {
                if ((data.NotificationTime - DateTime.UtcNow) < TimeSpan.FromMinutes(maxInvisibleTime))
                {
                    invisibleTime = data.NotificationTime - DateTime.UtcNow;
                    log.LogInformation($"\n------------------1------------------");
                }
                else
                {
                    invisibleTime = TimeSpan.FromMinutes(maxInvisibleTime);
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
