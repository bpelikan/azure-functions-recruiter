using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Shared;

namespace CheckAppointmentStatusFunc
{
    public static class CheckAppointmentStatus
    {
        [FunctionName("CheckAppointmentStatus")]
        public async static Task Run(
            [QueueTrigger("checkappointmentstatusqueue", Connection = "CheckAppointmentStatusQueueConnectionString")]string myQueueItem,
            [Queue("sendappointmentreminderqueue", Connection = "SendAppointmentReminderQueueConnectionString")]CloudQueue outputQueue,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function CheckAppointmentStatus processed: \n{myQueueItem}");

            var data = JsonConvert.DeserializeObject<AppointmentReminderMessage>(myQueueItem);

            var url = GetEnvironmentVariable("recruiterUrl") + "api/InterviewAppointment/" + data.InterviewAppointmentId;
            log.LogInformation($"\n3:------------------url:{url}------------------");
            bool appointmentExist = false;
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Credentials = CredentialCache.DefaultCredentials;
                WebResponse response = request.GetResponse();

                var statusCode = ((HttpWebResponse)response).StatusDescription;
                log.LogInformation($"Status: {statusCode}");

                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                log.LogInformation($"responseFromServer: {responseFromServer}");
                appointmentExist = bool.Parse(responseFromServer);

                reader.Close();
                response.Close();
            }
            catch (WebException ex)
            {
                log.LogError($"Message:{ex.Message}" +
                                $"\nURL:{url}");
            }

            if (appointmentExist)
            {
                log.LogWarning($"\n3:------------------1-output-send------------------");
                var queueMessage = new CloudQueueMessage(myQueueItem);
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
                log.LogError(   $"\n3:------------------2-discard------------------" +
                                $"\n3:------------------Appointment not found------------------");
            }

        }

        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
