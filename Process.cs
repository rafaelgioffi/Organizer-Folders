using System.Configuration;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SortFIlesDown
{
    public class Process : BackgroundService
    {
        private readonly IConfiguration _config;
        public Process(IConfiguration config)
        {
            _config = config;
        }
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
            //IConfigurationRoot config = new ConfigurationBuilder()
            //.SetBasePath(AppContext.BaseDirectory)
            //.AddJsonFile("appsettings.json")
            //.Build();
            
            List<string> folders = new();

            try
            {
                for (int i = 1; i <= 10; i++)
                {
                    if (!string.IsNullOrWhiteSpace(_config.GetValue($"AppSettings:Folder{i}")))
                    {
                        folders.Add(config[$"Folder{i}"]);
                    }
                }

                foreach (string f in folders)
                {

                    //var currentUser = @"C:\Users\rafael";
                    //var fullPath = Path.Combine(currentUser, @"\OneDrive\Imagens\WhatsApp Images");
                    //var queueFiles = Directory.EnumerateFiles(fullPath);
                    var queueFiles = Directory.EnumerateFiles(f);
                    //var fileInfo = new FileInfo(fullPath);
                    var fileInfo = new FileInfo(f);

                    if (!queueFiles.Any())
                        return;

                    foreach (var queueFile in queueFiles)
                    {
                        fileInfo = new FileInfo(queueFile);

                        var folderName = Path.Combine(fullPath, $"Arquivos-{fileInfo.Extension.Trim('.')}");

                        if (!Directory.Exists(folderName))
                            Directory.CreateDirectory(folderName).Create();

                    }

                    var pahtFiles = new Queue<string>();

                    foreach (var queueDirectories in Directory.EnumerateDirectories(fullPath))
                    {
                        foreach (var queue in queueFiles)
                        {
                            fileInfo = new FileInfo(queue);
                            var folderName = Path.Combine(queueDirectories, $"{fileInfo.LastWriteTimeUtc.ToShortDateString().Replace("/", "-")}");

                            if (!Directory.Exists(folderName))
                                Directory.CreateDirectory(folderName).Create();

                            if (!pahtFiles.Contains(folderName))
                                pahtFiles.Enqueue(folderName);
                        }
                    }

                    MoveFiles(ref pahtFiles, ref queueFiles, ref fileInfo);
                }
            }
            catch (Exception)
            {

            }
            await Task.FromResult(0);
        }
        private static void MoveFiles(ref Queue<string> pathFiles, ref IEnumerable<string> fileList, ref FileInfo fileInfo)
        {
            try
            {
                foreach (var file in fileList)
                {
                    fileInfo = new FileInfo(file);

                    foreach (var folder in pathFiles)
                    {
                        var fileInfoDate = fileInfo.LastWriteTimeUtc.ToShortDateString().Replace("/", "-");


                        if (folder.EndsWith(fileInfoDate) && folder.ToLower().Contains(fileInfo.Extension.Replace('.', '-').ToLower()))
                        {

                            var result = fileExistsInPath(folder, fileInfo.Name, fileInfo.Extension);

                            File.Move(file, result);
                            Console.WriteLine("Passei aqui e movi");

                            break;
                        }

                    }


                }
            }
            catch (Exception) { }
        }
        private static string fileExistsInPath(string directory, string file, string extension)
        {

            var fullPath = Path.Combine(directory, file);

            if (!File.Exists(fullPath))
            {
                return fullPath;
            }

            file = file.Replace(extension, "");

            var newFileName = Path.Combine(directory, $"{file}-Copy-{DateTime.Now.Nanosecond}{extension}");

            return newFileName;
        }

    }
}
