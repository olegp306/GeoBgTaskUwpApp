using GeoBgTaskUwpApp.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Security.Credentials;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GeoBgTaskUwpApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string socketId = "GeoSocket";
        private StreamSocket socket = null;
        private IBackgroundTaskRegistration task = null;
        private const string port = "22668";


        public MainPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Geolocator locator = new Geolocator();
            var accessStatus = await Geolocator.RequestAccessAsync();
            LoadMe();

            try
            {
                foreach (var current in BackgroundTaskRegistration.AllTasks)
                {
                    if (current.Value.Name == "BackgroundTask")
                    {
                        task = current.Value;
                        break;
                    }
                }

                // If there is no task allready created, create a new one
                if (task == null)
                {
                    var socketTaskBuilder = new BackgroundTaskBuilder();
                    socketTaskBuilder.Name = "BackgroundTask";
                    socketTaskBuilder.TaskEntryPoint = "BackgroundTask.SocketActivityTask";
                    var trigger = new SocketActivityTrigger();
                    socketTaskBuilder.SetTrigger(trigger);
                    task = socketTaskBuilder.Register();
                }

                SocketActivityInformation socketInformation;
                if (SocketActivityInformation.AllSockets.TryGetValue(socketId, out socketInformation))
                {
                    // Application can take ownership of the socket and make any socket operation
                    // For sample it is just transfering it back.
                    socket = socketInformation.StreamSocket;
                    socket.TransferOwnership(socketId);
                    socket = null;

                    LogListView.Items.Add("Connected. You may close the application");
                    /*rootPage.NotifyUser("Connected. You may close the application", NotifyType.StatusMessage);
                    TargetServerTextBox.IsEnabled = false;
                    ConnectButton.IsEnabled = false;*/
                }

            }
            catch (Exception exception)
            {
                await new MessageDialog(exception.Message).ShowAsync();
            }

        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {

            //if (App.USE_TEST_DATASET)
            //{
            //    Frame.Navigate(typeof(MainPage));
            //    return;
            //}

            //var statusBar = StatusBar.GetForCurrentView();
            try
            {
                SetStateOfControls(false);
                //Аутентификация
                //statusBar.ProgressIndicator.Text = "Вход пользователя...";
                //await statusBar.ProgressIndicator.ShowAsync();

                var session = await HttpOperationHelper.AuthRequest(UserName.Text, Password.Password);


//#if !DEBUG
//                //Проверка времени на телефоне.
//                if ((App.Current as App).ConnectionState == eConnectionState.ONLINE)
//                {
//                    var expectedTime = session.ServerDateTime.ToUniversalTime();
//                    var expectedOffset = session.ServerDateTime.Offset.Hours + session.Timezone;
//                    var currentTime = DateTimeOffset.Now.ToUniversalTime();
//                    var currentOffset = DateTimeOffset.Now.Offset.Hours;
//                    var diff = Math.Abs((currentTime - expectedTime).TotalMinutes);

//                    if (diff > 5.0 || expectedOffset != currentOffset)
//                    {
//                        await statusBar.ProgressIndicator.HideAsync();
//                        SetStateOfControls(true);
//                        var region = session.Timezone == 0 ? string.Empty : $" (МСК+{ session.Timezone }ч.)";
//                        var curregion = $" (МСК+{ DateTimeOffset.Now.Offset.Hours - 3 }ч.)";
//                        //var message = $"На телефоне установлена некорректная дата или время. Ожидаемые дата и время для вашего региона{ region } { session.ServerDateTime.AddHours(session.Timezone).ToString("dd MMM yyyy HH:mm") }. Текущее время{ curregion } { DateTimeOffset.Now.ToString("dd MMM yyyy HH:mm") }. Измененные дата и время вступят в силу только после перезапуска приложения.";
//                        var message = $"На телефоне установлена некорректная дата и время. Установите корректный часовой пояс (UTC+{ expectedOffset }) и корректное время { session.ServerDateTime.AddHours(session.Timezone).ToString("dd MMM yyyy HH:mm") }.";
//                        LoggingHelper.Log(message, new { Employee = session.EmployeeName, ExpectedTime = expectedTime, ExpectedOffset = expectedOffset, CurrentTime = currentTime, CurrentOffset = currentOffset, Diff = diff });
//                        await new MessageDialog(message, "Внимание").ShowAsync();
//                        return;
//                    }
//                }
//#endif

                //await NotificationHelper.Init(session.UserGuid);
                //(App.Current as App).CurrentSession = session;
                //GeoSettings.SendGeoDataSetting = session.IsGeoTrackingOn;



                RememberMe();

                //await statusBar.ProgressIndicator.HideAsync();
                //Иниц-я Init Core
                //if (GeoSettings.SendGeoDataSetting)
                //{
                //    if (!(await LumiaMotionHelper.InitCore()))
                //    {
                //        SetStateOfControls(true);
                //        return;
                //    }
                //}

                //Синхронизация
                //var db = new AsyncDBHelper();
                //await db.InsertAsync(session);

                //var res = await db.Synchronize(true);
                //if (res != eSynchronizationResult.SUCCES)
                //{
                //    //Проверим, есть ли заявки у пользователя уже. Если есть переходим в оффлайн, если нет - выводим сообщение 
                //    var r = await db.Table<RequestViewModel>().ToListAsync();
                //    if (!r.Any())
                //    {
                //        if (res == eSynchronizationResult.NETWORK_ERROR)
                //            throw new HttpRequestException();

                //        if (res == eSynchronizationResult.FAILURE)
                //            throw new Exception();
                //    }
                //}

                //(App.Current as App).SynchronizationTask.Start();

                //if (GeoSettings.SendGeoDataSetting)
                //    (App.Current as App).GeoSynchronizationTask.Start();


                // Jump();
                await InitTask();
            }
            catch (HttpRequestException e0)
            {
                //await statusBar.ProgressIndicator.HideAsync();
                LoggingHelper.Trace(e0.ToString());
                await new MessageDialog("Сервер не доступен, проверьте свое интернет соединение! Из-за отсутствия данных, работа в offline режиме невозможна.").ShowAsync(); ;
                SetStateOfControls(true);
            }
            catch (UnauthorizedAccessException e1)
            {
                //await statusBar.ProgressIndicator.HideAsync();
                LoggingHelper.Trace(e1.ToString());
                await new MessageDialog("Неверное имя пользователя или пароль.", "Ошибка входа").ShowAsync();
                SetStateOfControls(true);
            }
            catch (PlatformNotSupportedException e3)
            {
                //await statusBar.ProgressIndicator.HideAsync();
                LoggingHelper.Trace(e3.ToString());
                var dataProto = new { message = string.Empty };
                var data = JsonConvert.DeserializeAnonymousType(e3.Message, dataProto);
                await new MessageDialog($"Доступна новая версия мобильного приложения {data.message}. Для продолжения работы необходимо обновить приложение.", "Необходимо обновление").ShowAsync();
                SetStateOfControls(true);
            }
            catch (Exception e2)
            {
               // await statusBar.ProgressIndicator.HideAsync();
                LoggingHelper.Trace(e2.ToString());
                await new MessageDialog($"Во время входа в приложение произошла ошибка: {e2}").ShowAsync();
                SetStateOfControls(true);
            }
        }

        /// <summary>
        /// Сохраняет учетную запись пользователя в хранилище
        /// </summary>
        private void RememberMe()
        {
            var vault = new PasswordVault();
            try
            {
                var creds = vault.FindAllByResource("geoclaris");
                foreach (var cred in creds)
                    vault.Remove(cred);
            }
            catch { }

            vault.Add(new PasswordCredential("geoclaris", UserName.Text, Password.Password));
        }

        /// <summary>
        /// Загружает учетную запись пользователя из хранилища
        /// </summary>
        private void LoadMe()
        {
            var vault = new PasswordVault();
            try
            {
                var crs = vault.FindAllByResource("geoclaris");
                if (crs.Any())
                {
                    var cr = crs.First();
                    UserName.Text = cr.UserName;
                    cr.RetrievePassword();
                    Password.Password = cr.Password;
                }
            }
            catch { }
        }


        /// <summary>
        /// Настраивает доступность контролов
        /// </summary>
        private void SetStateOfControls(bool isEnabled)
        {
            UserName.IsEnabled = Password.IsEnabled = Login.IsEnabled = isEnabled;
        }




        /// <summary>
        /// Инициализация фоновой задачи
        /// </summary>
        private async Task InitTask()
        {
            try
            {
                SocketActivityInformation socketInformation;
                if (!SocketActivityInformation.AllSockets.TryGetValue(socketId, out socketInformation))
                {
                    socket = new StreamSocket();
                    socket.EnableTransferOwnership(task.TaskId, SocketActivityConnectedStandbyAction.Wake);
                    var targetServer = new HostName("62.128.104.89");
                    await socket.ConnectAsync(targetServer, port);
                    // To demonstrate usage of CancelIOAsync async, have a pending read on the socket and call 
                    // cancel before transfering the socket. 
                    /*DataReader reader = new DataReader(socket.InputStream);
                    reader.InputStreamOptions = InputStreamOptions.Partial;
                    var read = reader.LoadAsync(250);
                    read.Completed += (info, status) =>
                    {

                    };
                    await socket.CancelIOAsync();*/
                    socket.TransferOwnership(socketId);
                    socket = null;
                }
            }
            catch (Exception exception)
            {
                await new MessageDialog(exception.Message).ShowAsync();
            }
        }
    }
}
