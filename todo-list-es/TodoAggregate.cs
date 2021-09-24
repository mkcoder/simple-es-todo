using System;
using System.Linq;
using simple_es_core;

namespace todo_list_es
{
    public class TodoAggregate
    {
        public Guid AggregateId { get; private set; }
        private EventStore eventStore;
        private TaskListProjector taskListProjector;
        public TodoAggregate(EventStore eventStore)
        {
            this.eventStore = eventStore;
            this.taskListProjector = new TaskListProjector(this.eventStore);
        }

        public void CreateNewTodoList(CreateTodoListCommand command)
        {
            AggregateId = command.Id;
            eventStore.CreateStream($"TodoList.{command.Id}");
            var todoListCreated = new TodoListCreated()
            {
                EventName = nameof(TodoListCreated),
                Id = command.Id,
                ListName = command.Name
            };
            eventStore.StoreEvent($"TodoList.{command.Id}", todoListCreated);
        }

        public void AddTodoItem(AddTaskCommand command)
        {
            var model = taskListProjector.Replay("TodoList", AggregateId);
            var maxId = 1;
            if (model.Tasks.Any())
            {
                maxId = model.Tasks.Select(t => t.TaskId).Max() + 1;
            }
            var todoListCreated = new TaskAdded()
            {
                EventName = nameof(TaskAdded),
                TaskId = maxId,
                TaskName = command.TaskName,
                Completed = false
            };
            eventStore.StoreEvent($"TodoList.{AggregateId}", todoListCreated);
        }


        public void RemoveTodoItem(RemoveTaskCommand command)
        {
            var todoListRemoved = new TaskRemoved()
            {
                EventName = nameof(TaskRemoved),
                TaskId = command.Id
            };
            eventStore.StoreEvent($"TodoList.{AggregateId}", todoListRemoved);
        }

        internal void UseTaskList(UseTaskListCommand command)
        {
            AggregateId = command.AggregateId;
        }

        public void Redraw(Guid aggregateId)
        {
            var model = taskListProjector.Replay("TodoList", aggregateId);
            Display.TableTodoItem(model);
        }
    }
}
