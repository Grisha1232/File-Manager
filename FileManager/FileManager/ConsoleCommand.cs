using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FileManager
{
    /// <summary>
    /// Класс отвечающий за работу с файлами и директориями.
    /// </summary>
    public class ConsoleCommand
    {
        // Строка содержащая команду.
        private string command;
        // Строка содержащая доп. атрибуты команды.
        private string commandAttributes;
        // Стартовый диск.
        private string startDir = Environment.OSVersion.ToString().Split(' ')[0] == "Unix" ? "/" : DriveInfo.GetDrives()[0].ToString();
        // Текущая директория.
        private string currentDir;
        // Проверка на окончание работы с файлами.
        private bool stopExecute = false;
        // Список всех дисков в системе.
        private string[] drives = ListDrives();

        /// <summary>
        /// Конструктор.
        /// Заводит дефолтные переменные.
        /// </summary>
        public ConsoleCommand()
        {
            command = "";
            currentDir = startDir;
            commandAttributes = null;
        }

        /// <summary>
        /// Выводит все переменные.
        /// Записывает команду и ее атрибуты если есть.
        /// </summary>
        public string Command
        {
            get
            {
                string output = "Command: " + command + " Attr: " + commandAttributes + '\n';
                output += "StartDir: "+ startDir + '\n';
                output += "currentDir: " + currentDir + '\n';
                output += "Drive: " + DriveInfo.GetDrives()[0].ToString() + '\n';
                return output;
            }
            set
            {
                try
                {
                    var commands = value.Split(' ', 2);
                    command = commands[0];
                    if (commands[1] != null)
                        commandAttributes = commands[1];
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Получение текущей директории.
        /// </summary>
        public string Dir
        {
            get { return currentDir; }
        }

        /// <summary>
        /// Получает значение о остановке или продолжение работы.
        /// </summary>
        public bool IsStopped
        {
            get { return stopExecute; }
        }


        /// <summary>
        /// Выполняет введенную команду.
        /// </summary>
        public void Execute()
        {
            switch (command)
            {
                case "cd":
                    SetDirectory(commandAttributes);
                    break;
                case "ls":
                    GetListDir(commandAttributes);
                    break;
                case "help":
                    Program.PrintAllCommands();
                    break;
                case "cat":
                    GetTextFromFile(commandAttributes);
                    break;
                case "cp":
                    CopyFile(commandAttributes);
                    break;
                case "mv":
                    MoveFile(commandAttributes);
                    break;
                case "rm":
                    RemoveFile(commandAttributes);
                    break;
                case "mk":
                    MakeFile(commandAttributes);
                    break;
                case "cn":
                    ConcatenateFiles(commandAttributes);
                    break;
                case "man":
                    PrintMan(commandAttributes);
                    break;
                case "exit":
                    stopExecute = true;
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// Задает текущую директорию указанную в атрибутах команды.
        /// </summary>
        /// <param name="commandAttributes">Атрибуты команды.</param>
        private void SetDirectory(string commandAttributes)
        {
            if (commandAttributes == null)
            {
                currentDir = Path.Join(startDir);
            }
            else if (commandAttributes == "..")
            {
                if (currentDir != startDir && Directory.Exists(Path.Join(Directory.GetParent(currentDir).ToString(), commandAttributes.Substring(2))))
                {
                    currentDir = Path.Join(Directory.GetParent(currentDir).ToString(), commandAttributes[2..]);
                }
                else
                {
                    Console.WriteLine("Директории не существует");
                }
            }
            else if (Array.IndexOf(drives, commandAttributes) != -1)
            {
                currentDir = Path.Join(commandAttributes);
                startDir = Path.Join(commandAttributes);
            }
            else if (Directory.Exists(Path.Join(currentDir, commandAttributes)))
            {
                currentDir = Path.Join(currentDir, commandAttributes);
            }
            else
            {
                Console.WriteLine("Директории не существует");
                this.commandAttributes = null;
            }
            this.commandAttributes = null;
        }


        /// <summary>
        /// Получает список всех поддиректорий в текущей директории.
        /// </summary>
        /// <param name="commandAttributes">Принимает либо null, либо drives.</param>
        private void GetListDir(string commandAttributes)
        {
            if (commandAttributes == "drives")
            {
                foreach (var item in DriveInfo.GetDrives())
                {
                    Array.Resize(ref drives, drives.Length + 1);
                    drives[drives.Length - 1] = item.ToString();
                    Console.WriteLine(item.ToString());
                }
            }
            else
            {
                try
                {
                    foreach (var item in Directory.GetDirectories(currentDir))
                        Console.WriteLine(item.Replace(currentDir, ""));
                    foreach (var item in Directory.GetFiles(currentDir))
                        Console.WriteLine(item.Replace(currentDir, ""));
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("У программы нет доступа к этой папке");
                    this.commandAttributes = null;
                }
                catch (Exception)
                {
                    Console.WriteLine("Неправильный ввод");
                    this.commandAttributes = null;
                }
            }
            this.commandAttributes = null;
        }


        /// <summary>
        /// Получение текста из файла.
        /// </summary>
        /// <param name="commadAttributes">Имя файла или путь к нему.</param>
        private void GetTextFromFile(string commadAttributes)
        {
            var fileName = Regex.Match(commadAttributes, @"^.*\.\w{2,6}");
            string match;
            try
            {
                match = commadAttributes.Split(fileName.Value, 2)[1].TrimStart();
            }
            catch (Exception)
            {
                Console.WriteLine("Неправильный ввод.");
                this.commandAttributes = null;
                return;
            }
            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(match);
            }
            catch (Exception)
            {
                encoding = Encoding.UTF8;
            }
            try
            {
                Console.WriteLine(File.ReadAllText(Path.Join(currentDir, fileName.Value), encoding));
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("У программы нет доступа к этому файлу.");
                this.commandAttributes = null;
            }
            catch (Exception)
            {
                Console.WriteLine("Файл не существует в этой директории");
                this.commandAttributes = null;
            }

            this.commandAttributes = null;
        }


        /// <summary>
        /// Копирует файл из текущей директории в указанную директорию.
        /// </summary>
        /// <param name="commandAttributes">Имя копируемого файла + " " + имя выходного файла.</param>
        private void CopyFile(string commandAttributes)
        {
            var fileName = Regex.Match(commandAttributes, @"^.*\.\w{2,6}\s");
            string fileDestinationInCurrDir;
            try
            {
                fileDestinationInCurrDir = commandAttributes.Split(fileName.Value, 2)[1];
            }
            catch (Exception)
            {
                Console.WriteLine("Неправильная команда. Проверьте правильность указанного пути/файла, и имеет ли доступ программа к это папке/файлу.");
                return;
            }
            var fileDestinationFullPath = Environment.OSVersion.ToString().Split(' ')[0] == "Unix" ?
                                            Regex.Match(commandAttributes, @"\s/?.*/\w*\.\w{2,6}$") :
                                            Regex.Match(commandAttributes, @"[A-Z]:\\.*\.\w{2,6}$");
            try
            {
                if (File.Exists(Path.Join(currentDir, fileName.Value.Trim())) && fileDestinationInCurrDir.Trim() != "" && fileDestinationInCurrDir != fileDestinationFullPath.Value.Trim())
                {
                    File.Copy(Path.Join(currentDir, fileName.Value.Trim()), Path.Join(currentDir, fileDestinationInCurrDir.Trim()), true);
                    Console.WriteLine($"Файл скопирован в {Path.Join(currentDir, fileDestinationInCurrDir.Trim())}");
                }
                else if (File.Exists(Path.Join(currentDir, fileName.Value.Trim())) && fileDestinationFullPath.Value.Trim() != "")
                {
                    File.Copy(Path.Join(currentDir, fileName.Value.Trim()), fileDestinationFullPath.Value.Trim(), true);
                    Console.WriteLine($"Файл скопирован в {fileDestinationFullPath.Value.Trim()}");
                }
                else
                {
                    File.Copy(Path.Join(currentDir, fileName.Value.Trim()), fileDestinationFullPath.Value.Trim());
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Файл не скопирован. Проверьте правильность указанного пути/файла, и имеет ли доступ программа к это папке/файлу.");
            }
            this.commandAttributes = null;
        }

        /// <summary>
        /// Переместить файл из текущей директории в указанную директорию.
        /// </summary>
        /// <param name="commandAttributes">Имя файла из текущей директории + " " + Полный путь куда нужно переместить.</param>
        private void MoveFile(string commandAttributes)
        {
            string fileName = Regex.Match(commandAttributes, @"^\w+\.\w{2,6}\s").Value.Trim();
            string destination = Environment.OSVersion.ToString().Split(' ')[0] == "Unix" ?
                                 Regex.Match(commandAttributes, @"\s/?.*/.*$").Value.Trim() :
                                 Regex.Match(commandAttributes, @"[A-Z]:\\.*$").Value.Trim();
            try
            {
                File.Move(Path.Join(currentDir, fileName), Path.Join(destination, fileName));
                Console.WriteLine($"Файл перемещен в {Path.Join(destination, fileName)}");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Файл не существует.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("У программы нет доступа к этому файлу.");
            }
            catch (Exception)
            {
                Console.WriteLine("Файл не перемещен проверьте правильность указанного пути, и имеет ли программа доcтуп к этой папке.");
            }
            this.commandAttributes = null;
        }

        /// <summary>
        /// Удаление файлал в текущей или указанной директории.
        /// </summary>
        /// <param name="commandAttributes">Имя файла в текущей или указанной директории.</param>
        private void RemoveFile(string commandAttributes)
        {
            string fileNameInCurrDir = commandAttributes;
            var fileNameFullPath = Environment.OSVersion.ToString().Split(' ')[0] == "Unix" ?
                                            Regex.Match(commandAttributes, @"^/?.*/\w*\.\w{2,6}$") :
                                            Regex.Match(commandAttributes, @"^[A-Z]:\\.*\.\w{2,6}$");
            try
            {
                File.Delete(fileNameFullPath.Value);
                Console.WriteLine("Файл удален");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("У программы нет доступа к этому файлу.");
            }
            catch (Exception)
            {
                if (File.Exists(Path.Join(currentDir, fileNameInCurrDir)))
                {
                    File.Delete(Path.Join(currentDir, fileNameInCurrDir));
                    Console.WriteLine("Файл удален");
                }
                else
                {
                    Console.WriteLine("Файл не существует");
                }
            }
            this.commandAttributes = null;
        }

        /// <summary>
        /// Создание файла в текущей или указанной директории.
        /// </summary>
        /// <param name="commandAttributes">Имя файла в текущей или указанной директории.</param>
        private void MakeFile(string commandAttributes)
        {
            var fileNameFullPath = Environment.OSVersion.ToString().Split(' ')[0] == "Unix" ?
                                            Regex.Match(commandAttributes, @"^/?.*/.*?\.\w{2,6}") :
                                            Regex.Match(commandAttributes, @"^[A-Z]:\\.*?\.\w{2,6}");
            var fileNameInCurrDir = Regex.Match(commandAttributes, @"^.*\.\w+");
            var textForFile = Regex.Match(commandAttributes, "\".*\"");
            Encoding encod;
            try
            {
                if (fileNameFullPath.Value != "" &&
                    commandAttributes.Split(fileNameFullPath.Value, 2)[1].Split(textForFile.Value, 2)[0].Trim() != "")
                {
                    encod = Encoding.GetEncoding(commandAttributes.Split(fileNameFullPath.Value, 2)[1].Split(textForFile.Value, 2)[0].Trim());
                }

                else if (fileNameInCurrDir.Value != "" &&
                    commandAttributes.Split(fileNameInCurrDir.Value, 2)[1].Split(textForFile.Value, 2)[0].Trim() != "")
                {
                    encod = Encoding.GetEncoding(commandAttributes.Split(fileNameInCurrDir.Value, 2)[1].Split(textForFile.Value, 2)[0].Trim());
                }

                else
                    encod = Encoding.UTF8;
            }
            catch (Exception)
            {
                Console.WriteLine("Неправильная кодировка.");
                Console.WriteLine("Файл не создан.");
                this.commandAttributes = null;
                return;
            }
            try
            {
                if (fileNameFullPath.Value != "" && !File.Exists(fileNameFullPath.Value))
                {
                    File.WriteAllText(fileNameFullPath.Value, textForFile.Value.Trim('"'), encod);
                    Console.WriteLine($"Файл создан по пути: {fileNameFullPath.Value}");
                }
                else if (fileNameInCurrDir.Value != "" && !File.Exists(Path.Join(currentDir, fileNameInCurrDir.Value)))
                {
                    File.WriteAllText(Path.Join(currentDir, fileNameInCurrDir.Value), textForFile.Value.Trim('"'), encod);
                    Console.WriteLine($"Файл создан по пути: {Path.Join(currentDir, fileNameInCurrDir.Value)}");
                }
                else
                {
                    File.WriteAllText(fileNameFullPath.Value, textForFile.Value.Trim('"'), encod);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("У программы нет доступа к этому файлу.");
                this.commandAttributes = null;
            }
            catch (Exception)
            {
                Console.WriteLine("Файл не создан проверьте правильность указанного пути, и имеет ли дотуп программа к этой папке. " +
                    "Также проверьте правильност написания команды, введите man cp для подробного описания.");
                this.commandAttributes = null;
            }
            this.commandAttributes = null;
        }

        /// <summary>
        /// Конкатенация двух или более файлов в один и вывод этих файлов.
        /// </summary>
        /// <param name="commandAttributes">Список всех файлов, в том числе и выходного.</param>
        private void ConcatenateFiles(string commandAttributes)
        {
            MatchCollection matches = Regex.Matches(commandAttributes, @".*?\.\w{2,6}\s");
            var outputFileName = Regex.Match(commandAttributes, @"\s\w+\.\w{2,6}$");
            string[] filesNames = new string[0];
            foreach (Match item in matches)
            {
                Array.Resize(ref filesNames, filesNames.Length + 1);
                filesNames[filesNames.Length - 1] = item.Value.Trim();
            }

            if (filesNames.Length < 2)
            {
                Console.WriteLine("Неправильный ввод. Введите 'man cn' для просмотра как работает команда.");
                return;
            }


            var newFileString = new StringBuilder();

            foreach (var item in filesNames)
            {
                if (!File.Exists(Path.Join(currentDir, item)))
                {
                    Console.WriteLine($"Файл ({item}) не существует в директории: " + currentDir);
                    return;
                }
                else
                {
                    try
                    {

                        var textFromfile = File.ReadAllText(Path.Join(currentDir, item));
                        newFileString.Append(textFromfile);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine("У программы нет доступа к этому файлу.");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Файл не создан проверьте правильность указанного пути, и имеет ли дотуп программа к этой папке. " +
                            "Также проверьте правильност написания команды, введите man cp для подробного описания.");
                    }
                }
            }
            Console.WriteLine(newFileString);
            Console.WriteLine();
            File.WriteAllText(Path.Join(currentDir, outputFileName.Value.Trim()), newFileString.ToString(), Encoding.UTF8);
            Console.WriteLine($"Файлы сконкатенированы в файл {outputFileName.Value.Trim()}");
        }

        /// <summary>
        /// Вывод мануала по командам.
        /// </summary>
        /// <param name="command">Имя команды.</param>
        private static void PrintMan(string command)
        {
            switch (command)
            {
                case "cd":
                    PrintManCd();
                    break;
                case "ls":
                    PrintManLs();
                    break;
                case "cat":
                    PrintManCat();
                    break;
                case "cp":
                    PrintManCp();
                    break;
                case "mv":
                    PrintManMv();
                    break;
                case "rm":
                    PrintManRm();
                    break;
                case "mk":
                    PrintManMk();
                    break;
                case "cn":
                    PrintManCn();
                    break;
                case "exit":
                    PrintManExit();
                    break;
                case "help":
                    PrintManHelp();
                    break;
                default:
                    Console.WriteLine("Нет такой команды.");
                    break;
            }
        }

        /// <summary>
        /// Вывод мануала по сd.
        /// </summary>
        private static void PrintManCd()
        {
            Console.WriteLine("Описание команды 'cd'\n" +
                              "\n" +
                              "cd <Directory>\n\n" +
                              "Что делает: Смена текущей дериктории.\n" +
                              "\n" +
                              "Пример применения: \n" +
                              "'cd C:\\Users\\User\\Desktop' - Переместит в папку рабочео стола указанного пользователя (User).\n\n" +
                              "'cd C:\\' - Переместит в диск С.\n" +
                              "'cd Users' - Переместит в папку Users.\n" +
                              "'cd User' - Переместит в папку User.\n" +
                              "'cd Desktop' - Переместит в папку Desktop.\n" +
                              "Данные команды также выполнят перемещение в папку рабочего стола указанного пользователя (User).\n\n" +
                              "'cd' - Переместит в корень, то есть если на Windows то в диск С, если на Mac то в /.\n\n" +
                              "'cd ..' - Переместит на директорию выше");
        }

        /// <summary>
        /// Вывод мануала по ls.
        /// </summary>
        private static void PrintManLs()
        {
            Console.WriteLine("Описание команды 'ls'\n" +
                              "\n" +
                              "ls <(optional)drives>\n\n" +
                              "Что делает: Отображение все файлов или дисков системы.\n" +
                              "\n" +
                              "Пример применения: \n" +
                              "'ls' - Отобразит все файлы в текущей папке\n\n" +
                              "'ls drives' - Отобразит все дискы системы");
        }

        /// <summary>
        /// Вывод мануала по cat.
        /// </summary>
        private static void PrintManCat()
        {
            Console.WriteLine("Описание команды 'cat'\n" +
                              "\n" +
                              "cat <fileName> <(optional)encoding>\n" +
                              "По умолчанию encoding: utf-8\n\n" +
                              "Что делает: Выводит содержимое файла.\n" +
                              "\n" +
                              "Пример применения: \n" +
                              "'cat fileName.txt ascii' - Выведет содержимое файла, который находится в текущей директории в кодировке ascii.");
        }

        /// <summary>
        /// Вывод мануала по cp.
        /// </summary>
        private static void PrintManCp()
        {
            Console.WriteLine("Описание команды 'cp'\n" +
                              "\n" +
                              "cp <fileName> <newFileName>\n\n" +
                              "Что делает: Копирует файл в другой или создает его, если его нет.\n" +
                              "\n" +
                              "Пример применения: \n" +
                              "'cp fileName.txt newFileName.txt' - копирует fileName.txt в файл newFileName.txt в текущей директории\n\n" +
                              "'cp fileName.txt \\full\\path\\to\\file.txt' - копирует fileName.txt в файл по пути: \\full\\path\\to\\file.txt");
        }

        /// <summary>
        /// Вывод мануала по mv.
        /// </summary>
        private static void PrintManMv()
        {
            Console.WriteLine("Описание команды 'mv'\n" +
                              "\n" +
                              "mv <fileName> <newFileNameFullPath>\n\n" +
                              "Что делает: Перемещает файл в выбранную папку по полному пути.\n" +
                              "\n" +
                              "Пример применения: \n" +
                              "'mv file.txt \\full\\path\\to\\file' - Переместит файл file.txt в папку по пути: \\full\\path\\to\\file");
        }

        /// <summary>
        /// Вывод мануала по rm.
        /// </summary>
        private static void PrintManRm()
        {
            Console.WriteLine("Описание команды 'rm'\n" +
                              "\n" +
                              "rm <fileName>\n\n" +
                              "Что делает: Удаляет файл в текущей директории или в указанной директории.\n" +
                              "\n" +
                              "Пример применения: \n" +
                              "'rm file.txt' - Удаление файла file.txt в текущей директории.\n" +
                              "'rm \\full\\path\\to\\file.txt' - Удаление файл file.txt в папке по пути: \\full\\path\\to\\file.txt");
        }

        /// <summary>
        /// Вывод мануала по mk.
        /// </summary>
        private static void PrintManMk()
        {
            Console.WriteLine("Описание команды 'mk'\n" +
                              "\n" +
                              "mk <fileName> <(optional)encoding> (optional)\"Text into file\"\n" +
                              "По умолчанию encoding: utf-8\n\n" +
                              "Что делает: Создает файл в текущей или указанной директории.\n" +
                              "\n" +
                              "Пример применения: \n" +
                              "'mk file.txt <encoding> \"text into file\"' - Создает файл file.txt в текущей директории с указаной кодировкой и текстом " +
                              "(можно не указывать кодировку и текст)\n" +
                              "'mk \\full\\path\\to\\file.txt <encoding> \"text into file\"' - Создает файл file.txt в директории по пути: \\full\\path\\to\\file.txt " +
                              "с указаной кодировкой и текстом (можно не указывать кодировку и текст)");
        }

        /// <summary>
        /// Вывод мануала по cn.
        /// </summary>
        private static void PrintManCn()
        {
            Console.WriteLine("Описание команды 'cn'\n" +
                              "\n" +
                              "cn [filesNames] <outputFileName>\n\n" +
                              "Что делает: Конкатенация текста файлов в один файл в текущей директории.\n" +
                              "\n" +
                              "Пример применения: \n" +
                              "'cn [filesNames] outputFileName.txt' - Конкатенирует файлы из [filesNames](два и болле файла указано) и помещает весь текст в outputFileName.txt " +
                              "(Если такого файла нет, то создает его. Если такой файл есть он его перезапишет)\n" +
                              "[filesNames] = example1.txt example2.txt example3.txt ...");
        }

        /// <summary>
        /// Вывод мануала по exit.
        /// </summary>
        private static void PrintManExit()
        {
            Console.WriteLine("Описание команды 'exit'\n" +
                              "\n" +
                              "Что делает: Завершение работы с File manager.");
        }

        /// <summary>
        /// Вывод мануала по help.
        /// </summary>
        private static void PrintManHelp()
        {
            Console.WriteLine("Описание команды 'help'\n" +
                              "\n" +
                              "Что делает: Вывод списка всех команд.");
        }

        /// <summary>
        /// Получение списка всех дисков системы.
        /// </summary>
        private static string[] ListDrives()
        {
            string[] result = Array.Empty<string>();
            foreach (var item in DriveInfo.GetDrives())
            {
                Array.Resize(ref result, result.Length + 1);
                result[^1] = item.ToString();
            }
            return result;
        }
    }
}
