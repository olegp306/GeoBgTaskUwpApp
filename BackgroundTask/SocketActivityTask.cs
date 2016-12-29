using System;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Background;
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
                switch (details.Reason)
                {
                    case SocketActivityTriggerReason.SocketActivity:
                        var socket = socketInformation.StreamSocket;

                        DataWriter writer0 = new DataWriter(socket.OutputStream);
                        writer0.WriteBytes(Encoding.UTF8.GetBytes("bla bla coordinate"));
                        await writer0.StoreAsync();
                        writer0.DetachStream();
                        writer0.Dispose();
                        ShowToast("bla bla coordinate");
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
                ShowToast(exception.Message);
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
