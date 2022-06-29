using System;

namespace FileManager
{
    class Program
    {
        // Класс файлого менеджера, работа с комадами.
        private static ConsoleCommand s_console = new();


        static void Main(string[] args)
        {
            bool stop = false;
            PrintIntroduction();
            while (!stop)
                Start(out stop);
            Console.WriteLine("Stopped");
        }

        /// <summary>
        /// Запуск работы с файловым менеджером
        /// </summary>
        /// <param name="stop">Логическа переменная указывающая на остановку работы с программой.</param>
        static void Start(out bool stop)
        {
            string input;
            string error = "";
            do
            {
                if (error == "")
                {
                    Console.Write(s_console.Dir.TrimEnd() + ">");
                    input = Console.ReadLine();
                    error = "Неправильная команда, введите \"help\" для вывода списка команд";
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(error);
                    Console.Write(s_console.Dir.TrimEnd() + ">");
                    input = Console.ReadLine();
                }
            } while (!IsCommand(input));
            s_console.Command = input;

            s_console.Execute();
            Console.WriteLine();
            stop = s_console.IsStopped;
        }

        /// <summary>
        /// Проверка введенного значения на правильность написания.
        /// </summary>
        /// <param name="input">Введенная строка в консоле.</param>
        /// <returns></returns>
        public static bool IsCommand(string input)
        {
            string command = input.Split(' ', 2)[0];
            string attr;
            try
            {
                attr = input.Split(' ', 2)[1];
            }
            catch (Exception)
            {
                attr = null;
            }
            return command switch
            {
                "cd" => true,
                "ls" => (attr == "drives" || attr == null) && true,
                "help" => attr == null,
                "cat" => attr != null,
                "cp" => attr != null,
                "mv" => attr != null,
                "rm" => attr != null,
                "mk" => attr != null,
                "cn" => attr != null,
                "man" => attr != null,
                "exit" => attr == null,
                _ => false,
            };
        }

        /// <summary>
        /// Вывод списка всех доступных команд с небольшим описанием.
        /// </summary>
        public static void PrintAllCommands()
        {
            Console.WriteLine(" 'cd <diectories>' - Смена текущей дериктории;\n" +
                              " 'ls <(optional)drives>' - Вывод всех файлов в текущей директории или всех дисков;\n"+
                              " 'cat <fileName> <(optional)encoding>' - Вывод содержимого файла;\n"+
                              " 'cp <fileName> <newFileName>' - Копирование файла в указанную директорию;\n"+
                              " 'mv <fileName> <destinationDirectory>' - Перемещение файла в указанную директорию;\n"+
                              " 'rm <fileName>' - Удаления файла в указанной директории;\n"+
                              " 'mk <fileName>' - Создание файла в указанной директории;\n"+
                              " 'cn <[fileNames]> <outpuFileName>' - конкатенация содержимого двух или более текстовых файлов и вывод результата в консоль в кодировке UTF - 8;\n" +
                              " 'man <command>' - Информация о команде с примерами;\n"+
                              " 'exit' - Выход из файлового менеджера;\n"+
                              " 'help' - Вывод списка команд;");
        }

        /// <summary>
        /// Вывод при запуске программы.
        /// </summary>
        static void PrintIntroduction()
        {
            Console.WriteLine("FileManager");
            Console.WriteLine("Для отображения списка команд введите help");
        }

        


    }
}
