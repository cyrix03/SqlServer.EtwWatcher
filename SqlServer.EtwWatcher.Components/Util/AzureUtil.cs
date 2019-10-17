using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.EtwWatcher
{

    /// <summary>
    /// Utilities for prepping and sending log data to Azure.
    /// </summary>
    public static class AzureUtil
    {

        /// <summary>
        /// Build Azure Signature with Azure Shared Key as <see cref="Byte"/>[].
        /// </summary>
        /// <param name="message"></param>
        /// <param name="keyByte"></param>
        /// <param name="customerId"></param>
        /// <param name="datestring"></param>
        /// <returns></returns>
        public static string BuildSignature(string message, byte[] keyByte, Guid customerId, string datestring)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (customerId == null)
                throw new ArgumentNullException(nameof(customerId));

            var jsonBytes = Encoding.UTF8.GetBytes(message);
            var stringToHash = string.Concat(new object[] { "POST\n", jsonBytes.Length, "\napplication/json\nx-ms-date:", datestring, "\n/api/logs" });
            var encoding = new ASCIIEncoding();

            var messageBytes = encoding.GetBytes(stringToHash);

            using (HMACSHA256 hmacsha256 = new HMACSHA256(keyByte))
            {
                var hash = hmacsha256.ComputeHash(messageBytes);
                return string.Concat("SharedKey ", customerId.ToString(), ":", Convert.ToBase64String(hash));
            }

        }

        /// <summary>
        /// Build Azure Signature with Azure Shared Key as <see cref="String"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="secret"></param>
        /// <param name="customerId"></param>
        /// <param name="datestring"></param>
        /// <returns></returns>
        public static string BuildSignature(string message, string secret, Guid customerId, string datestring)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (secret == null)
                throw new ArgumentNullException(nameof(secret));

            if (customerId == null)
                throw new ArgumentNullException(nameof(customerId));

            var keyByte = Convert.FromBase64String(secret);
            var messageBytes = new ASCIIEncoding()
                .GetBytes(string.Concat(new object[]
                {
                    "POST\n", Encoding.UTF8.GetBytes(message).Length, "\napplication/json\nx-ms-date:", datestring, "\n/api/logs" 
                }));

            using (HMACSHA256 hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hash = hmacsha256.ComputeHash(messageBytes);
                return string.Concat("SharedKey ", customerId.ToString(), ":", Convert.ToBase64String(hash));
            }

        }

        /// <summary>
        /// Post generated log data to Azure.
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="date"></param>
        /// <param name="json"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public static async Task<string> PostData(string signature, string date, string json, Guid customerId)
        {
            if (signature == null)
                throw new ArgumentNullException(nameof(signature));

            if (date == null)
                throw new ArgumentNullException(nameof(date));

            if (json == null)
                throw new ArgumentNullException(nameof(json));

            if (customerId == null)
                throw new ArgumentNullException(nameof(customerId));

            try
            {
                string url = "https://" + customerId.ToString() + ".ods.opinsights.azure.com/api/logs?api-version=2016-04-01";

                var client = new HttpClient();

                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Log-Type", "SqlServer_Custom_DeadLock_Log");
                client.DefaultRequestHeaders.Add("Authorization", signature);
                client.DefaultRequestHeaders.Add("x-ms-date", date);

                var httpContent = new StringContent(json, Encoding.UTF8);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.PostAsync(new Uri(url), httpContent);

                client.Dispose();

                return response.ReasonPhrase;
            }
            catch (Exception exception)
            {
                return string.Concat("API Post Exception : ", exception.Message);
            }

        }

    }

}
