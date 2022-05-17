using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Sc
{
    public static class HttpTriggerJSONFormat
    {
        [FunctionName("HttpTriggerJSONFormat")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");



            var fname = "file.json";
            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            fname = data?.name;

            // Setup the connection to the storage account
            var accountName = "";
            var keyValue = "";
            var useHttps = true;

            //
            var storageCredentials = new StorageCredentials(accountName, keyValue);
            var storageAccount = new CloudStorageAccount(storageCredentials, useHttps);


            // Connect to the blob storage
            CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();
            // Connect to the blob container
            CloudBlobContainer container = serviceClient.GetContainerReference("scxconnectdata");
            // Connect to the blob file
            CloudBlockBlob blob = container.GetBlockBlobReference(fname);
            // Get the blob file as text
            string contents = blob.DownloadTextAsync().Result;

            string out_cont = Regex.Replace(contents, "@odata.", "");
            blob.Properties.ContentType = "application/json";

            await blob.UploadTextAsync(out_cont);


            dynamic json = JsonConvert.DeserializeObject(out_cont);
            string x = json?.nextLink;



            //return new OkObjectResult(System.Web.HttpUtility.UrlDecode(x));

            return new OkObjectResult(x);
        }
    }
}
