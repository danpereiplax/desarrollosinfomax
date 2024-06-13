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
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("yyyyMMdd"));
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string logFilePath = Path.Combine(folderPath, "Resultado.log");

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
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("yyyyMMdd"));
            string checkFilePath = Path.Combine(folderPath, "first_run_check.txt");

            if (File.Exists(checkFilePath))
                return false;

            using (StreamWriter sw = new StreamWriter(checkFilePath))
            {
                sw.WriteLine($"{DateTime.Now}: First run today.");
            }

            return true;
        }
    }
}
