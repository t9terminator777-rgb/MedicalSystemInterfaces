using System;
using System.Collections.Generic;
using System.Text;

internal static class Program
{
    private static void Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        Patient patient = new("Алиса", "111222333", "alice@example.com", "O+");
        List<ISpravka> spravki =
        [
            new Spravka083(patient),
            new SanitaryCard(patient, analysesCount: 3),
            new SpravkaBassein(patient, validMonths: 6)
        ];
        Kassa kassa = new();

        while (true)
        {
            ShowMenu();
            int choice = InputHelper.ReadIntInRange("Выбери пункт: ", 0, 3);
            Console.WriteLine();

            switch (choice)
            {
                case 1:
                    DisplayCertificates(spravki);
                    break;
                case 2:
                    kassa.PrintCheck(spravki);
                    break;
                case 3:
                    ChooseAndSendNotification(patient);
                    break;
                case 0:
                    Console.WriteLine("Программа завершена.");
                    return;
            }

            Console.WriteLine();
        }
    }

    private static void ShowMenu()
    {
        Console.WriteLine("=== Медицинские справки ===");
        Console.WriteLine("1. Показать справки");
        Console.WriteLine("2. Напечатать чек кассы");
        Console.WriteLine("3. Уведомить пациента");
        Console.WriteLine("0. Выход");
    }

    private static void DisplayCertificates(List<ISpravka> spravki)
    {
        foreach (ISpravka spravka in spravki)
        {
            spravka.DisplaySpravkaInfo();
            Console.WriteLine($"К оплате: {spravka.CalculatePrice():F2}");
            Console.WriteLine($"Кратко: {spravka.GetShortInfo()}");
            Console.WriteLine();
        }
    }

    private static void ChooseAndSendNotification(Patient patient)
    {
        Console.WriteLine("Как уведомить пациента?");
        Console.WriteLine("1. SMS");
        Console.WriteLine("2. Email");
        Console.WriteLine("3. Telegram");

        int choice = InputHelper.ReadIntInRange("Выбери способ: ", 1, 3);
        INotifier notifier = choice switch
        {
            1 => new SmsNotifier(),
            2 => new EmailNotifier(),
            _ => new TelegramNotifier()
        };

        notifier.Send(patient, "Ваша справка готова");
    }
}

internal interface ISpravka
{
    string Name { get; }
    Patient Patient { get; }
    decimal Price { get; }

    void DisplaySpravkaInfo();
    decimal CalculatePrice();
    string GetShortInfo();
}

internal sealed class Spravka083 : ISpravka
{
    public string Name => "Справка 083";
    public Patient Patient { get; }
    public decimal Price => 100m;

    public Spravka083(Patient patient)
    {
        Patient = patient;
    }

    public void DisplaySpravkaInfo()
    {
        Console.WriteLine($"Название: {Name}");
        Patient.DisplayInfo();
        Console.WriteLine($"Цена: {Price:F2} (фиксированная)");
    }

    public decimal CalculatePrice() => Price;

    public string GetShortInfo() => $"{Name}: {CalculatePrice():F2}";
}

internal sealed class SanitaryCard : ISpravka
{
    private const decimal PricePerAnalysis = 150m;

    public string Name => "Санитарная книжка";
    public Patient Patient { get; }
    public decimal Price => 300m;
    public int AnalysesCount { get; }

    public SanitaryCard(Patient patient, int analysesCount)
    {
        Patient = patient;
        AnalysesCount = analysesCount;
    }

    public void DisplaySpravkaInfo()
    {
        Console.WriteLine($"Название: {Name}");
        Patient.DisplayInfo();
        Console.WriteLine($"Базовая цена: {Price:F2}");
        Console.WriteLine($"Количество анализов: {AnalysesCount}");
        Console.WriteLine($"Цена анализа: {PricePerAnalysis:F2}");
    }

    public decimal CalculatePrice() => Price + AnalysesCount * PricePerAnalysis;

    public string GetShortInfo() => $"{Name}: {CalculatePrice():F2}";
}

internal sealed class SpravkaBassein : ISpravka
{
    public string Name => "Справка в бассейн";
    public Patient Patient { get; }
    public decimal Price => 200m;
    public int ValidMonths { get; }

    public SpravkaBassein(Patient patient, int validMonths)
    {
        Patient = patient;
        ValidMonths = validMonths;
    }

    public void DisplaySpravkaInfo()
    {
        Console.WriteLine($"Название: {Name}");
        Patient.DisplayInfo();
        Console.WriteLine($"Цена: {Price:F2}");
        Console.WriteLine($"Срок действия: {ValidMonths} мес.");
    }

    public decimal CalculatePrice() => Price;

    public string GetShortInfo() => $"{Name}: {CalculatePrice():F2}";
}

internal sealed class Patient
{
    public string Name { get; }
    public string Phone { get; }
    public string Email { get; }
    public string BloodType { get; }

    public Patient(string name, string phone, string email, string bloodType)
    {
        Name = name;
        Phone = phone;
        Email = email;
        BloodType = bloodType;
    }

    public void DisplayInfo()
    {
        Console.WriteLine($"Пациент: {Name}");
        Console.WriteLine($"Телефон: {Phone}");
        Console.WriteLine($"Email: {Email}");
        Console.WriteLine($"Группа крови: {BloodType}");
    }
}

internal sealed class Kassa
{
    public decimal Total(List<ISpravka> spravki)
    {
        decimal sum = 0m;
        foreach (ISpravka spravka in spravki)
        {
            sum += spravka.CalculatePrice();
        }

        return sum;
    }

    public void PrintCheck(List<ISpravka> spravki)
    {
        Console.WriteLine("=== Чек ===");
        foreach (ISpravka spravka in spravki)
        {
            Console.WriteLine(spravka.GetShortInfo());
        }

        Console.WriteLine($"Итого: {Total(spravki):F2}");
    }
}

internal interface INotifier
{
    void Send(Patient patient, string message);
}

internal sealed class SmsNotifier : INotifier
{
    public void Send(Patient patient, string message)
    {
        Console.WriteLine($"SMS на номер {patient.Phone}: {message}");
    }
}

internal sealed class EmailNotifier : INotifier
{
    public void Send(Patient patient, string message)
    {
        Console.WriteLine($"Email на адрес {patient.Email}: {message}");
    }
}

internal sealed class TelegramNotifier : INotifier
{
    public void Send(Patient patient, string message)
    {
        Console.WriteLine($"Telegram для {patient.Name}: {message}");
    }
}

internal static class InputHelper
{
    public static int ReadIntInRange(string prompt, int minimum, int maximum)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine() ?? string.Empty;

            if (int.TryParse(input, out int value) && value >= minimum && value <= maximum)
            {
                return value;
            }

            Console.WriteLine($"Введи целое число от {minimum} до {maximum}.");
        }
    }
}
