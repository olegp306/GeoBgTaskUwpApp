using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace GeoBgTaskUwpApp.Common
{

    /// <summary>
    /// Настройки  Гео
    /// </summary>
    public class GeoSettings
    {
        public static int SendGeoDataPediodOnRunning
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values["SendGeoDataPediodOnRunning"] == null)
                    ApplicationData.Current.LocalSettings.Values["SendGeoDataPediodOnRunning"] = 60;

                return int.Parse(ApplicationData.Current.LocalSettings.Values["SendGeoDataPediodOnRunning"].ToString());
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values["SendGeoDataSetting"] = value;
            }
        }
        /// <summary>
        /// Ручная настройка отправлять гео данные или нет
        /// </summary>
        public static bool SendGeoDataSetting
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values["SendGeoDataSetting"] == null)
                    ApplicationData.Current.LocalSettings.Values["SendGeoDataSetting"] = false;

                return bool.Parse(ApplicationData.Current.LocalSettings.Values["SendGeoDataSetting"].ToString());
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values["SendGeoDataSetting"] = value;
            }
        }

        /// <summary>
        /// Дата и время последней синхронизации
        /// </summary>
        public static DateTime LastTimeCoordSyncDatetime
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values["LastTimeCoordSyncDatetime"] == null)
                {
                    //если нет данных о последней синхронизации - то 10 дней назад(max глубина LumiaMotion)
                    ApplicationData.Current.LocalSettings.Values["LastTimeCoordSyncDatetime"] = (DateTime.Now - TimeSpan.FromDays(10)).ToString();
                }

                return DateTime.Parse(ApplicationData.Current.LocalSettings.Values["LastTimeCoordSyncDatetime"].ToString());
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values["LastTimeCoordSyncDatetime"] = value.ToString();
            }
        }


        public const double RequestTimeOut = 10000;

        public const string GeoApiUrl = "https://map-asuss.melston.ru/api/usergeochronology";

        public const string AuthApiUrl = "https://claris.eu.auth0.com/oauth/ro";

        public const string RefresthApiTokenUrl = "https://claris.eu.auth0.com/delegation";

        public const string LastEmployerCoordApiUrl = "https://map-asuss.melston.ru/api/UserGeochronology/";

        public const string LastDistrictEmployersCoordsApiUrl = "https://map-asuss.melston.ru/api/usergeochronology/actual?districtGuid=";


        /// <summary>
        /// Текущий токен для GeoApi
        /// </summary>
        public static string CurrentGeoIdToken
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values["CurrentGeoIdToken"] == null)
                {
                    //если нет данных о последней синхронизации - то 10 дней назад(max глубина LumiaMotion)
                    ApplicationData.Current.LocalSettings.Values["CurrentGeoIdToken"] = string.Empty;
                }

                return ApplicationData.Current.LocalSettings.Values["CurrentGeoIdToken"].ToString();
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values["CurrentGeoIdToken"] = value;
            }
        }

        /// <summary>
        /// Текущий Access для GeoApi
        /// </summary>
        public static string CurrentGeoAccessToken
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values["CurrentGeoAccessToken"] == null)
                {
                    //если нет данных о последней синхронизации - то 10 дней назад(max глубина LumiaMotion)
                    ApplicationData.Current.LocalSettings.Values["CurrentGeoAccessToken"] = string.Empty;
                }

                return ApplicationData.Current.LocalSettings.Values["CurrentGeoAccessToken"].ToString();
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values["CurrentGeoAccessToken"] = value;
            }
        }

    }

}

