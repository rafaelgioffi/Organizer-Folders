using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic.FileIO;

namespace OrganizeFolders
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

            //Verifica se os arquivos são iguais, senão substitui...
            DateTime dateProject = File.GetLastWriteTime(projectFilePath);
            DateTime dateDocuments = File.GetLastWriteTime(fileSettings);

            if (dateProject > dateDocuments)
            {
                try
                {
                    File.Copy(projectFilePath, fileSettings, true);
                    Log($"Arquivo {projectFilePath} copiado para {fileSettings}.");
                }
                catch (Exception ex)
                {
                    Log($"Falha ao copiar o arquivo de configurações de {projectFilePath} para {fileSettings}. {ex.Message}");
                }
            }
            else if (dateDocuments > dateProject)
            {
                try
                {
                    File.Copy(fileSettings, projectFilePath, true);
                    Log($"Arquivo {fileSettings} copiado para {projectFilePath}.");
                }
                catch (Exception ex)
                {
                    Log($"Falha ao copiar o arquivo de configurações de {projectFilePath} para {fileSettings}. {ex.Message}");
                }
            }

            //verifica se os arquivos são iguais, senão substitui na pasta de destino... DEPRECIADO

            //string projectFile = File.ReadAllText(projectFilePath);
            //string destinationFile = File.ReadAllText(fileSettings);

            //JObject jpf = JObject.Parse(projectFile);
            //JObject jdf = JObject.Parse(destinationFile);

            //if (!JToken.DeepEquals(jpf, jdf))
            //{
            //    try
            //    {
            //        File.Copy(projectFilePath, fileSettings, true);
            //        Log($"Arquivo de configurações diferentes na origem e/ou destino. Arquivo substituído");
            //    }
            //    catch (Exception ex)
            //    {
            //        Log($"Erro ao copiar o arquivo {projectFilePath} para {fileSettings}. {ex.Message}");
            //    }
            //}

            config = new ConfigurationBuilder()
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

            string order1 = config["DateOrder:Order1"];
            string order2 = config["DateOrder:Order2"];
            string order3 = config["DateOrder:Order3"];
            string order4 = config["DateOrder:Order4"];
            string order5 = config["DateOrder:Order5"];

            List<string> folders = new();
            List<string> types = new();
            List<string> orders = new();

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

            if (!string.IsNullOrEmpty(order1))
                orders.Add(order1.ToUpper());
            if (!string.IsNullOrEmpty(order2))
                orders.Add(order2.ToUpper());
            if (!string.IsNullOrEmpty(order3))
                orders.Add(order3.ToUpper());
            if (!string.IsNullOrEmpty(order4))
                orders.Add(order4.ToUpper());
            if (!string.IsNullOrEmpty(order5))
                orders.Add(order5.ToUpper());

            if (folders.Count != types.Count || types.Count != orders.Count)
            {
                Log($"Configurações inválidas! Pastas: {folders.Count}. Tipos de pastas: {types.Count} Ordenação por data: {orders.Count}.\nÉ necessário ter as mesmas configurações.");
                return;
            }

            try
            {
                for (int i = 0; i < folders.Count; i++)
                {
                    var queueFiles = Directory.EnumerateFiles(folders[i]);
                    if (queueFiles.Any())
                    {
                        Log($"Processando a pasta {folders[i]}");

                        var fileInfo = new FileInfo(folders[i]);

                        foreach (var queueFile in queueFiles)
                        {
                            fileInfo = new FileInfo(queueFile);

                            switch (types[i])
                            {
                                case "date":
                                    string fileYear = "";
                                    string fileMonth = "";

                                    if (orders[i] == "M")
                                    {
                                        fileYear = fileInfo.LastWriteTime.Year.ToString();
                                        fileMonth = fileInfo.LastWriteTime.Month.ToString("D2");
                                    }
                                    else if (orders[i] == "C")
                                    {
                                        fileYear = fileInfo.CreationTime.Year.ToString();
                                        fileYear = fileInfo.CreationTime.Month.ToString("D2");
                                    }

                                    var folderYear = Path.Combine(folders[i], fileYear);
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
                    else
                    {
                        Log($"Nenhum arquivo para processar na pasta {folders[i]}");
                    }
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
            if (cleanTemp)
            {
                string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp\\");
                string winTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp\\");
                string prefetchDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch\\");

                List<string> tempFolders = new List<string>();
                tempFolders.Add(tempFolder);
                tempFolders.Add(winTemp);
                tempFolders.Add(prefetchDir);

                foreach (var tmp in tempFolders)    //Lista as pastas temporárias...
                {
                    try // Tenta excluir cada arquivo da pasta....
                    {
                        var queueTemp = Directory.EnumerateFiles(tmp);
                        foreach (var file in queueTemp)
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch (Exception ex)
                            {
                                Log($"Não foi possível excluir {file}. {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Falha ao excluir arquivos temporários em {tmp}. {ex.Message}");
                    }

                    try // Tenta excluir cada pasta...
                    {
                        var queueTempFolders = Directory.EnumerateDirectories(tmp);
                        foreach (var folder in queueTempFolders)  // Lista cada sub-pasta...
                        {
                            var queueSubFoldersFile = Directory.EnumerateFiles(folder);
                            foreach (var file in queueSubFoldersFile)    // tenta excluir os arquivos em cada sub-pasta...
                            {
                                try
                                {
                                    File.Delete(file);
                                }
                                catch (Exception ex)
                                {
                                    Log($"Falha ao excluir o arquivo {file} em {folder}. {ex.Message}");
                                }
                            }
                            try //Após excluir cada arquivo da sub-pasta, tenta excluir a pasta...
                            {
                                Directory.Delete(folder);
                            }
                            catch (Exception ex)
                            {
                                Log($"Falha ao excluir o diretório {folder}. {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Falha ao excluir pastas em {tmp}");
                    }
                }
            }
        }

        public void Log(string message, bool special = false)
        {
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
