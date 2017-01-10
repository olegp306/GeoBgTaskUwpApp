using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace GeoBgTaskUwpApp.Common
{
    public class HttpOperationHelper
    {
        public static async Task<bool> AuthRequest(string login, string password)
        {
            var uri = string.Format("{0}?action=login", App.SERVER_URL);
            var version = "99";// Package.Current.Id.Version.Major.ToString();
            var request = new { user = login, password = password, api = version };

            HttpContent content = new StringContent(JsonConvert.SerializeObject(request));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            var response = await client.PostAsync(uri, content);
            string recievedContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    var dataProto = new { sessionId = Guid.Empty, loginGuid = Guid.Empty, employeeGuid = Guid.Empty, employeeName = string.Empty, employeePhone = string.Empty, employeeAuto = string.Empty, timezone = 0, serverDateTime = DateTimeOffset.MinValue, admin = false, nfcMaster = false, geoTracking = false };
                    var data = JsonConvert.DeserializeAnonymousType(recievedContent, dataProto);

                    ApplicationData.Current.LocalSettings.Values["EmployeeGuid"] = data.employeeGuid;
                    return true;
                }
                catch (Exception e0)
                {
                    LoggingHelper.Trace($"В процессе аутентификация пользователя с сервера получен некорректный ответ: {e0}");
                    throw new Exception("В процессе аутентификация пользователя с сервера получен некорректный ответ");
                }
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new HttpRequestException("Сервер не доступен. Проверьте интернет-соединение.");
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException(recievedContent);
            }
            else if (response.StatusCode == HttpStatusCode.UpgradeRequired)
            {
                throw new PlatformNotSupportedException(recievedContent);
            }
            else
            {
                throw new Exception(recievedContent);
            }
        }
    }
}
