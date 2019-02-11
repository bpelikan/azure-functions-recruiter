using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.IO;

namespace NotificationFunctions
{
    public static class SendAppointmentReminder
    {
        [FunctionName("SendAppointmentReminder")]
        public static void Run(
            [QueueTrigger("sendappointmentreminderqueue", Connection = "SendAppointmentReminderQueueConnectionString")]string sendAppointmentReminderItem, 
            [SendGrid]out SendGridMessage notificationEmail, 
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {sendAppointmentReminderItem}");

            var notificationData = JsonConvert.DeserializeObject<AppointmentReminderMessage>(sendAppointmentReminderItem);

            notificationEmail = new SendGridMessage();
            notificationEmail.AddTo(notificationData.Email);
            notificationEmail.SetFrom(new EmailAddress("no-reply@recruiterbp.azurewebsites.net", "Recruiter"));
            notificationEmail.SetSubject(notificationData.Subject);
            notificationEmail.AddContent("text/html", notificationData.Content);
        }
    }
}


//[FunctionName("SendAppointmentReminder")]
//public static void Run(
//    [QueueTrigger("sendappointmentreminderqueue", Connection = "SendAppointmentReminderQueueConnectionString")]string myQueueItem,
//    //[SendGrid(ApiKey = "SendGridApiKey")] out SendGridMessage email,
//    ILogger log)
//{
//    log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

//    var data = JsonConvert.DeserializeObject<AppointmentReminderMessage>(myQueueItem);
//    //log.LogInformation($"\nEmail:              {data.Email}" +
//    //                    $"\nJobPositionName:    {data.JobPositionName}" +
//    //                    $"\nNowTime:            {DateTime.UtcNow.ToLocalTime()}" +
//    //                    $"\nNotificationTime:   {data.NotificationTime.ToLocalTime()}" +
//    //                    $"\nStartTime:          {data.StartTime.ToLocalTime()}");

//    //log.LogInformation($"Send email to: {data.Email}");

//    log.LogWarning($"\n4:------------------Send email to: {data.Email}------------------");

//    //email = new SendGridMessage();
//    //email = new SendGridMessage();
//    //email.AddTo(data.Email);
//    //email.AddContent("text/html", EmailTemplate("Interview appointment reminder", $"Interview appointment reminder: StartTime: {data.StartTime}"));
//    //email.SetFrom(new EmailAddress("no-reply@recruiterbp.azurewebsites.net", "Recruiter"));
//    //email.SetSubject($"{data.JobPositionName} - Interview appointment reminder");

//    //email = new SendGridMessage();
//    //email.AddTo(data.Email);
//    //email.AddContent("text/html", data.Content);
//    //email.SetFrom(new EmailAddress("no-reply@recruiterbp.azurewebsites.net", "Recruiter"));
//    //email.SetSubject(data.Subject);
//}

//private static string EmailTemplate(string title, string content)
//{
//    return $@"
//            <!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
//            <html xmlns='http://www.w3.org/1999/xhtml'>
//            <head>
//             <meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />
//             <title>Recruiter</title>
//             <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
//            </head>                
//            <body style='margin: 0; padding: 0;'>
//            <table align='center' border='0' cellpadding='0' cellspacing='0' width='600' style='border-collapse: collapse; border: 1px solid #cccccc;'>
//             <tr>
//              <td align='center' bgcolor='#0254E6' style='padding: 0 0 0 0; '>
//               <img src='https://recruiterbpstorage.blob.core.windows.net/static/career-3449422_640.png' alt='Recruiter' width='600' height='300' style='display: block;' />
//              </td>
//             </tr>
//             <tr>
//              <td bgcolor='#ffffff' style='padding: 40px 30px 40px 30px;'>

//                        <table border='0' cellpadding='0' cellspacing='0' width='100%'>
//                         <tr>
//                          <td style='color: #153643; font-family: Arial, sans-serif; font-size: 24px;'>
//                           <b>
//                                        {title}
//                                    </b>
//                          </td>
//                         </tr>
//                         <tr>
//                          <td style='padding: 20px 0 30px 0; color: #153643; font-family: Arial, sans-serif; font-size: 16px; line-height: 20px;'>
//                                    {content}
//                          </td>
//                         </tr>
//                        </table>
//              </td>

//             </tr>
//             <tr>
//              <td bgcolor='#ee4c50' style='padding: 30px 30px 30px 30px;'>
//               <table border='0' cellpadding='0' cellspacing='0' width='100%'>
//                <tr>
//                 <td width='75%' style='color: #ffffff; font-family: Arial, sans-serif; font-size: 14px;'>
//                  &reg; <a href='https://recruiterbp.azurewebsites.net/' style='color: #ffffff;'>Recruiter {DateTime.UtcNow.Year}</a>  

//                  <br/>
//                 </td>
//                 <td align='right'>
//                  <table border='0' cellpadding='0' cellspacing='0'>
//                   <tr>
//                    <td>
//	                    <a href='https://recruiterbp.azurewebsites.net/'>
//		                    <img src='https://recruiterbpstorage.blob.core.windows.net/static/website.png' alt='Recruiter website' width='38' height='38' style='display: block;' border='0' />
//	                    </a>
//                    </td>
//                   </tr>
//                  </table>
//                 </td>
//                </tr>
//               </table>
//              </td>
//             </tr>
//            </table>
//            </body>
//            </html>
//            ";
//}