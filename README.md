# azure-functions-recruiter

[![Build Status](https://bpelikan.visualstudio.com/Recruiter-app-functions/_apis/build/status/Recruiter-app-functions-CI?branchName=master)](https://bpelikan.visualstudio.com/Recruiter-app-functions/_build/latest?definitionId=13&branchName=master)


| branch/project|status|
|---------------------------------|---|
| master                          |[![Build Status](https://dev.azure.com/bpelikan/Recruiter-app-functions/_apis/build/status/Recruiter-app-functions-master-CI?branchName=master)](https://dev.azure.com/bpelikan/Recruiter-app-functions/_build/latest?definitionId=13&branchName=master)|
| CheckAppointmentStatusFunc      |[![Build Status](https://dev.azure.com/bpelikan/Recruiter-app-functions/_apis/build/status/Recruiter-app-functions-CheckAppointmentStatusFunc-CI?branchName=master)](https://dev.azure.com/bpelikan/Recruiter-app-functions/_build/latest?definitionId=12&branchName=master)|
| GenerateAppointmentReminderFunc |[![Build Status](https://dev.azure.com/bpelikan/Recruiter-app-functions/_apis/build/status/Recruiter-app-functions-GenerateAppointmentReminderFunc-CI?branchName=master)](https://dev.azure.com/bpelikan/Recruiter-app-functions/_build/latest?definitionId=9&branchName=master)|
| ProcessAppointmentReminderFunc  |[![Build Status](https://dev.azure.com/bpelikan/Recruiter-app-functions/_apis/build/status/Recruiter-app-functions-ProcessAppointmentReminderFunc-CI?branchName=master)](https://dev.azure.com/bpelikan/Recruiter-app-functions/_build/latest?definitionId=8&branchName=master)|
| SendAppointmentReminderFunc     |[![Build Status](https://dev.azure.com/bpelikan/Recruiter-app-functions/_apis/build/status/Recruiter-app-functions-SendAppointmentReminderFunc-CI?branchName=master)](https://dev.azure.com/bpelikan/Recruiter-app-functions/_build/latest?definitionId=10&branchName=master)|


Appointment reminder system that uses Azure Functions, Azure Queues, SendGrid and [Recruiter project](https://github.com/bpelikan/Recruiter "Recruiter project"):

<p align="center">
    <img alt="Architecture" src="https://raw.githubusercontent.com/bpelikan/azure-functions-recruiter/master/azure-functions-recruiter-architecture.jpg" />
</p>

This project uses:
* **Function Apps**, 
* **Storage account**
* **Application Insights**
* **SendGrid**

## Setting up on Azure
What is needed to deploy this project on Azure:

* **Function Apps** with:
  * `Application settings`:
    * `"GenerateAppointmentReminderQueueConnectionString": "{connection_string_to_storage_with_generateappointmentreminderqueue}"`
    * `"ProcessAppointmentReminderQueuequeueConnectionString": "{connection_string_to_storage_with_processappointmentreminderqueuequeue}"`
    * `"CheckAppointmentStatusQueueConnectionString": "{connection_string_to_storage_with_checkappointmentstatusqueue}"`
    * `"SendAppointmentReminderQueueConnectionString": "{connection_string_to_storage_with_sendappointmentreminderqueue}"`
    * `"SendGridApiKey": "{SendGrid_API_Key}"`
    * `"AzureWebJobsSendGridApiKey": "{SendGrid_API_Key}"`
    * `"recruiterUrl": "{primary_blob_service_endpoint}"`
    
* **Storage account**
* **Application Insights**
* **SendGrid account**

## Setting up on local machine

`local.settings.json` file:
```json
{
    "IsEncrypted": false,
    "Values": {
      "AzureWebJobsStorage": "UseDevelopmentStorage=true",
      "FUNCTIONS_WORKER_RUNTIME": "dotnet",
      "GenerateAppointmentReminderQueueConnectionString": "UseDevelopmentStorage=true",
      "ProcessAppointmentReminderQueuequeueConnectionString": "UseDevelopmentStorage=true",
      "CheckAppointmentStatusQueueConnectionString": "UseDevelopmentStorage=true",
      "SendAppointmentReminderQueueConnectionString": "UseDevelopmentStorage=true",
      "SendGridApiKey": "{SendGrid_API_Key}",
      "AzureWebJobsSendGridApiKey": "{SendGrid_API_Key}",
      "recruiterUrl": "https://localhost:44380/"
    }
}
```
