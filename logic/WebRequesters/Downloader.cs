using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.ViewModel;

namespace Fork.Logic.WebRequesters
{
    public static class Downloader
    {
        private static readonly List<EntityViewModel> jarDownloading = new();
        private static readonly List<EntityViewModel> downloadCanceled = new();

        public static void DownloadJarAsync(EntityViewModel viewModel, DirectoryInfo directoryInfo)
        {
            Task.Run(async () =>
            {
                try
                {
                    jarDownloading.Add(viewModel);
                    viewModel.StartDownload();
                    HttpClient httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                        ApplicationManager.UserAgent);
                    string downloadUrl = viewModel.Entity.Version.JarLink;
                    long length;
                    using (HttpResponseMessage headMessage =
                        await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, downloadUrl)))
                    {
                        if (headMessage.Content.Headers.ContentLength != null)
                        {
                            length = (long) headMessage.Content.Headers.ContentLength;
                        }
                        else
                        {
                            length = 0;
                        }
                    }

                    using (HttpResponseMessage response =
                        httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).Result)
                    {
                        response.EnsureSuccessStatusCode();

                        await using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                            fileStream = new FileStream(Path.Combine(directoryInfo.FullName, "server.jar"),
                                FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            var totalRead = 0L;
                            var buffer = new byte[8192];
                            var isMoreToRead = true;
                            do
                            {
                                var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                                if (read == 0)
                                {
                                    isMoreToRead = false;
                                }
                                else
                                {
                                    await fileStream.WriteAsync(buffer, 0, read);

                                    totalRead += read;

                                    viewModel.DownloadProgressChanged(new object(),
                                        new DownloadProgressChangedEventArgs(totalRead, length));
                                }
                            } while (isMoreToRead && !downloadCanceled.Contains(viewModel));
                        }

                        if (!downloadCanceled.Contains(viewModel))
                        {
                            viewModel.DownloadCompletedHandler(new object(),
                                new AsyncCompletedEventArgs(null, false, null));
                        }
                        else
                        {
                            downloadCanceled.Remove(viewModel);
                        }
                        jarDownloading.Remove(viewModel);
                    }
                }
                catch (Exception e)
                {
                    //TODO show that an error occured and add retry button
                    ErrorLogger.Append(e);
                }
            });
        }

        public static async Task CancelJarDownloadAsync(EntityViewModel viewModel)
        {
            if (jarDownloading.Contains(viewModel))
            {
                downloadCanceled.Add(viewModel);
                while (jarDownloading.Contains(viewModel))
                {
                    await Task.Delay(100);
                }
            }
        }

        public static async Task DownloadFileAsync(string url, string targetPath, IProgress<double> progress)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged += (sender, args) =>
                progress?.Report((double) args.BytesReceived / args.TotalBytesToReceive * 100d);
            await webClient.DownloadFileTaskAsync(new Uri(url), targetPath);
        }

        public class DownloadProgressChangedEventArgs
        {
            public long BytesReceived { get; }

            public long TotalBytesToReceive { get; }

            public DownloadProgressChangedEventArgs(long bytesReceived, long totalBytesToReceive)
            {
                BytesReceived = bytesReceived;
                TotalBytesToReceive = totalBytesToReceive;
            }
        }
    }
}