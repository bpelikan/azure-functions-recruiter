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
        public async static Task Run(
            [QueueTrigger("checkappointmentstatusqueue", Connection = "CheckAppointmentStatusQueueConnectionString")]string checkAppointmentStatusItem, 
            [Queue("sendappointmentreminderqueue", Connection = "SendAppointmentReminderQueueConnectionString")]CloudQueue sendAppointmentReminderQueue, 
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function CheckAppointmentStatus processed: \n{checkAppointmentStatusItem}");

            var notificationData = JsonConvert.DeserializeObject<AppointmentReminderMessage>(checkAppointmentStatusItem);
            var interviewAppointmentId = notificationData.InterviewAppointmentId;
            var url = GetEnvironmentVariable("recruiterUrl") + "api/InterviewAppointment/CheckAppointmentStatus/";

            bool appointmentCheck = false;
            try
            {
                WebRequest request = WebRequest.Create(url + interviewAppointmentId);
                request.Credentials = CredentialCache.DefaultCredentials;
                WebResponse response = request.GetResponse();

                appointmentCheck = ((HttpWebResponse)response).StatusCode == HttpStatusCode.OK ? true : false;
                response.Close();
            }
            catch (WebException ex)
            {
                log.LogError($"Message:{ex.Message}" +
                                $"\nURL:{url}");
            }

            if (appointmentCheck)
            {
                var queueMessage = new CloudQueueMessage(checkAppointmentStatusItem);
                await sendAppointmentReminderQueue.CreateIfNotExistsAsync();
                await sendAppointmentReminderQueue.AddMessageAsync(queueMessage);
            }
        }

        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
