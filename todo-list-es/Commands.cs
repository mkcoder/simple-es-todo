using System;
using simple_es_core;

namespace todo_list_es
{
    public interface ICommand { }


    public class QuitCommand : ICommand { }
    public class RedrawCommand : ICommand { }
    public class AdminCommand : ICommand { }
    public class SwitchCommand : ICommand
    {
        public ViewMode ViewMode { get; set; }
    }
    public class CreateTodoListCommand : ICommand
    {
        public string Name { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
    }
    public class AddTaskCommand : ICommand
    {
        public string TaskName { get; set; }
        public Guid AggregateId { get; set; }
    }
    public class RemoveTaskCommand : ICommand
    {
        public int Id { get; set; }
        public Guid AggregateId { get; set; }
    }
    public class TodoListCreated : IEvent
    {
        public string EventName { get; set; }
        public Guid Id { get; set; }
        public string ListName { get; set; }
    }
    public class TaskAdded : IEvent
    {
        public string EventName { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public string Notes { get; set; }
        public bool Completed { get; set; }
    }
    public class TaskRemoved : IEvent
    {
        public string EventName { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public int TaskId { get; set; }
    }
}
