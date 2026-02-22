using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;
using LogAnalyzer.Models;


namespace LogAnalyzer
{
     class Program
    {
        const string inputFile = "events.log"; // логи для анализа
        const string outputFile = "analysis.csv"; // .csv результат программы
        static int NeedUserId = 10;
        static int idLine = 0;
        static int lineNumber = 0;
        static int errorCount = 0;
        static int login = 0;
        static int logout = 0;
        static int error = 0;
        static int purchase = 0;
        static DateTime? firstEventDate = null;
        static DateTime? lastEventDate = null;
        static void Main(string[] args)
        {
            Console.WriteLine("\t\t\t\t*****************************************************************");
            Console.WriteLine("\t\t\t\t**********ББСО-02-24 Луев Борис Вариант 20***********************");
            Console.WriteLine("\t\t\t\t*****************************************************************");
            Console.WriteLine("\t\t\t\t**********Консольное приложение для парсинга JSON логов**********");
            Console.WriteLine("\t\t\t\t*****************************************************************");
            Console.WriteLine();
            Console.WriteLine("\t\t\t\tВыберите режим анализа:");
            Console.WriteLine("\t\t\t\t1. Анализ для пользователя по умолчанию (ID = 10)");
            Console.WriteLine("\t\t\t\t2. Указать свой ID");
            Console.WriteLine("\t\t\t\t3. Выход");
            Console.Write("\t\t\t\tВведите: ");
            string? value = Console.ReadLine();
            Console.WriteLine();
            Console.WriteLine();
            switch (value)
            {
                case "1":
                    NeedUserId = 10;
                    Console.WriteLine($"Выбран ID: {NeedUserId}");
                    Console.WriteLine();
                    break;
                case "2":
                    Console.Write("Введите новый ID (1 - 10) для фильтрации: ");
                    if (int.TryParse(Console.ReadLine(), out int newID))
                    {
                        NeedUserId = newID;
                        Console.WriteLine($"Выбран ID: {NeedUserId}");
                        Console.WriteLine();
                    }
                    else
                    {
                        NeedUserId = 10;
                        Console.WriteLine($"Выбран неправильный ID, будет использован ID: {NeedUserId}");
                        Console.WriteLine();
                    }
                    break;
                case "3":
                    Console.WriteLine("Завершение программы...");
                    return;
                default:
                    NeedUserId = 10;
                    Console.WriteLine($"Неккоректный ввод, будет использован ID: {NeedUserId}");
                    Console.WriteLine();
                    break;
            
            }



            Console.WriteLine($"Текущая папка: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"Ищу файл: {Path.GetFullPath(inputFile)}");
            Dictionary<string, int> eventStats = new Dictionary<string, int>();
            const int bufferSize = 65536;
            try
            {
                using (var fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize))
                {
                    string? line;
                    Console.WriteLine("Файл найден и успешно открыт!");
                    Console.WriteLine();
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        try
                        {
                            ProcessLine(line, eventStats);
                        }
                        catch (JsonException ex)
                        {
                            errorCount++;
                            Console.WriteLine($"Ошибка в строке {lineNumber}: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            Console.WriteLine($"Ошибка в строке {lineNumber}: {ex.Message}");
                        }

                        if (lineNumber % 1000 == 0)
                        {
                            Console.WriteLine($"Прогресс: {lineNumber} строк, ошибок: {errorCount}");
                        }
                    }
                }
                SaveResultCSV(eventStats, outputFile);
                Console.WriteLine($"Диапазон дат событий для пользователя с ID = {NeedUserId}:");
                Console.WriteLine($"Начало - {firstEventDate} || Конец - {lastEventDate}");
                Console.WriteLine();
                Console.WriteLine($"Подсчет событий: login: {login}, logout: {logout}, error: {error}, purchase: {purchase}");
                Console.WriteLine($"Обработано всего {lineNumber} строк, ошибок: {errorCount}");
                Console.WriteLine($"Обработано {idLine} строк для пользователя с ID = {NeedUserId}");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Файл '{inputFile}' не был найден! Завершение программы.");
                Console.WriteLine("Нажмите любую клавишу для закрытия программы...");
                Console.ReadKey();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("Нажмите любую клавишу для закрытия программы...");
                Console.ReadKey();
                return;
            }
        }

        static void ProcessLine(string line, Dictionary<string, int> eventStats)
        {
            int indexSeparator = line.IndexOf(" - ");
            if (indexSeparator == -1)
            {
                errorCount++;
                Console.WriteLine($"Строка {lineNumber} содержит ошибку.");
                return;
            }
            string datePart = line.Substring(0, indexSeparator);
            string jsonPart = line.Substring(indexSeparator + 3);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }; // регистр
            LogEvent? logEvent = JsonSerializer.Deserialize<LogEvent>(jsonPart, options);
            if (logEvent == null )
            {
                Console.WriteLine($"Не удалось десериализовать JSON в строке {lineNumber}");
                return;
            }
            if (logEvent.UserId == NeedUserId && DateTime.TryParse(datePart, out DateTime eventDate))
            {
                idLine++;
                if (logEvent.EventType == "login") { login++; }
                if (logEvent.EventType == "logout") { logout++; }
                if (logEvent.EventType == "error") { error++; }
                if (logEvent.EventType == "purchase") { purchase++; }
                if (firstEventDate == null || eventDate < firstEventDate)
                    firstEventDate = eventDate;
                if (lastEventDate == null || eventDate > lastEventDate)
                    lastEventDate = eventDate;
                string key = $"{logEvent.Ip};{logEvent.EventType}";

                if (eventStats.ContainsKey(key))
                    eventStats[key]++;
                else
                    eventStats[key] = 1;
            }
        }
        
        static void SaveResultCSV(Dictionary<string, int> stats, string file)
        {
            using (var writer = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                writer.WriteLine("IP;EventType;Count");
                if (stats.Count > 0)
                {
                    foreach (var item in stats.OrderBy(k => k.Key))
                    {
                        writer.WriteLine($"{item.Key};{item.Value}");
                    }
                    Console.WriteLine();
                    Console.WriteLine($"Результаты успешно сохранены в файл '{outputFile}'");
                }
                else
                {
                    writer.WriteLine($"Нет данных пользователя с ID = {NeedUserId}");
                    Console.WriteLine($"Данных пользователя с ID = {NeedUserId} не найдено.");
                }
            }
        }
    }
}