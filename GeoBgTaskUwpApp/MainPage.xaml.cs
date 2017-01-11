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
            try
            {
                SetStateOfControls(false);
                var session = await HttpOperationHelper.AuthRequest(UserNameTextBox.Text, PasswordTextBox.Password);
                RememberMe();

                UserNameTextBlock.Visibility = Visibility.Collapsed;
                UserNameTextBox.Visibility = Visibility.Collapsed;
                PasswordTextBlock.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                LoginButton.Visibility = Visibility.Collapsed;

                LogListView.Visibility = Visibility.Visible;

                var log = new List<string>();

                log.Add($"Вы вошли в приложение как {session.employeeName}");
                log.Add($"Определение координат успешно запущено");
                log.Add($"Каждый раз, когда сервер будет запрашивать координаты, будет информационное сообщение. Приложение можно закрыть.");

                LogListView.ItemsSource = log;


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

            vault.Add(new PasswordCredential("geoclaris", UserNameTextBox.Text, PasswordTextBox.Password));
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
                    UserNameTextBox.Text = cr.UserName;
                    cr.RetrievePassword();
                    PasswordTextBox.Password = cr.Password;
                }
            }
            catch { }
        }


        /// <summary>
        /// Настраивает доступность контролов
        /// </summary>
        private void SetStateOfControls(bool isEnabled)
        {
            UserNameTextBox.IsEnabled = PasswordTextBox.IsEnabled = LoginButton.IsEnabled = isEnabled;
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
