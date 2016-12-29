using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.System.Profile;

namespace GeoBgTaskUwpApp.Common
{
    public sealed class LoggingHelper
    {
        /// <summary>
        /// Включена ли запись трассировок в файл
        /// </summary>
        public static bool IsTraceToFileEnabled { get; set; }

        /// <summary>
        /// Файл журнала трассировки
        /// </summary>
        public static readonly StorageFile LogFile;

        static LoggingHelper()
        {
            var task = ApplicationData.Current.LocalFolder.CreateFileAsync("trace.log", CreationCollisionOption.OpenIfExists).AsTask();
            task.Wait();
            LogFile = task.Result;
        }


        /// <summary>
        /// Выводит отладочное сообщение в Debug поток. 
        /// Дублирует сообщение в файле, если константа установлена в true
        /// </summary>
        public static void Trace(string message)
        {
            Debug.WriteLine($"{ DateTime.Now.ToString("dd MMM hh:mm:ss") }: {message}");

            try
            {
                var task0 = LogFile.GetBasicPropertiesAsync().AsTask();
                task0.Wait();

                if (task0.Result.Size > 100000000)
                    FileIO.WriteTextAsync(LogFile, $"{ DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") }: Файл журнала достиг 100 МБ и был очищен.").AsTask().Wait(); ;

                FileIO.AppendLinesAsync(LogFile, new List<string>() { $"{ DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") }: {message}" }).AsTask().Wait();

            }
            catch (Exception e0)
            {
                Debug.WriteLine($"Ошибка записи сообщения в файл журнала: {e0}");
            }
        }

        /// <summary>
        /// Фиксирует в журнале сообщение об ошибке и необходимую информацию по окружению
        /// </summary>
        //public static async void Log(Exception e)
        //{
        //    LoggingHelper.Trace(e.ToString());
        //    await addLogEntry("Exception", e.Message, e.StackTrace);
        //}

        ///// <summary>
        ///// Фиксирует в журнале сообщение и сериализованный объект
        ///// </summary>
        //public static async void Log(string message, object data)
        //{
        //    var str = string.Empty;
        //    try { str = JsonConvert.SerializeObject(data); }
        //    catch { str = data.ToString(); }

        //    LoggingHelper.Trace(message + ". Данные: " + str);
        //    await addLogEntry(message, str, string.Empty);
        //}

        /// <summary>
        /// Фиксирует информацию в SQLite базе данных
        /// </summary>
        //private static async Task addLogEntry(string title, string data, string callStack)
        //{
        //    var app = App.Current as App;
        //    var appVer = "{0}.{1}.{2}.{3}:{4}".F(Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision, GetDeviceID());
        //    var entry = new LogEntryViewModel()
        //    {
        //        Guid = Guid.NewGuid(),
        //        Date = DateTime.UtcNow,
        //        AppVersion = appVer,
        //        UserGuid = app.CurrentSession != null ? app.CurrentSession.UserGuid : Guid.Empty,
        //        Title = title,
        //        Data = data,
        //        CallStack = callStack,
        //        ReadyToSync = true
        //    };

        //    var db = new AsyncDBHelper();
        //    await db.InsertAsync(entry);
        //}

        /// <summary>
        /// Возвращает хеш уникального ИД девайса
        /// </summary>
        private static string GetDeviceID()
        {
            var token = HardwareIdentification.GetPackageSpecificToken(null);
            var hardwareId = token.Id;
            var hasher = HashAlgorithmProvider.OpenAlgorithm("MD5");
            var hashed = hasher.HashData(hardwareId);
            string hashedString = CryptographicBuffer.EncodeToHexString(hashed);
            return hashedString;
        }

        /// <summary>
        /// Возвращает объем свободного места в байтах
        /// </summary>
        public static async Task<UInt64> GetFreeSpace()
        {
            var local = ApplicationData.Current.LocalFolder;
            var retrivedProperties = await local.Properties.RetrievePropertiesAsync(new string[] { "System.FreeSpace" });
            return (UInt64)retrivedProperties["System.FreeSpace"];
        }
    }
}
