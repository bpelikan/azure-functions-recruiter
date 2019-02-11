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


//[FunctionName("GenerateAppointmentReminder")]
//public async static Task Run(
//    [QueueTrigger("generateappointmentreminderqueue", Connection = "GenerateAppointmentReminderQueueConnectionString")]string myQueueItem,
//    [Queue("processappointmentreminderqueue", Connection = "ProcessAppointmentReminderQueuequeueConnectionString")]CloudQueue outputQueue,
//    ILogger log)
//{
//    log.LogInformation($"C# Queue trigger function GenerateAppointmentReminder processed:\n{myQueueItem}");

//    var data = JsonConvert.DeserializeObject<AppointmentReminderMessage>(myQueueItem);
//    log.LogInformation( $"\nEmail:              {data.Email}" +
//                        $"\nJobPositionName:    {data.JobPositionName}" +
//                        $"\nNowTime:            {DateTime.UtcNow.ToLocalTime()}" +
//                        $"\nNotificationTime:   {data.NotificationTime.ToLocalTime()}" +
//                        $"\nStartTime:          {data.StartTime.ToLocalTime()}");

//    TimeSpan invisibleTime = TimeSpan.FromMinutes(0);
//    if (DateTime.UtcNow <= data.NotificationTime)
//    {
//        if ((data.NotificationTime - DateTime.UtcNow) <= TimeSpan.FromDays(StaticValue.maxInvisibleTime))
//        {
//            invisibleTime = data.NotificationTime - DateTime.UtcNow;
//            log.LogWarning(     $"\n1:------------------1------------------" +
//                                $"\n1:------------------invisibleTime: {invisibleTime}------------------");
//        }
//        else
//        {
//            invisibleTime = TimeSpan.FromDays(StaticValue.maxInvisibleTime);
//            log.LogWarning(     $"\n1:------------------2------------------" +
//                                $"\n1:------------------invisibleTime: {invisibleTime}------------------");
//        }
//    }   
//    else
//    {
//        log.LogWarning( $"\n1:------------------3-------------------" +
//                        $"\n1:------------------invisibleTime: {invisibleTime}------------------");
//    }

//    await outputQueue.CreateIfNotExistsAsync();
//    var queueMessage = new CloudQueueMessage(myQueueItem);
//    await outputQueue.AddMessageAsync(
//        queueMessage,
//        timeToLive: null,
//        initialVisibilityDelay: invisibleTime,
//        options: null,
//        operationContext: null);

//    //dynamic data = JsonConvert.DeserializeObject(myQueueItem);
//    //var data = JsonConvert.DeserializeObject<AppointmentReminderMessage>(myQueueItem);
//    //log.LogInformation($@"NotificationTime: {data.NotificationTime}/n
//    //                      StartTime: {data.StartTime}");
//}
