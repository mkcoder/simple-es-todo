using System;
using System.Linq;
using System.Reflection;
using simple_es_core;

namespace todo_list_es
{
    public class CommandHandler
    {
        private readonly EventStore eventStore = new EventStore();
        private readonly TodoAggregate aggregate;
        public CommandHandler()
        {
            aggregate = new TodoAggregate(eventStore);
        }

        public void Handle(ICommand command)
        {
            var mi = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var method in mi)
            {
                var p = method.GetParameters();
                if (p.Any(p => p.ParameterType == command.GetType()))
                {
                    method.Invoke(this, new object[] { command });
                    return;
                }
            }
        }

        private void Accept(QuitCommand quitCommand)
        {
            Logger.Log("Shutting down the program");
            Environment.Exit(-1);
        }

        private void Accept(CreateTodoListCommand command)
        {
            aggregate.CreateNewTodoList(command);
        }

        private void Accept(AddTaskCommand command)
        {
            aggregate.AddTodoItem(command);
        }

        private void Accept(RemoveTaskCommand command)
        {
            aggregate.RemoveTodoItem(command);
        }

        private void Accept(SwitchCommand command)
        {
            Display.ClearScreen();
            Display.PrintScreen(command.ViewMode);
        }

        private void Accept(UseTaskListCommand command)
        {
            aggregate.UseTaskList(command);
            Display.SwitchViewMode(ViewMode.CreateTodoItem);
        }

        private void Accept(RedrawCommand command)
        {
            if (Display.ViewMode == ViewMode.Admin)
            {
                return;
            }
            Display.ClearScreen();
            Display.PrintScreen(Display.ViewMode);
            if (Display.ViewMode == ViewMode.CreateTodoItem)
            {
                aggregate.Redraw(aggregate.AggregateId);
            }

            if (Display.ViewMode == ViewMode.ListTodoList)
            {
                var listProjector = new TaskListNameProjector(new EventStore());
                var model = listProjector.Replay("TodoList");
                Display.DrawList(model);
            }
        }

        private void Accept(AdminCommand command)
        {
        }
    }
}
