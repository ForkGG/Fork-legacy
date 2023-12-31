using Fork.Logic.Manager;
using Fork.logic.model.PluginModels;
using Fork.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using ICSharpCode.SharpZipLib.Zip;
using YamlDotNet.Serialization;

namespace Fork.logic.Utils
{
    internal class LocalPluginUtils
    {
        public static async Task<bool> DoVirtualLoading(PluginViewModel viewModel, string targetName, string selectedElement, string destination)
        {
            InstalledPlugin ip = new InstalledPlugin
            {
                LocalId = new Random().Next(),
                Name = targetName,
                IsSpigetPlugin = false,
                IsDownloaded = true,
                LocalPlugin = new Fork.Logic.Model.PluginModels.File
                {
                    type = "jar",
                    actualType = "jar",
                    size = new FileInfo(selectedElement).Length,
                    sizeUnit = "bytes",
                    url = destination
                }
            };

            viewModel.InstalledPlugins.Add(ip);
            viewModel.EnablePlugin(ip);
            return await PluginManager.Instance.EnablePluginAsync(ip, viewModel);
        }

        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        public static string ReadPluginName(string file)
        {
            try
            {
                using (ZipFile zip = new ZipFile(file))
                {
                    ZipEntry entry = zip.GetEntry("bungee.yml");
                    if (entry == null)
                    {
                        entry = zip.GetEntry("paper-plugin.yml");
                        if (entry == null)
                        {
                            entry = zip.GetEntry("plugin.yml");
                        }
                    }

                    if (entry != null)
                    {
                        using (Stream stream = zip.GetInputStream(entry))
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            string rawYaml = sr.ReadToEnd();
                            dynamic yaml = new DeserializerBuilder().Build().Deserialize(new StringReader(rawYaml));

                            return yaml["name"];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Append(ex);
                Console.WriteLine($"Failed to read {file} contents");
            }

            return null;
        }
    }
}
