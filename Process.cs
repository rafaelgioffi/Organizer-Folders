using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json.Linq;

namespace SortFIlesDown
{
    public class Process : BackgroundService
    {
        string fileSettings = "";
        string dirSettings = "";
        bool logging = false;
        bool cleanTemp = false;

        IConfigurationRoot config;

        public void ReadConfigFile()
        {
            Log("Iniciando a execução...");

            dirSettings = Path.GetFullPath($"{SpecialDirectories.MyDocuments}\\OrganizeFolders\\");
            fileSettings = dirSettings + "appSettings.json";
            string projectFilePath = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\appSettings.json";

            if (!Directory.Exists(dirSettings))
            {
                try
                {
                    Directory.CreateDirectory(dirSettings);
                    Log($"Diretório {dirSettings} criado.");
                }
                catch (Exception ex)
                {
                    Log($"Falha ao criar o diretório {dirSettings}. {ex.Message}");
                }
            }

            if (!File.Exists(fileSettings))
            {
                try
                {
                    File.Copy(projectFilePath, fileSettings, true);
                    Log($"Arquivo {projectFilePath} copiado para {fileSettings}");
                }
                catch (Exception ex)
                {
                    Log($"Falha ao copiar o arquivo de configurações para {fileSettings}. {ex.Message}");
                }
            }

            //verifica se os arquivos são iguais, senão substitui na pasta de destino...
            string projectFile = File.ReadAllText(projectFilePath);
            string destinationFile = File.ReadAllText(fileSettings);

            JObject jpf = JObject.Parse(projectFile);
            JObject jdf = JObject.Parse(destinationFile);

            if (!JToken.DeepEquals(jpf, jdf))
            {
                try
                {
                    File.Copy(projectFilePath, fileSettings, true);
                    Log($"Arquivo de configurações diferentes na origem e/ou destino. Arquivo substituído");
                }
                catch (Exception ex)
                {
                    Log($"Erro ao copiar o arquivo {projectFilePath} para {fileSettings}. {ex.Message}");
                }
            }

            config = new ConfigurationBuilder()
            //.SetBasePath(dirSettings)
            .AddJsonFile(fileSettings, optional: false, reloadOnChange: true)
            .Build();

            logging = bool.Parse(config["Settings:Log"]);
            cleanTemp = bool.Parse(config["Settings:CleanTempData"]);
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ReadConfigFile();

            int monitoringTime;
            int.TryParse(config["Settings:IntervalMonitoring"], out monitoringTime);

            while (!stoppingToken.IsCancellationRequested)
            {
                await GetFoldersAndFiles();

                ClearTemp();

                Log("Fim da execução");
                Log("", true);
                Log($"Aguardando {monitoringTime} minutos para a próxima execução...");
                Log("", true);

                await Task.Delay(TimeSpan.FromMinutes(monitoringTime));
            }
        }

        private async Task GetFoldersAndFiles()
        {
            //string dirSettings = Path.GetFullPath($"{SpecialDirectories.MyDocuments}\\OrganizeFolders\\");
            //string fileSettings = dirSettings + "appSettings.json";

            //if (!File.Exists(fileSettings))
            //{
            //    Directory.CreateDirectory(Path.GetFullPath($"{SpecialDirectories.MyDocuments}\\OrganizeFolders"));
            //    File.Copy(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\appSettings.json", fileSettings, true);
            //}

            //var config = new ConfigurationBuilder()
            // .AddJsonFile(fileSettings, optional: false, reloadOnChange: true)
            // .Build();

            //IConfigurationRoot config = new ConfigurationBuilder()
            ////.SetBasePath(dirSettings)
            //.AddJsonFile(fileSettings, optional: false, reloadOnChange: true)
            //.Build();


            string folder1 = config["Folders:Folder1"];
            string folder2 = config["Folders:Folder2"];
            string folder3 = config["Folders:Folder3"];
            string folder4 = config["Folders:Folder4"];
            string folder5 = config["Folders:Folder5"];

            string type1 = config["FolderOrderType:Type1"];
            string type2 = config["FolderOrderType:Type2"];
            string type3 = config["FolderOrderType:Type3"];
            string type4 = config["FolderOrderType:Type4"];
            string type5 = config["FolderOrderType:Type5"];

            List<string> folders = new();
            List<string> types = new();
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

            if (!string.IsNullOrEmpty(type1))
                types.Add(type1);
            if (!string.IsNullOrEmpty(type2))
                types.Add(type2);
            if (!string.IsNullOrEmpty(type3))
                types.Add(type3);
            if (!string.IsNullOrEmpty(type4))
                types.Add(type4);
            if (!string.IsNullOrEmpty(type5))
                types.Add(type5);

            if (folders.Count != types.Count)
            {
                Log($"Configurações inválidas! Pastas: {folders.Count}. Tipos de pastas: {types.Count}.\nÉ necessário ter as mesmas configurações.");
                return;
            }

            try
            {
                //var currentUser = @"C:\Users\SeuUser\";
                //var fullPath = Path.Combine(currentUser, "Downloads");
                //var queueFiles = Directory.EnumerateFiles(fullPath);

                //foreach (string currentPath in folders)
                for (int i = 0; i < folders.Count; i++)
                {
                    //var queueFiles = Directory.EnumerateFiles(currentPath);
                    var queueFiles = Directory.EnumerateFiles(folders[i]);
                    if (queueFiles.Any())
                    {
                        Log($"Processando a pasta {folders[i]}");
                        //return;

                        //var fileInfo = new FileInfo(currentPath);
                        var fileInfo = new FileInfo(folders[i]);
                        //var directoriesToMove = new List<string>();

                        foreach (var queueFile in queueFiles)
                        {
                            fileInfo = new FileInfo(queueFile);

                            switch (types[i])
                            {
                                case "date":
                                    string fileYear = fileInfo.CreationTime.Year.ToString();
                                    string fileMonth = fileInfo.CreationTime.Month.ToString("D2");
                                    //var folderYear = Path.Combine(currentPath, fileYear);
                                    var folderYear = Path.Combine(folders[i], fileYear);
                                    //var folderMonth = Path.Combine(currentPath, fileYear + "\\" + fileMonth);
                                    var folderMonth = Path.Combine(folders[i], fileYear + "\\" + fileMonth);

                                    if (!Directory.Exists(folderYear))
                                    {
                                        Directory.CreateDirectory(folderYear).Create();
                                        Log($"Criada a pasta {folderYear}");
                                    }

                                    if (!Directory.Exists(folderMonth))
                                    {
                                        Directory.CreateDirectory(folderMonth);
                                        Log($"Criada a pasta {folderMonth}");
                                    }

                                    //if (!directoriesToMove.Contains(folderMonth))
                                    //directoriesToMove.Add(folderMonth);

                                    int fileNumber = 1;
                                    var result = fileExistsInPath(folderMonth, fileInfo.Name, fileInfo.Extension, fileNumber);

                                    try
                                    {
                                        File.Move(queueFile, result);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log($"Falha ao mover o arquivo {queueFile}. {ex.Message}");
                                    }
                                    break;

                                case "type":
                                    //var folderName = Path.Combine(currentPath, fileInfo.Extension.Trim('.').ToLower());
                                    var folderName = "";
                                    if (string.IsNullOrEmpty(fileInfo.Extension))
                                    {
                                        folderName = Path.Combine(folders[i], "no-extension");
                                    }
                                    else
                                    {
                                        folderName = Path.Combine(folders[i], fileInfo.Extension.Trim('.').ToLower());
                                    }

                                    if (!Directory.Exists(folderName))
                                    {
                                        Directory.CreateDirectory(folderName).Create();
                                        Log($"Criada a pasta {folderName}");
                                    }

                                    //if (!directoriesToMove.Contains(folderName))
                                    //    directoriesToMove.Add(folderName);

                                    fileNumber = 1;
                                    result = fileExistsInPath(folderName, fileInfo.Name, fileInfo.Extension, fileNumber);

                                    try
                                    {
                                        File.Move(queueFile, result);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log($"Falha ao mover o arquivo {queueFile}. {ex.Message}");
                                    }
                                    break;
                            }
                        }
                    }

                    //var pathFiles = new List<string>();

                    //foreach (var queueDirectories in directoriesToMove)
                    //{
                    //    foreach (var queue in queueFiles)
                    //    {
                    //        fileInfo = new FileInfo(queue);
                    //        //var folderName = Path.Combine(queueDirectories, $"{fileInfo.LastWriteTimeUtc.ToShortDateString().Replace("/", "-")}");
                    //        //var folderName = Path.Combine(queueDirectories, $"{fileInfo.CreationTime.Year}\\{fileInfo.CreationTime.Month}");

                    //        //if (!Directory.Exists(folderName) && folderName.Contains(fileInfo.Extension.Replace(".", "-").ToLower()))
                    //        //Directory.CreateDirectory(folderName).Create();

                    //        //if (!pathFiles.Contains(folderName))
                    //        if (!pathFiles.Contains(queueDirectories))
                    //            //pathFiles.Add(folderName);
                    //            pathFiles.Add(queueDirectories);
                    //    }
                    //}

                    //MoveFiles(ref pathFiles, ref queueFiles, ref fileInfo);

                    //pathFiles.Clear();
                }
            }
            catch (Exception ex)
            {
                Log($"Falha ao processar... {ex.Message}");
            }
            await Task.FromResult(0);
        }
        private void MoveFiles(ref List<string> pathFiles, ref IEnumerable<string> fileList, ref FileInfo fileInfo)
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
                        int fileNumber = 1;
                        var result = fileExistsInPath(folder, fileInfo.Name, fileInfo.Extension, fileNumber);

                        try
                        {
                            File.Move(file, result);
                        }
                        catch (Exception ex)
                        {
                            Log($"Falha ao mover o arquivo {file} para {result}. {ex.Message}");
                        }

                        break;
                        //}
                    }
                }
            }
            catch (Exception) { }
        }

        private string fileExistsInPath(string directory, string file, string extension, int number)
        {
            var fullPath = Path.Combine(directory, file);

            if (!File.Exists(fullPath))
            {
                return fullPath;
            }

            file = file.Replace(extension, "");

            //var newFileName = Path.Combine(directory, $"{file}-Copy-{DateTime.Now.Nanosecond}{extension}");
            var newFileName = Path.Combine(directory, $"{file} ({number}){extension}");

            while (File.Exists(newFileName))
            {
                number++;
                newFileName = Path.Combine(directory, $"{file} ({number}){extension}");
                Log($"Arquivo {file} já existe em {fullPath}. Tentando salvar como {newFileName}.");
            }

            return newFileName;
        }

        public void ClearTemp()
        {
            //cleanTemp = bool.Parse(config["Settings:CleanTempData"]);
            if (cleanTemp)
            {
                string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp\\");
                string winTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp\\");
                string prefetchDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch\\");
                try {
                    Directory.Delete(tempFolder, true);                    
                }
                catch (Exception ex)
                {
                    Log($"Falha ao excluir arquivo temporário. {ex.Message}");
                }

                try
                {
                    Directory.Delete(winTemp, true);
                }
                catch (Exception ex)
                {
                    Log($"Falha ao excluir arquivo temporário. {ex.Message}");
                }

                try
                {
                    Directory.Delete(prefetchDir, true);
                }
                catch (Exception ex)
                {
                    Log($"Falha ao excluir arquivo temporário. {ex.Message}");
                }
            }
        }

        public void Log(string message, bool special = false)
        {
            //logging = bool.Parse(config["Settings:Log"]);
            if (logging)
            {
                using (StreamWriter sw = new StreamWriter(config["Logging:File"], true))
                {
                    if (special)
                    {
                        sw.WriteLine(message);
                        Console.WriteLine(message);
                    }
                    else
                    {
                        sw.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} => {message}");
                        Console.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} => {message}");
                    }
                }
            }
        }

    }
}
