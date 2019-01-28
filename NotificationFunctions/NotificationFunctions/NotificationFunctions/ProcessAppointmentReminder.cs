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
        public async static Task Run([QueueTrigger("processappointmentreminderqueue", Connection = "ProcessAppointmentReminderQueuequeueConnectionString")]string processAppointmentReminderItem, [Queue("checkappointmentstatusqueue", Connection = "CheckAppointmentStatusQueueConnectionString")]CloudQueue checkAppointmentStatusQueue,[Queue("generateappointmentreminderqueue", Connection = "GenerateAppointmentReminderQueueConnectionString")]CloudQueue generateAppointmentReminderQueue,ILogger log)
        {
            log.LogInformation($"Function ProcessAppointmentReminder processed:\n{processAppointmentReminderItem}");

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

        //[FunctionName("ProcessAppointmentReminder")]
        //public async static Task Run(
        //    [QueueTrigger("processappointmentreminderqueue", Connection = "ProcessAppointmentReminderQueuequeueConnectionString")]string myQueueItem,
        //    //[Queue("sendappointmentreminderqueue", Connection = "SendAppointmentReminderQueueConnectionString")]CloudQueue sendQueue,
        //    [Queue("checkappointmentstatusqueue", Connection = "CheckAppointmentStatusQueueConnectionString")]CloudQueue outputQueue,
        //    [Queue("generateappointmentreminderqueue", Connection = "GenerateAppointmentReminderQueueConnectionString")]CloudQueue reprocessQueue,
        //    ILogger log)
        //{
        //    log.LogInformation($"C# Queue trigger function ProcessAppointmentReminder processed:\n{myQueueItem}");

        //    var data = JsonConvert.DeserializeObject<AppointmentReminderMessage>(myQueueItem);
        //    log.LogInformation($"\nEmail:              {data.Email}" +
        //                        $"\nJobPositionName:    {data.JobPositionName}" +
        //                        $"\nNowTime:            {DateTime.UtcNow.ToLocalTime()}" +
        //                        $"\nNotificationTime:   {data.NotificationTime.ToLocalTime()}" +
        //                        $"\nStartTime:          {data.StartTime.ToLocalTime()}");

        //    var queueMessage = new CloudQueueMessage(myQueueItem);
        //    TimeSpan invisibleTime = TimeSpan.FromMinutes(0);
        //    if (DateTime.UtcNow <= data.NotificationTime)
        //    {
        //        if ((data.NotificationTime - DateTime.UtcNow) <= TimeSpan.FromDays(StaticValue.maxInvisibleTime))
        //        {
        //            invisibleTime = data.NotificationTime - DateTime.UtcNow;
        //            log.LogWarning($"\n2:------------------1-output-send-delay------------------" +
        //                                $"\n2:------------------invisibleTime: {invisibleTime}------------------");
        //            await outputQueue.CreateIfNotExistsAsync();
        //            await outputQueue.AddMessageAsync(
        //                queueMessage,
        //                timeToLive: null,
        //                initialVisibilityDelay: invisibleTime,
        //                options: null,
        //                operationContext: null);
        //        }
        //        else
        //        {
        //            //invisibleTime = TimeSpan.FromMinutes(StaticValue.maxInvisibleTimeInMinute);
        //            //log.LogInformation($"\n2:------------------invisibleTime: {invisibleTime}------------------");
        //            log.LogWarning($"\n2:------------------2-reprocess------------------" +
        //                                $"\n2:------------------invisibleTime: null------------------");
        //            await reprocessQueue.CreateIfNotExistsAsync();
        //            await reprocessQueue.AddMessageAsync(
        //                queueMessage,
        //                timeToLive: null,
        //                initialVisibilityDelay: null,
        //                options: null,
        //                operationContext: null);
        //        }
        //    }
        //    else
        //    {
        //        if (DateTime.UtcNow < data.StartTime)
        //        {
        //            log.LogWarning($"\n2:------------------3-output-send-no-delay------------------" +
        //                            $"\n2:------------------invisibleTime: null------------------");
        //            await outputQueue.CreateIfNotExistsAsync();
        //            await outputQueue.AddMessageAsync(
        //                queueMessage,
        //                timeToLive: null,
        //                initialVisibilityDelay: null,
        //                options: null,
        //                operationContext: null);
        //        }
        //        else
        //        {
        //            log.LogError($"\n2:------------------4-discard------------------" +
        //                            $"\n2:------------------StartTime is in past------------------");
        //        }
        //    }

        //}
    }
}
