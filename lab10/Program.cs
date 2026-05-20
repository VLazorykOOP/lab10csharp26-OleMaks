using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab10
{
    public class ForestEventArgs : EventArgs
    {
        public string EventName { get; set; }
        public int Priority { get; set; } 
        public int Value { get; set; }

        public ForestEventArgs(string name, int priority, int value)
        {
            EventName = name;
            Priority = priority;
            Value = value;
        }
    }

    public delegate Task ForestEventHandler(object sender, ForestEventArgs e);

    public static class Statistics
    {
        public static int TotalTrees = 100;
        public static int TotalAnimals = 50;
        public static int FiresSurvived = 0;
        public static int Rains = 0;

        public static void PrintStats(int day)
        {
            Console.WriteLine($"\nСтатистика за День {day}");
            Console.WriteLine($"Дерев у лісі: {TotalTrees}");
            Console.WriteLine($"Тварин у лісі: {TotalAnimals}");
            Console.WriteLine($"Пережито пожеж: {FiresSurvived}");
            Console.WriteLine($"Кількість дощів: {Rains}");
            Console.WriteLine("--------------------------------\n");
        }
    }

    public class Tree
    {
        public async Task OnWeatherChanged(object sender, ForestEventArgs e)
        {
            await Task.Delay(100);
            if (e.EventName == "Дощ")
            {
                Statistics.TotalTrees += e.Value;
                Console.WriteLine($"[Дерева] Пішов дощ! Виросло нових дерев: {e.Value}.");
            }
            else if (e.EventName == "Пожежа")
            {
                int lost = e.Value * 2;
                Statistics.TotalTrees -= lost;
                if (Statistics.TotalTrees < 0) Statistics.TotalTrees = 0;
                Console.WriteLine($"[Дерева] ПОЖЕЖА! Згоріло дерев: {lost}.");
            }
        }
    }

    public class Animal
    {
        public async Task OnWeatherChanged(object sender, ForestEventArgs e)
        {
            await Task.Delay(150);
            if (e.EventName == "Дощ")
            {
                Statistics.TotalAnimals += e.Value / 2;
                Console.WriteLine($"[Тварини] Є вода і їжа. Народилося тварин: {e.Value / 2}.");
            }
            else if (e.EventName == "Пожежа")
            {
                int lost = e.Value;
                Statistics.TotalAnimals -= lost;
                if (Statistics.TotalAnimals < 0) Statistics.TotalAnimals = 0;
                Console.WriteLine($"[Тварини] Тікають від пожежі! Загинуло тварин: {lost}.");
            }
        }
    }

    public class Nature
    {
        public event ForestEventHandler WeatherEvent;

        private List<ForestEventArgs> eventQueue = new List<ForestEventArgs>();

        public void AddEventToQueue(string name, int priority, int value)
        {
            eventQueue.Add(new ForestEventArgs(name, priority, value));
            Console.WriteLine($"[Прогноз] Заплановано: {name} (Пріоритет: {priority})");
        }

        public async Task ProcessEventsAsync()
        {
            var sortedEvents = eventQueue.OrderBy(e => e.Priority).ToList();
            eventQueue.Clear();

            foreach (var ev in sortedEvents)
            {
                Console.WriteLine($"\n>>> НАСТАЛА ПОДІЯ: {ev.EventName} <<<");
                
                if (ev.EventName == "Пожежа") Statistics.FiresSurvived++;
                if (ev.EventName == "Дощ") Statistics.Rains++;

                if (WeatherEvent != null)
                {
                    Delegate[] subscribers = WeatherEvent.GetInvocationList();
                    List<Task> reactionTasks = new List<Task>();

                    foreach (ForestEventHandler subscriber in subscribers)
                    {
                        reactionTasks.Add(subscriber(this, ev));
                    }

                    await Task.WhenAll(reactionTasks);
                }
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Nature nature = new Nature();
            Tree trees = new Tree();
            Animal animals = new Animal();


            nature.WeatherEvent += trees.OnWeatherChanged;
            nature.WeatherEvent += animals.OnWeatherChanged;

            // Імітація життя лісу протягом 3 днів
            for (int day = 1; day <= 3; day++)
            {
                Console.WriteLine($"\n================ ДЕНЬ {day} ================");
                
                if (day == 1)
                {
                    nature.AddEventToQueue("Дощ", priority: 2, value: 20);
                    nature.AddEventToQueue("Легкий вітерець", priority: 3, value: 0);
                }
                else if (day == 2)
                {
                    nature.AddEventToQueue("Дощ", priority: 2, value: 30);
                    nature.AddEventToQueue("Пожежа", priority: 1, value: 25); 
                }
                else if (day == 3)
                {
                    nature.AddEventToQueue("Рясний дощ", priority: 2, value: 40);
                }

                await nature.ProcessEventsAsync();

                Statistics.PrintStats(day);
                
                await Task.Delay(1500); 
            }

            Console.WriteLine("Симуляція успішно завершена!");
        }
    }
}