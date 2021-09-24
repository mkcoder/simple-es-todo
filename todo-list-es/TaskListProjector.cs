using System;
using simple_es_core;

namespace todo_list_es
{
    public class TaskListProjector : Projector<TodoList>
    {
        public TaskListProjector(EventStore eventStore) : base(eventStore)
        {
        }

        private void Accept(TodoListCreated evt)
        {
            model.AggregateId = evt.Id;
            model.ListName = evt.ListName;
        }

        private void Accept(TaskAdded evt)
        {
            var task = new TodoItems();
            task.TaskName = evt.TaskName;
            task.TaskId = evt.TaskId;
            task.Notes = evt.Notes;
            task.Completed = evt.Completed;
            model.Tasks.Add(task);
        }

        private void Accept(TaskRemoved evt)
        {
            var task = model.Tasks.Find(p => p.TaskId == evt.TaskId);
            if (task != null)
            {
                model.Tasks.Remove(task);
            }
        }
    }
}
