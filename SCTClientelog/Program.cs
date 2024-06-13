using System;
using System.IO;
using System.Linq;

namespace SCTClientelog
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Crear archivo de log del aplicativo con formato Resultado-fechadehoy.log
                string logFileName = $"Resultado-{DateTime.Now:yyyyMMdd}.log";
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);

                bool isFirstRun = IsFirstExecutionToday();
                using (StreamWriter sw = new StreamWriter(logFilePath, true))
                {
                    if (isFirstRun)
                    {
                        sw.WriteLine($"{DateTime.Now}: Primera ejecución del día.");
                    }
                    else
                    {
                        sw.WriteLine($"{DateTime.Now}: Ejecución consecutiva.");
                    }
                }

                // Renombrar archivos en C:\SCTCliente
                string sourceFolder = @"C:\SCTCliente";
                string[] filesToRename = { "SCTCliente.log", "SCTCliente.old" };

                foreach (string fileName in filesToRename)
                {
                    string sourceFilePath = Path.Combine(sourceFolder, fileName);
                    if (File.Exists(sourceFilePath))
                    {
                        string newFileName;

                        if (isFirstRun)
                        {
                            string datePrefix = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                            newFileName = $"{datePrefix}-{fileName}";
                        }
                        else
                        {
                            string datePrefix = DateTime.Now.ToString("yyyyMMdd");
                            newFileName = $"{datePrefix}-{fileName}";
                        }

                        string destinationFilePath = GetNextAvailableFileName(Path.Combine(sourceFolder, newFileName));
                        File.Move(sourceFilePath, destinationFilePath);
                    }
                }

                // Eliminar archivos antiguos
                DeleteOldLogs(AppDomain.CurrentDomain.BaseDirectory, 30);
                DeleteOldLogs(sourceFolder, 30);
            }
            catch (Exception ex)
            {
                string errorFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_log.txt");
                using (StreamWriter sw = new StreamWriter(errorFilePath, true))
                {
                    sw.WriteLine($"{DateTime.Now}: Error - {ex.Message}");
                }
            }
        }

        static string GetNextAvailableFileName(string baseFilePath)
        {
            if (!File.Exists(baseFilePath))
                return baseFilePath;

            string directory = Path.GetDirectoryName(baseFilePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFilePath);
            string extension = Path.GetExtension(baseFilePath);

            int count = Directory.GetFiles(directory, $"{fileNameWithoutExtension}*{extension}").Length;
            return Path.Combine(directory, $"{fileNameWithoutExtension}-{count}{extension}");
        }

        static bool IsFirstExecutionToday()
        {
            string firstRunCheckFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "first_run_check.txt");

            if (File.Exists(firstRunCheckFilePath))
            {
                var fileDate = File.GetLastWriteTime(firstRunCheckFilePath);
                if (fileDate.Date == DateTime.Now.Date)
                {
                    return false;
                }
                else
                {
                    File.Delete(firstRunCheckFilePath);
                }
            }

            using (StreamWriter sw = new StreamWriter(firstRunCheckFilePath))
            {
                sw.WriteLine($"{DateTime.Now}: First run today.");
            }

            return true;
        }

        static void DeleteOldLogs(string folderPath, int daysOld)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(folderPath);
                var oldFiles = directoryInfo.GetFiles().Where(f => f.LastWriteTime < DateTime.Now.AddDays(-daysOld)).ToList();
                var oldDirectories = directoryInfo.GetDirectories().Where(d => d.LastWriteTime < DateTime.Now.AddDays(-daysOld)).ToList();

                foreach (var file in oldFiles)
                {
                    Console.WriteLine($"Deleting file: {file.FullName}");
                    file.Delete();
                }

                foreach (var directory in oldDirectories)
                {
                    Console.WriteLine($"Deleting directory: {directory.FullName}");
                    directory.Delete(true);
                }
            }
            catch (Exception ex)
            {
                string errorFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_log.txt");
                using (StreamWriter sw = new StreamWriter(errorFilePath, true))
                {
                    sw.WriteLine($"{DateTime.Now}: Error al eliminar archivos antiguos - {ex.Message}");
                }
            }
        }
    }
}