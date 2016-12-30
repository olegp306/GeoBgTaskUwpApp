using GeoBgTaskUwpApp.Common;
using GeoBgTaskUwpApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;

namespace BackgroundTask
{
    public sealed class SocketActivityTask : IBackgroundTask
    {
        private const string socketId = "GeoSocket";

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            try
            {
                var details = taskInstance.TriggerDetails as SocketActivityTriggerDetails;
                var socketInformation = details.SocketInformation;
                Debug.WriteLine("Запустилась таска");
                switch (details.Reason)
                {
                    case SocketActivityTriggerReason.SocketActivity:
                        var socket = socketInformation.StreamSocket;

                        Geolocator locator = new Geolocator();
                        var coordinate = await locator.GetGeopositionAsync();
                        BasicGeoposition snPosition = new BasicGeoposition() { Latitude = coordinate.Coordinate.Point.Position.Latitude, Longitude = coordinate.Coordinate.Point.Position.Longitude };
                        Geopoint snPoint = new Geopoint(snPosition);
                        var g = new GeoData() { DateTime = DateTime.Now, Guid = Guid.NewGuid().ToString(), ObjectGuid = new Guid("166F946A-5D1B-4CB3-BDEE-234DC677B593").ToString(), DepartmentGuid = new Guid("e054e33f-a3bc-4c67-a5e8-791f6c910140").ToString() };
                        await GeoDataSender.SendGeoData(new List<GeoData>() { g });

                        ShowToast("Получен запрос координат от сервера. Данные успешно отправлены.");

                        socket.TransferOwnership(socketInformation.Id);
                        break;
                    case SocketActivityTriggerReason.KeepAliveTimerExpired:
                        socket = socketInformation.StreamSocket;
                        DataWriter writer = new DataWriter(socket.OutputStream);
                        writer.WriteBytes(Encoding.UTF8.GetBytes("Keep alive"));
                        await writer.StoreAsync();
                        writer.DetachStream();
                        writer.Dispose();
                        socket.TransferOwnership(socketInformation.Id);
                        break;
                    case SocketActivityTriggerReason.SocketClosed:
                        socket = new StreamSocket();
                        socket.EnableTransferOwnership(taskInstance.Task.TaskId, SocketActivityConnectedStandbyAction.Wake);
                        if (ApplicationData.Current.LocalSettings.Values["hostname"] == null)
                        {
                            break;
                        }
                        var hostname = (String)ApplicationData.Current.LocalSettings.Values["hostname"];
                        var port = (String)ApplicationData.Current.LocalSettings.Values["port"];
                        await socket.ConnectAsync(new HostName(hostname), port);
                        socket.TransferOwnership(socketId);
                        break;
                    default:
                        break;
                }

                deferral.Complete();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                deferral.Complete();
            }
        }


        public void ShowToast(string text)
        {
            var toastNotifier = ToastNotificationManager.CreateToastNotifier();
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var textNodes = toastXml.GetElementsByTagName("text");
            textNodes.First().AppendChild(toastXml.CreateTextNode(text));
            var toastNotification = new ToastNotification(toastXml);
            toastNotifier.Show(new ToastNotification(toastXml));
        }

    }
}
