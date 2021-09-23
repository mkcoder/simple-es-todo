using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ConsoleTables;
using simple_es_core;

namespace todo_list_es
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.TurnOnDebug();
            Display.ClearScreen();
            Display.PrintScreen(ViewMode.MainMenu);
            var ch = new CommandHandler();
            while (true)
            {
                var input = Display.GetInput();
                var command = StringToCommandAdapter.GetCommand(input);
                ch.Handle(command);
                ch.Handle(new RedrawCommand());
            }
        }
    }

    public enum ViewMode
    {
        MainMenu,
        ListTodoList,
        CreateTodoItem,
        Admin
    }

    public static class Logger
    {
        public static bool DebugMode { get; private set; }

        public static void Log(string message, object o=null)
        {
            if (DebugMode)
            {
                if (o != null)
                {
                    Log(o, message);
                }
                else
                {
                    Log(message);
                }
            }
        }

        internal static void TurnOnDebug()
        {
            DebugMode = true;
        }

        internal static void TurnOffDebug()
        {
            DebugMode = false;
        }

        private static void Log(string message) => Console.WriteLine(message);
        private static void Log(object o, string message) => Console.WriteLine($"{message} {o.ToString()}");
    }

    public static class StringToCommandAdapter
    {
        public static ICommand GetCommand(string input)
        {
            var command = input.Split(" ")[0].ToUpperInvariant();

            if (command.StartsWith("M"))
            {
                return new SwitchCommand()
                {
                    ViewMode = ViewMode.MainMenu
                };
            }

            if (Display.ViewMode == ViewMode.MainMenu)
            {
                if (command.StartsWith("Q"))
                {
                    return new QuitCommand();
                }
                else if (command.StartsWith("C"))
                {
                    Display.SwitchViewMode(ViewMode.CreateTodoItem);
                    return new CreateTodoListCommand()
                    {
                        Name = string.Join(" ", input.Split(" ").Skip(1).ToList())
                    };
                }
                else if (command.StartsWith("A"))
                {
                    var key = input.Split(" ")[1];
                    if (key == "abc123")
                    {
                        return new SwitchCommand()
                        {
                            ViewMode = ViewMode.Admin
                        };
                    }
                }
            }

            if (Display.ViewMode == ViewMode.CreateTodoItem)
            {
                if (command.StartsWith("ADD", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new AddTaskCommand()
                    {
                        TaskName = string.Join(" ", input.Split(" ").Skip(1).ToList())
                    };
                }
                else if (command.StartsWith("REMOVE", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new RemoveTaskCommand()
                    {
                        Id = int.Parse(input.Split(" ")[1])
                    };
                }
            }

            if (Display.ViewMode == ViewMode.Admin)
            {
                if (command.StartsWith("D0", StringComparison.InvariantCultureIgnoreCase))
                {
                    Logger.TurnOffDebug();
                }
                else if (command.StartsWith("D1", StringComparison.InvariantCultureIgnoreCase))
                {
                    Logger.TurnOnDebug();
                }
                else if (command.StartsWith("E", StringComparison.InvariantCultureIgnoreCase))
                {
                    Display.RenderEventStore(new EventStore());
                }

                return new AdminCommand() { };
            }
            return null;
        }
    }

    public static class Display
    {
        public static ViewMode ViewMode { get; private set; }

        public static string GetInput()
        {
            Console.WriteLine("User Input: ");
            var key = Console.ReadLine();
            Logger.Log($"User input: '{key}'");
            return key;
        }

        public static void ClearScreen()
        {
            Console.Clear();
        }

        public static void PrintScreen(ViewMode viewMode)
        {
            ViewMode = viewMode;
            Logger.Log($"New ViewMode: {viewMode}");
            switch (viewMode)
            {
                case ViewMode.MainMenu:
                    Console.WriteLine("Welcome to a simple todo app:");
                    Console.WriteLine("You can create multiple todo list, add todo items and delete. ");
                    Console.WriteLine("[A {key}] - You can view admin mode.");
                    Console.WriteLine("[C] - Create Todo List");
                    Console.WriteLine("[S] - Switch Todo List");
                    Console.WriteLine("[Q] - Quit");
                    break;
                case ViewMode.ListTodoList:
                    Console.WriteLine("[Use {todo-list}] - Change to this list");
                    Console.WriteLine("[M] - Go back to the main menu");
                    break;
                case ViewMode.CreateTodoItem:
                    Console.WriteLine("[Add {todo}] - Add todo item");
                    Console.WriteLine("[Remove {id}] - Remove todo item");
                    Console.WriteLine("[M] - Go back to the main menu");
                    break;
                case ViewMode.Admin:
                    Console.WriteLine("[D1] - Turn on debug mode");
                    Console.WriteLine("[D0] - Turn off debug mode");
                    Console.WriteLine("[E] - Explore EventStore");
                    Console.WriteLine("[M] - Go back to the main menu");
                    break;
                default:
                    break;
            }
        }

        public static void TableTodoItem(TodoList todoList)
        {
            ConsoleTable table = new ConsoleTable(new string[] { "Id", "Task Name", "Notes", "Completed" });
            foreach (var item in todoList.Tasks)
            {
                table.AddRow(item.TaskId, item.TaskName, item.Notes, item.Completed);
            }
            Console.WriteLine($"Title: {todoList.ListName}");
            table.Write();
        }

        public static void SwitchViewMode(ViewMode viewMode)
        {
            ViewMode = viewMode;
        }

        internal static void RenderEventStore(EventStore eventStore)
        {
            foreach (var kv in eventStore.Store)
            {
                Console.WriteLine($"Stream Name: {kv.Key}");
                ConsoleTable table = new ConsoleTable(new string[] { "EventName", "Id", "Data" });
                foreach (var item in kv.Value)
                {
                    table.AddRow(item.Id, item.EventName, item);
                }
                table.Write();
            }
        }
    }

    public class TodoList : IModel
    {
        public Guid AggregateId { get; set; }
        public string ListName { get; set; }
        public List<TodoItems> Tasks { get; set; } = new List<TodoItems>();
    }

    public class TodoItems
    {
        public string EventName { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public string Notes { get; set; }
        public bool Completed { get; set; }
    }
}
