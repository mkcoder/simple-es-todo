using simple_es_core;

namespace todo_list_es
{
    public class TaskListNameProjector : Projector<TaskList>
    {
        public TaskListNameProjector(EventStore eventStore) : base(eventStore)
        {
        }

        private void Accept(TodoListCreated evt)
        {
            model.AggregateId = evt.Id;
            model.ListName = evt.ListName;
        }
    }
}
