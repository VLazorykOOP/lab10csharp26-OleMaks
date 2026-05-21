using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab10
{

    public class FacultyEventArgs : EventArgs
    {
        public string EventName { get; set; }
        public int Priority { get; set; } 
        public int Impact { get; set; } 

        public FacultyEventArgs(string name, int priority, int impact)
        {
            EventName = name;
            Priority = priority;
            Impact = impact;
        }
    }

    public delegate Task FacultyEventHandler(object sender, FacultyEventArgs e);


    public static class Statistics
    {
        public static int StudentHappiness = 50; 
        public static int TeacherEnergy = 100;
        public static int EventsPassed = 0;

        public static void PrintStats(int day)
        {
            Console.WriteLine($"\nСтатистика Факультету (День {day})");
            Console.WriteLine($"Настрій студентів: {StudentHappiness}%");
            Console.WriteLine($"Енергія викладачів: {TeacherEnergy}%");
            Console.WriteLine($"Проведено заходів: {EventsPassed}");
            Console.WriteLine("--------------------------------------\n");
        }
    }


    public class Student
    {
        public async Task ReactToEventAsync(object sender, FacultyEventArgs e)
        {
            await Task.Delay(100);

            if (e.EventName == "День факультету")
            {
                Statistics.StudentHappiness += e.Impact;
                if (Statistics.StudentHappiness > 100) Statistics.StudentHappiness = 100;
                Console.WriteLine($"[Студенти] Святкують {e.EventName}! Настрій зріс на {e.Impact}.");
            }
            else if (e.EventName == "Складний іспит")
            {
                Statistics.StudentHappiness -= e.Impact;
                if (Statistics.StudentHappiness < 0) Statistics.StudentHappiness = 0;
                Console.WriteLine($"[Студенти] Паніка! Пишуть {e.EventName}. Настрій впав на {e.Impact}.");
            }
            else
            {
                Console.WriteLine($"[Студенти] Відвідують {e.EventName}.");
            }
        }
    }

    public class Teacher
    {
        public async Task ReactToEventAsync(object sender, FacultyEventArgs e)
        {
            await Task.Delay(150);

            if (e.EventName == "День факультету")
            {
                Statistics.TeacherEnergy -= (e.Impact / 2);
                Console.WriteLine($"[Викладачі] Наглядають за студентами на {e.EventName}. Енергія трохи впала.");
            }
            else if (e.EventName == "Складний іспит")
            {
                Statistics.TeacherEnergy -= e.Impact;
                Console.WriteLine($"[Викладачі] Перевіряють роботи після події '{e.EventName}'. Сильно втомилися.");
            }
            else
            {
                Console.WriteLine($"[Викладачі] Проводять {e.EventName}.");
            }
        }
    }

    public class Faculty
    {

        public event FacultyEventHandler FacultyEvent;

        private List<FacultyEventArgs> eventQueue = new List<FacultyEventArgs>();

        public void PlanEvent(string name, int priority, int impact)
        {
            eventQueue.Add(new FacultyEventArgs(name, priority, impact));
            Console.WriteLine($"[Розклад] Заплановано: {name} (Пріоритет: {priority})");
        }


        public async Task ProcessEventsAsync()
        {
            var sortedEvents = eventQueue.OrderBy(e => e.Priority).ToList();
            eventQueue.Clear();

            foreach (var ev in sortedEvents)
            {
                Console.WriteLine($"\n>>> РОЗПОЧАЛАСЯ ПОДІЯ: {ev.EventName} <<<");
                Statistics.EventsPassed++;

                if (FacultyEvent != null)
                {

                    var delegates = FacultyEvent.GetInvocationList();
                    List<Task> reactionTasks = new List<Task>();

                    foreach (FacultyEventHandler handler in delegates)
                    {
                        reactionTasks.Add(handler(this, ev));
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

            Faculty decanate = new Faculty();
            Student students = new Student();
            Teacher teachers = new Teacher();


            decanate.FacultyEvent += students.ReactToEventAsync;
            decanate.FacultyEvent += teachers.ReactToEventAsync;


            for (int day = 1; day <= 2; day++)
            {
                Console.WriteLine($"\n================ ДЕНЬ {day} ================");
                
                if (day == 1)
                {

                    decanate.PlanEvent("Звичайна лекція", priority: 3, impact: 5);
                    decanate.PlanEvent("Складний іспит", priority: 1, impact: 30);
                }
                else if (day == 2)
                {

                    decanate.PlanEvent("День факультету", priority: 2, impact: 40);
                }

                await decanate.ProcessEventsAsync();

                Statistics.PrintStats(day);
                
                await Task.Delay(1500);
            }

            Console.WriteLine("Навчальний рік завершено!");
        }
    }
}