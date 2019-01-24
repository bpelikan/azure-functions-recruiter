using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace NotificationFunctions
{
    public static class CheckAppointmentStatus
    {
        [FunctionName("CheckAppointmentStatus")]
        public async static Task Run([QueueTrigger("checkappointmentstatusqueue", Connection = "CheckAppointmentStatusQueueConnectionString")]string checkAppointmentStatusItem, [Queue("sendappointmentreminderqueue", Connection = "SendAppointmentReminderQueueConnectionString")]CloudQueue sendAppointmentReminderQueue, ILogger log)
        {
            log.LogInformation($"Function CheckAppointmentStatus processed: \n{checkAppointmentStatusItem}");

            var notificationData = JsonConvert.DeserializeObject<AppointmentReminderMessage>(checkAppointmentStatusItem);
            var interviewAppointmentId = notificationData.InterviewAppointmentId;
            var url = Environment.GetEnvironmentVariable("recruiterUrl", EnvironmentVariableTarget.Process) + "api/InterviewAppointment/CheckAppointmentStatus/";

            bool appointmentCheck = false;
            try
            {
                WebRequest request = WebRequest.Create(url + interviewAppointmentId);
                WebResponse response = request.GetResponse();
                appointmentCheck = ((HttpWebResponse)response).StatusCode == HttpStatusCode.OK ? true : false;
                response.Close();
            }
            catch (WebException ex)
            {
                log.LogError($"Message:{ex.Message}\nURL:{url}");
            }

            if (appointmentCheck)
            {
                var queueMessage = new CloudQueueMessage(checkAppointmentStatusItem);
                await sendAppointmentReminderQueue.CreateIfNotExistsAsync();
                await sendAppointmentReminderQueue.AddMessageAsync(queueMessage);
            }
        }

        //[FunctionName("CheckAppointmentStatus")]
        //public async static Task Run(
        //    [QueueTrigger("checkappointmentstatusqueue", Connection = "CheckAppointmentStatusQueueConnectionString")]string myQueueItem,
        //    [Queue("sendappointmentreminderqueue", Connection = "SendAppointmentReminderQueueConnectionString")]CloudQueue outputQueue,
        //    ILogger log)
        //{
        //    log.LogInformation($"C# Queue trigger function CheckAppointmentStatus processed: \n{myQueueItem}");

        //    var data = JsonConvert.DeserializeObject<AppointmentReminderMessage>(myQueueItem);

        //    var url = GetEnvironmentVariable("recruiterUrl") + "api/InterviewAppointment/" + data.InterviewAppointmentId;
        //    log.LogInformation($"\n3:------------------url:{url}------------------");
        //    bool appointmentExist = false;
        //    try
        //    {
        //        WebRequest request = WebRequest.Create(url);
        //        request.Credentials = CredentialCache.DefaultCredentials;
        //        WebResponse response = request.GetResponse();

        //        var statusCode = ((HttpWebResponse)response).StatusDescription;
        //        log.LogInformation($"Status: {statusCode}");

        //        Stream dataStream = response.GetResponseStream();
        //        StreamReader reader = new StreamReader(dataStream);
        //        string responseFromServer = reader.ReadToEnd();
        //        log.LogInformation($"responseFromServer: {responseFromServer}");
        //        appointmentExist = bool.Parse(responseFromServer);

        //        reader.Close();
        //        response.Close();
        //    }
        //    catch (WebException ex)
        //    {
        //        log.LogError($"Message:{ex.Message}" +
        //                        $"\nURL:{url}");
        //    }

        //    var message = JsonConvert.SerializeObject(data);
        //    log.LogInformation($"\nMessage: \n{message}");
        //    if (appointmentExist)
        //    {
        //        log.LogWarning($"\n3:------------------1-output-send------------------");
        //        var queueMessage = new CloudQueueMessage(message);
        //        await outputQueue.CreateIfNotExistsAsync();
        //        await outputQueue.AddMessageAsync(
        //            queueMessage,
        //            timeToLive: null,
        //            initialVisibilityDelay: null,
        //            options: null,
        //            operationContext: null);
        //    }
        //    else
        //    {
        //        log.LogError(   $"\n3:------------------2-discard------------------" +
        //                        $"\n3:------------------Appointment not found------------------");
        //    }
        //}

        //public static string GetEnvironmentVariable(string name)
        //{
        //    return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        //}
    }
}
