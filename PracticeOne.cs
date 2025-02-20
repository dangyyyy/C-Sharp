using System;

class GroupSchedule
{
    public int WeeklyHours;
    public string GroupName;

    public GroupSchedule(int weeklyHours, string groupName)
    {
        WeeklyHours = weeklyHours;
        GroupName = groupName;
    }

    public void DisplayInfo()
    {
        Console.WriteLine($"Группа: {GroupName}");
        Console.WriteLine($"Количество учебных часов в неделю: {WeeklyHours}");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Введите название группы: ");
        string groupName = Console.ReadLine().Trim(); 

        while (string.IsNullOrEmpty(groupName))
        {
            Console.WriteLine("Ошибка! Название группы не может быть пустым. Попробуйте снова: ");
            groupName = Console.ReadLine().Trim(); 
        }

        Console.Write("Введите количество учебных часов в неделю: ");
        int hours;
        while (!int.TryParse(Console.ReadLine(), out hours) || hours < 0)
        {
            Console.WriteLine("Ошибка! Введите корректное количество часов (целое число, большее или равное 0): ");
        }

        GroupSchedule schedule = new GroupSchedule(hours, groupName);
        schedule.DisplayInfo();
    }
}
