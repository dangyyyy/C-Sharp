using System;

class Person
{
    public int Age { get; private set; }
    public string Name { get; private set; }

    public Person(int age, string name)
    {
        Age = age;
        Name = name;
    }

    public void DisplayInfo()
    {
        Console.WriteLine($"Имя: {Name}, Возраст: {Age}");
    }
}

class Program
{
    static void Main()
    {
        Console.Write("Введите имя: ");
        string name = Console.ReadLine();
        
        while (string.IsNullOrWhiteSpace(name))
        {
            Console.Write("Имя не может быть пустым. Введите имя: ");
            name = Console.ReadLine();
        }

        int age;
        Console.Write("Введите возраст: ");
        while (!(int.TryParse(Console.ReadLine(), out age) && age >= 0 && age <= 150))
        {
            Console.Write("Некорректный возраст. Введите возраст (0-150): ");
        }

        Person person = new Person(age, name);
        person.DisplayInfo();
    }
}
