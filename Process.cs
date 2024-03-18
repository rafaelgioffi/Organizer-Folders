using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic.FileIO;

namespace SortFIlesDown
{
    public class Process : BackgroundService
    {
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                await GetFoldersAndFiles();

                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }

        private async static Task GetFoldersAndFiles()
        {
            string dirSettings = Path.GetFullPath($"{SpecialDirectories.MyDocuments}\\OrganizeFolders\\");
            string fileSettings = dirSettings + "appSettings.json";


            if (!File.Exists(fileSettings))
            {
                Directory.CreateDirectory(Path.GetFullPath($"{SpecialDirectories.MyDocuments}\\OrganizeFolders"));
                File.Copy(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\appSettings.json", fileSettings, true);
            }

            var config = new ConfigurationBuilder()
             .AddJsonFile(fileSettings, optional: false, reloadOnChange: true)
             .Build();

            //IConfigurationRoot config = new ConfigurationBuilder()
            ////.SetBasePath(dirSettings)
            //.AddJsonFile(fileSettings, optional: false, reloadOnChange: true)
            //.Build();

            string folder1 = config["Folders:Folder1"];
            string folder2 = config["Folders:Folder2"];
            string folder3 = config["Folders:Folder3"];
            string folder4 = config["Folders:Folder4"];
            string folder5 = config["Folders:Folder5"];
            bool isType = bool.Parse(config["Settings:FoldersPerTypes"]);
            bool isDate = bool.Parse(config["Settings:OrderByDate"]);

            List<string> folders = new();
            if (!string.IsNullOrEmpty(folder1))
                folders.Add(folder1);
            if (!string.IsNullOrEmpty(folder2))
                folders.Add(folder2);
            if (!string.IsNullOrEmpty(folder3))
                folders.Add(folder3);
            if (!string.IsNullOrEmpty(folder4))
                folders.Add(folder4);
            if (!string.IsNullOrEmpty(folder5))
                folders.Add(folder5);

            try
            {
                //var currentUser = @"C:\Users\SeuUser\";
                //var fullPath = Path.Combine(currentUser, "Downloads");
                //var queueFiles = Directory.EnumerateFiles(fullPath);

                foreach (string currentPath in folders)
                {
                    var queueFiles = Directory.EnumerateFiles(currentPath);
                    if (!queueFiles.Any())
                        return;

                    var fileInfo = new FileInfo(currentPath);
                    var directoriesToMove = new List<string>();

                    foreach (var queueFile in queueFiles)
                    {
                        fileInfo = new FileInfo(queueFile);

                        if (isDate)
                        {
                            string fileYear = fileInfo.LastWriteTime.Year.ToString();
                            string fileMonth = fileInfo.LastWriteTime.Month.ToString("D2");
                            var folderYear = Path.Combine(currentPath, fileYear);
                            var folderMonth = Path.Combine(currentPath, fileYear + "\\" + fileMonth);

                            if (!Directory.Exists(folderYear))
                                Directory.CreateDirectory(folderYear).Create();

                            if (!Directory.Exists(folderMonth))
                                Directory.CreateDirectory(folderMonth);

                            if (!directoriesToMove.Contains(folderMonth))
                                directoriesToMove.Add(folderMonth);
                        }
                        else if (isType)
                        {
                            var folderName = Path.Combine(currentPath, $"Arquivos-{fileInfo.Extension.Trim('.').ToLower()}");

                            if (!Directory.Exists(folderName))
                                Directory.CreateDirectory(folderName).Create();

                            if (!directoriesToMove.Contains(folderName))
                                directoriesToMove.Add(folderName);
                        }

                    }

                    var pahtFiles = new List<string>();

                    foreach (var queueDirectories in directoriesToMove)
                    {
                        foreach (var queue in queueFiles)
                        {
                            fileInfo = new FileInfo(queue);
                            //var folderName = Path.Combine(queueDirectories, $"{fileInfo.LastWriteTimeUtc.ToShortDateString().Replace("/", "-")}");

                            //if (!Directory.Exists(folderName) && folderName.Contains(fileInfo.Extension.Replace(".", "-").ToLower()))
                                //Directory.CreateDirectory(folderName).Create();

                            //if (!pahtFiles.Contains(folderName))
                            if (!pahtFiles.Contains(queueDirectories))
                                //pahtFiles.Add(folderName);                                
                                pahtFiles.Add(queueDirectories);
                        }
                    }

                    MoveFiles(ref pahtFiles, ref queueFiles, ref fileInfo);

                    pahtFiles.Clear();
                }

            }
            catch (Exception)
            {

            }
            await Task.FromResult(0);
        }
        private static void MoveFiles(ref List<string> pathFiles, ref IEnumerable<string> fileList, ref FileInfo fileInfo)
        {
            try
            {
                foreach (var file in fileList)
                {
                    fileInfo = new FileInfo(file);

                    foreach (var folder in pathFiles)
                    {
                        //var fileInfoDate = fileInfo.LastWriteTimeUtc.ToShortDateString().Replace("/", "-");


                        //if (folder.EndsWith(fileInfoDate) && folder.ToLower().Contains(fileInfo.Extension.Replace('.', '-').ToLower()))
                        //{

                        //var result = fileExistsInPath(folder, fileInfo.Name, fileInfo.Extension);
                        var result = fileExistsInPath(folder, fileInfo.Name, fileInfo.Extension);

                        File.Move(file, result);

                        break;
                        //}

                    }


                }
            }
            catch (Exception) { }
        }
        private static string fileExistsInPath(string directory, string file, string extension)
        {
            int fileNum = 2;
            var fullPath = Path.Combine(directory, file);

            if (!File.Exists(fullPath))
            {
                return fullPath;
            }

            file = file.Replace(extension, "");

            //var newFileName = Path.Combine(directory, $"{file}-Copy-{DateTime.Now.Nanosecond}{extension}");
            var newFileName = Path.Combine(directory, $"{file} ({fileNum}){extension}");

            return newFileName;
        }

    }
}
