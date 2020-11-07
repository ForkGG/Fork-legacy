using System;
using System.IO;
using System.Net;
using System.Threading;
using Fork.ViewModel;

namespace Fork.Logic.WebRequesters
{
    public static class Downloader
    {
        public static void DownloadJarAsync(EntityViewModel viewModel, DirectoryInfo directoryInfo)
        {
            Thread thread = new Thread(() =>
            {
                viewModel.StartDownload();
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += viewModel.DownloadProgressChanged;
                webClient.DownloadFileCompleted += viewModel.DownloadCompletedHandler;
                webClient.DownloadFileAsync(new Uri(viewModel.Entity.Version.JarLink),
                    Path.Combine(directoryInfo.FullName, "server.jar"));
            }){IsBackground = true};
            thread.Start();
        }
    }
}