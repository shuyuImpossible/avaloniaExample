using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace avaloniaExample.ViewModels.SplitViewPane;

public partial class InsertDragAndDropPageViewModel : ViewModelBase
{
    public const string CustomFormat = "task-item-format";
    private int _count;

    [ObservableProperty] private ObservableCollection<TaskItem> _todoTasks = [
        new TaskItem("TicketStatic0", "Clean"),
        new TaskItem("TicketStatic1", "Gifts"),
    ];

    [ObservableProperty] private ObservableCollection<TaskItem> _doneTasks = [];
    [ObservableProperty] private TaskItem? _draggingTaskItem;

    [RelayCommand]
    private void AddTask()
    {
        var id = $"Task{++_count}";
        //TodoTasks.Add(new TaskItem(id, id));
        TodoTasks.Insert(0,new TaskItem(id, id));
    } 

    private void ReplaceTask(TaskItem taskItem)
    {
        // Find the index of the task with the same title in the TodoTasks collection
        var todoTask = TodoTasks.FirstOrDefault(t => t.Title == taskItem.Title);
        var index = todoTask != null ? TodoTasks.IndexOf(todoTask) : -1;
        if (index != -1)
        {
            // Replace the task at the same position
            TodoTasks[index] = taskItem;
        }
        else
        {
            // Find the index of the task with the same title in the DoneTasks collection
            var doneTask = DoneTasks.FirstOrDefault(t => t.Title == taskItem.Title);
            index = doneTask != null ? DoneTasks.IndexOf(doneTask) : -1;
            if (index != -1)
            {
                // Replace the task at the same position
                DoneTasks[index] = taskItem;
            }
        }
    }

    public void StartDrag(TaskItem taskItem)
    {
        DraggingTaskItem = taskItem;
        taskItem.Background = "Red";
    }

    public void EndDrag()
    {
        if (DraggingTaskItem is not null)
        {
            DraggingTaskItem.Background = "White";
            DraggingTaskItem = null;
        }
    }

    public void AddItem()
    {
        if (DraggingTaskItem is null) return;
        if (DraggingTaskItem.Status == "todo")
        {
            TodoTasks.Add(DraggingTaskItem);
        }

        PrintAllTodoItems("AddItem");
    }

    public void RemoveItem()
    {
        if (DraggingTaskItem is null) return;
        if (DraggingTaskItem.Status == "todo")
        {
            TodoTasks.Remove(DraggingTaskItem);
        }
    }

    public void InsertBeforeItem(string title)
    {
        if (DraggingTaskItem is null) return;
        if (DraggingTaskItem.Status == "todo")
        {
            if (DraggingTaskItem.Title == title) return;

            var task = TodoTasks.FirstOrDefault(t => t.Title == title);
            if (task != null)
            {
                var index = TodoTasks.IndexOf(task);
                Console.WriteLine($"InsertBeforeItem {title} at index {index} time {DateTime.Now}");
                if (index != -1)
                {
                    TodoTasks.Remove(DraggingTaskItem);
                    TodoTasks.Insert(index, DraggingTaskItem);

                    PrintAllTodoItems("InsertBeforeItem");
                }
            }
        }
    }

    public void Drop(TaskItem taskItem, string? destinationListName)
    {
        var sourceList = GetSourceList(taskItem.Status);
        var item = sourceList.SingleOrDefault(t => t.TicketId == taskItem.TicketId);
        if (item is null)
        {
            Console.WriteLine($"Task with id '{taskItem.TicketId}' not found");
            return;
        }

        var destination = GetDestinationList(taskItem.Status);

        if (destination.ListName != destinationListName)
        {
            Console.WriteLine($"Invalid drop location '{destinationListName}'. Valid location is {destination.ListName}");
            return;
        }

        sourceList.Remove(item);
        var updatedItem = item.UpdateStatus(destination.Status);
        destination.List.Add(updatedItem);
        Console.WriteLine($"Moving task '{taskItem.TicketId}' from '{item.Status}' to '{updatedItem.Status}'");
    }

    public bool IsDestinationValid(TaskItem taskItem, string? destinationName)
    {
        var destination = GetDestinationList(taskItem.Status);
        return destination.ListName == destinationName;
    }

    private ObservableCollection<TaskItem> GetSourceList(string status)
    {
        return status switch
        {
            "todo" => TodoTasks,
            "done" => DoneTasks,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    private (ObservableCollection<TaskItem> List, string ListName, string Status) GetDestinationList(string status)
    {
        return status switch
        {
            "todo" => (DoneTasks, "DoneItems", "done"),
            "done" => (TodoTasks, "TodoItems", "todo"),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    private void PrintAllTodoItems(string prefix)
    {
        Console.WriteLine($"TodoTasks: {prefix}");
        foreach (var task in TodoTasks)
        {
            Console.WriteLine($"- {task.Title} (ID: {task.TicketId}, Status: {task.Status}, Background: {task.Background})");
        }
    }
}
