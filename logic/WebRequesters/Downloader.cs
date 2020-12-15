using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;
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

        public static async Task DownloadFileAsync(string url, string targetPath, IProgress<double> progress)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged += (sender, args) =>
                    progress?.Report((double)args.BytesReceived / args.TotalBytesToReceive * 100d);
            await webClient.DownloadFileTaskAsync(new Uri(url), targetPath);
        }
    }
}