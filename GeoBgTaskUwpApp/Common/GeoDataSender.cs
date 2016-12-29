using GeoBgTaskUwpApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace GeoBgTaskUwpApp.Common
{

    class GeoDataSender
    {
        private static string id_token { get; set; }
        private static string access_token { get; set; }

        //флажок для рекурсии
        private static bool needRepeat = true;

        /// <summary>
        /// модель данных для получения токена на geo API
        /// </summary>
        private class AuthUserData
        {
            public AuthUserData(string _username, string _password)
            {
                this.client_id = "n5caG9olJcOAl7XKOiQXxBNHFm0dfnmk";
                this.username = _username;
                this.password = _password;
                this.grant_type = "password";
                this.connection = "ClarisMS";
                this.scope = "openid";
            }

            public string client_id { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string grant_type { get; set; }
            public string connection { get; set; }
            public string scope { get; set; }
        }
        /// <summary>
        /// модель данных для refresha токена на geo API
        /// </summary>
        private class ResreshTokenData
        {
            public ResreshTokenData(string token)
            {
                this.client_id = "n5caG9olJcOAl7XKOiQXxBNHFm0dfnmk";
                this.grant_type = "urn:ietf:params:oauth:grant-type:jwt-bearer";
                this.id_token = token;
                this.access_token = "AzPnqr9anjXXhuDj";
                this.scope = "openid";
            }
            public string client_id { get; set; }
            public string grant_type { get; set; }
            public string id_token { get; set; }
            public string access_token { get; set; }
            public string scope { get; set; }
        }


        /// <summary>
        /// Посылает запрос на получение токена для Гео и записывает данные о токене  в настройках   GeoSettings.CurrentGeoIdToken и GeoSettings.CurrentGeoAccessToken
        /// </summary>
        public static async Task<bool> AuthInGeoWebApi()
        {

            bool result = false;
            string url = GeoSettings.AuthApiUrl;

            try
            {
                //даные для авторизации
                var credental = GetCurrentUserCredential();
                AuthUserData authUserData = null;


                if (credental != null)
                {
                    authUserData = new AuthUserData(credental.UserName, credental.Password);
                }
                else
                {
                    return false;
                }

                var jsonString = JsonConvert.SerializeObject(authUserData);

                if (authUserData != null)
                {

                    //Посылаем запрос с данными 
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromMilliseconds(GeoSettings.RequestTimeOut);

                        var response = await client.PostAsync(url, new StringContent(jsonString, Encoding.UTF8, "application/json"));

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string recievedContent = await response.Content.ReadAsStringAsync();
                            var okAuthData = new { id_token = string.Empty, access_token = string.Empty, token_type = string.Empty };
                            var data = JsonConvert.DeserializeAnonymousType(recievedContent, okAuthData);
                            // запишем в storage токен
                            GeoSettings.CurrentGeoIdToken = data.id_token;

                            LoggingHelper.Trace("AuthInGeoWebApi.Получены новые токены для доступа на GeoApi");
                            result = true;
                        }
                        else
                        {
                            LoggingHelper.Trace(string.Format("AuthInGeoWebApi. При получении токена ответ сервера. StatusCode={0}", response.StatusCode));
                            result = false;
                        }
                    }

                }
                return result;
            }
            catch (Exception e0)
            {
                LoggingHelper.Trace(string.Format("в AuthInGeoWebApi обработано исключение.Exception ={0}", e0.Message));
                return result;
            }

        }
        /// <summary>
        /// Продлевает действие текущего token_id и перезаписывает  GeoSettings.CurrentGeoIdToken
        /// </summary>
        public static async Task<bool> RefreshIdToken()
        {
            bool result = false;
            try
            {             //проверим есть ли текущий токен
                if (GeoSettings.CurrentGeoIdToken == string.Empty)
                {
                    LoggingHelper.Trace("RefreshIdToken. Токен не обновился т.к. нет старого токена (который обновляется)");
                    return false;
                }
                else
                {
                    ResreshTokenData resreshTokenData = new ResreshTokenData(GeoSettings.CurrentGeoIdToken);
                    var jsonRequestData = JsonConvert.SerializeObject(resreshTokenData);

                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromMilliseconds(GeoSettings.RequestTimeOut);

                        string url = GeoSettings.RefresthApiTokenUrl;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GeoSettings.CurrentGeoIdToken);
                        var response = await client.PostAsync(url, new StringContent(jsonRequestData, Encoding.UTF8, "application/json"));
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string recievedContent = await response.Content.ReadAsStringAsync();
                            var okAuthData = new { token_type = string.Empty, expires_in = string.Empty, id_token = string.Empty };
                            var data = JsonConvert.DeserializeAnonymousType(recievedContent, okAuthData);
                            // запишем в storage токен
                            GeoSettings.CurrentGeoIdToken = data.id_token;

                            result = true;
                            LoggingHelper.Trace(string.Format("RefreshIdToken. Токен Успешно обновлен. Ответ сервера {0}", response.StatusCode.ToString()));
                        }
                        else
                        {
                            result = true;
                            LoggingHelper.Trace(string.Format("RefreshIdToken. Ошибка при обновлении текущего токена. Ответ сервера {0}", response.StatusCode.ToString()));
                        }
                    }
                }
                return result;
            }
            catch (Exception e0)
            {
                LoggingHelper.Trace(string.Format("в RefreshIdToken обработано исключение.Exception ={0}", e0.Message));
                return result;
            }
        }
        /// <summary>
        /// Посылает гео данные
        /// </summary>
        /// <param name="historyList"></param>
        /// <returns></returns>
        public static async Task<bool> SendGeoData(List<GeoData> historyList)
        {
            bool result = false;
            try
            {
                if (historyList != null && historyList.Count != 0)
                {
                    string url = GeoSettings.GeoApiUrl;
                    //конвертируем гео данные для нового API
                    var requestData = ConvertGeoDateToGeoDataForApi(historyList);

                    //конверируем данные по методу от Максима
                    var converter = new FormattedDecimalConverter(CultureInfo.InvariantCulture);
                    string jsonRequestData = JsonConvert.SerializeObject(requestData, converter);

                    //проверим есть ли текущий токен
                    if (GeoSettings.CurrentGeoIdToken == string.Empty)
                    {
                        await AuthInGeoWebApi();
                    }

                    //Посылаем запрос
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromMilliseconds(GeoSettings.RequestTimeOut);

                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GeoSettings.CurrentGeoIdToken);
                        var response = await client.PostAsync(url, new StringContent(jsonRequestData, Encoding.UTF8, "application/json"));

                        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                        {
                            LoggingHelper.Trace(string.Format("Данные на GeoApi отправлены УДАЧНО. Ответ сервера {0}", response.StatusCode.ToString()));
                            result = true;
                            await RefreshIdToken();
                        }
                        else
                        {
                            LoggingHelper.Trace(string.Format("Данные на GeoApi отправлены Не УДАЧНО. Ответ сервера {0}", response.StatusCode.ToString()));
                        }

                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && needRepeat)
                        {
                            needRepeat = false;
                            LoggingHelper.Trace("GetLastEmloyerCoord. Запустил получение нового токена и AuthInGeoWebApi а затем повторно GetLastEmloyerCoord. ");

                            if (await AuthInGeoWebApi())
                                result = await SendGeoData(historyList);
                        }

                        if (!needRepeat)
                            needRepeat = true;
                    }
                }
                return result;
            }
            catch (Exception e0)
            {
                LoggingHelper.Trace(string.Format("в SendGeoData обработано исключение.Exception ={0}", e0.Message));
                return result;
            }
        }

        /// <summary>
        /// Получает последнюю координату сотрудника
        /// </summary>
        /// <returns></returns>
        //public static async Task<EmployerPoint> GetLastEmloyerCoord(string userGuid)
        //{
        //    EmployerPoint lastEmployerPoint = null;

        //    try
        //    {
        //        if (userGuid == string.Empty)
        //        {
        //            LoggingHelper.Trace("GetLastEmloyerCoord.  Передан пустой userGuid. ");
        //            return lastEmployerPoint;
        //        }
        //        if (GeoSettings.CurrentGeoIdToken == string.Empty)
        //        {
        //            LoggingHelper.Trace("GetLastEmloyerCoord. Нет  токена. Отправлен запрос на получение нового токена ");
        //            await AuthInGeoWebApi();
        //        }

        //        if (GeoSettings.CurrentGeoIdToken != string.Empty)
        //        {
        //            using (var client = new HttpClient())
        //            {
        //                client.Timeout = TimeSpan.FromMilliseconds(GeoSettings.RequestTimeOut);

        //                string url = GeoSettings.LastEmployerCoordApiUrl + userGuid + "/actual";

        //                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GeoSettings.CurrentGeoIdToken);
        //                var response = await client.GetAsync(url);

        //                if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //                {
        //                    string recievedContent = await response.Content.ReadAsStringAsync();
        //                    lastEmployerPoint = JsonConvert.DeserializeObject<EmployerPoint>(recievedContent);
        //                    LoggingHelper.Trace(string.Format("GetLastEmloyerCoord. Данные о последней координате получены УДАЧНО. Ответ сервера {0}", response.StatusCode.ToString()));
        //                    await RefreshIdToken();
        //                }
        //                else
        //                {
        //                    LoggingHelper.Trace(string.Format("GetLastEmloyerCoord. Данные о последней координате НЕ ПОЛУЧЕНЫ. Ответ сервера {0}", response.StatusCode.ToString()));
        //                }
        //                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && needRepeat)
        //                {
        //                    needRepeat = false;
        //                    LoggingHelper.Trace("GetLastEmloyerCoord. Запустил получение нового токена и AuthInGeoWebApi а затем повторно GetLastEmloyerCoord. ");
        //                    await AuthInGeoWebApi();
        //                    lastEmployerPoint = await GetLastEmloyerCoord(userGuid);
        //                }

        //                if (!needRepeat)
        //                    needRepeat = true;
        //            }
        //        }

        //        return lastEmployerPoint;
        //    }
        //    catch (Exception e0)
        //    {
        //        LoggingHelper.Trace(string.Format("в GetLastEmloyerCoord обработано исключение. Exception={0}", e0.Message));
        //        return lastEmployerPoint;
        //    }
        //}

        /// <summary>
        /// Получает последнее местоположение сотрудников участка 
        /// </summary>
        /// <returns></returns>
        //public static async Task<List<EmployerPoint>> GetLastEmloyersCoordsByDistrict(string districtGuid)
        //{
        //    List<EmployerPoint> lastEmployersPoints = null;

        //    try
        //    {
        //        if (districtGuid == string.Empty)
        //        {
        //            LoggingHelper.Trace("GetLastEmloyersCoords. Передан пустой districtGuid. ");
        //            return lastEmployersPoints;
        //        }
        //        if (GeoSettings.CurrentGeoIdToken == string.Empty)
        //        {
        //            LoggingHelper.Trace("GetLastEmloyersCoords. Нет  токена. Отправлен запрос на получение нового токена ");
        //            await AuthInGeoWebApi();
        //        }

        //        if (GeoSettings.CurrentGeoIdToken != string.Empty)
        //        {
        //            using (var client = new HttpClient())
        //            {
        //                client.Timeout = TimeSpan.FromMilliseconds(GeoSettings.RequestTimeOut);

        //                string url = GeoSettings.LastDistrictEmployersCoordsApiUrl + districtGuid;

        //                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GeoSettings.CurrentGeoIdToken);
        //                var response = await client.GetAsync(url);

        //                if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //                {
        //                    string recievedContent = await response.Content.ReadAsStringAsync();

        //                    lastEmployersPoints = JsonConvert.DeserializeObject<List<EmployerPoint>>(recievedContent);
        //                    LoggingHelper.Trace(string.Format("GetLastEmloyersCoords. Данные о последних координатах сотрудников на участке получены УДАЧНО. Ответ сервера {0}", response.StatusCode.ToString()));
        //                    await RefreshIdToken();
        //                }
        //                else
        //                {
        //                    LoggingHelper.Trace(string.Format("GetLastEmloyersCoords. Данные о последних координатах сотрудников на участке  НЕ ПОЛУЧЕНЫ. Ответ сервера {0}", response.StatusCode.ToString()));
        //                }

        //                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && needRepeat)
        //                {
        //                    needRepeat = false;
        //                    LoggingHelper.Trace("GetLastEmloyersCoords. Запустил получение нового токена и AuthInGeoWebApi а затем повторно GetLastEmloyersCoords.");
        //                    await AuthInGeoWebApi();
        //                    lastEmployersPoints = await GetLastEmloyersCoordsByDistrict(districtGuid);
        //                }

        //                if (!needRepeat)
        //                    needRepeat = true;
        //            }
        //        }

        //        return lastEmployersPoints;
        //    }
        //    catch (Exception e0)
        //    {
        //        LoggingHelper.Trace(string.Format("в GetLastEmloyersCoordsByDistrict обработано исключение. Exception={0}", e0.Message));
        //        return null;
        //    }
        //}

        //public static async Task<bool> GetNearestEmployersWithSpareparts(IEnumerable<Guid> spareParts, BasicGeoposition geoposition)
        //{
        //    FindSparePartsRequestModel findSparePartsRequestModel = new FindSparePartsRequestModel();
        //    findSparePartsRequestModel.Range = new Range() { Offset = "0", Fetch = "10" };
        //    findSparePartsRequestModel.Viewport = new Viewport() {Center= geoposition.Latitude.ToString() }

        //}
        //public static async Task<bool> GetNearestEmployersWithSpareparts(FindSparePartsRequestModel findSparePartsRequestModel)
        //{
        //    bool result = false;
        //    try
        //    {
        //        if (findSparePartsRequestModel != null )
        //        {
        //            string url = GeoSettings.GeoApiUrl;
        //            //конвертируем гео данные для нового API

        //            var requestData = sparePartsGuids;

        //            //конверируем данные по методу от Максима
        //            var converter = new FormattedDecimalConverter(CultureInfo.InvariantCulture);
        //            string jsonRequestData = JsonConvert.SerializeObject(requestData, converter);

        //            //проверим есть ли текущий токен
        //            if (GeoSettings.CurrentGeoIdToken == string.Empty)
        //            {
        //                await AuthInGeoWebApi();
        //            }

        //            //Посылаем запрос
        //            using (var client = new HttpClient())
        //            {
        //                client.Timeout = TimeSpan.FromMilliseconds(GeoSettings.RequestTimeOut);

        //                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GeoSettings.CurrentGeoIdToken);
        //                var response = await client.PostAsync(url, new StringContent(jsonRequestData, Encoding.UTF8, "application/json"));

        //                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
        //                {
        //                    LoggingHelper.Trace(string.Format("Данные на GeoApi отправлены УДАЧНО. Ответ сервера {0}", response.StatusCode.ToString()));
        //                    result = true;
        //                    await RefreshIdToken();
        //                }
        //                else
        //                {
        //                    LoggingHelper.Trace(string.Format("Данные на GeoApi отправлены Не УДАЧНО. Ответ сервера {0}", response.StatusCode.ToString()));
        //                }

        //                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && needRepeat)
        //                {
        //                    needRepeat = false;
        //                    LoggingHelper.Trace("GetLastEmloyerCoord. Запустил получение нового токена и AuthInGeoWebApi а затем повторно GetLastEmloyerCoord. ");

        //                    if (await AuthInGeoWebApi())
        //                        result = await SendGeoData(historyList);
        //                }

        //                if (!needRepeat)
        //                    needRepeat = true;
        //            }
        //        }
        //        return result;
        //    }
        //    catch (Exception e0)
        //    {
        //        LoggingHelper.Trace(string.Format("в SendGeoData обработано исключение.Exception ={0}", e0.Message));
        //        return result;
        //    }
        //}

        /// <summary>
        /// Конвертирует данные по геолокации в новый формат 
        /// </summary>
        /// <param name="geoDataHistory"></param>
        /// <returns></returns>
        private static GeoDataForApi ConvertGeoDateToGeoDataForApi(List<GeoData> geoDataHistory)
        {
            GeoDataForApi result = new GeoDataForApi();
            if (geoDataHistory != null && geoDataHistory.Count != 0)
            {
                foreach (var item in geoDataHistory)
                {
                    PointShort point = new PointShort { Lat = Math.Round(item.Latitude, 6), Lng = Math.Round(item.Longitude, 6), Tsmp = item.DateTime };
                    result.Points.Add(point);
                }
                var app = App.Current as App;
                //result.UserGuid = app.CurrentSession.EmployeeGuid.ToString();
                result.UserGuid = Guid.Empty.ToString();
            }
            return result;
        }


        /// <summary>
        ///Получает логин и пароль текущего пользователя
        /// </summary>
        /// <returns></returns>
        private static PasswordCredential GetCurrentUserCredential()
        {
            try
            {
                PasswordCredential result = null;
                var vault = new PasswordVault();
                var crs = vault.FindAllByResource("claris");
                if (crs.Any())
                {
                    result = crs.First();
                    result.RetrievePassword();
                }
                return result;
            }

            catch (Exception e0)
            {
                // LoggingHelper.Log("Exception при попытке достать логин и пароль из Vault. Exception{0} ", e0.ToString());
                return null;
            }
        }

    }
}
