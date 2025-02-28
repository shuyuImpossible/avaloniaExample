using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using avaloniaExample.ViewModels.SplitViewPane;

namespace avaloniaExample.Views;

public partial class InsertDragAndDropPageView : UserControl
{
    private Point _ghostPosition = new(0,0);
    private readonly Point _mouseOffset = new(-5, -5);
    private bool _isInDragDrop = false; 

    public InsertDragAndDropPageView()
    {
        InitializeComponent();

        AddHandler(DragDrop.DragOverEvent, DragOver);
        AddHandler(DragDrop.DropEvent, Drop);

        this.PointerMoved += OnPointerMoved;
        MainContainer.PointerEntered += OnPointerEntered;
        MainContainer.PointerExited += OnPointerExited;
    }

    // OnInitialized didn't work for some reason
    protected override void OnLoaded(RoutedEventArgs e)
    {
        GhostItem.IsVisible = false;
        base.OnLoaded(e);
    }

    private async void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Console.WriteLine("DoDrag start");

        if (sender is not Border border) return;
        Console.WriteLine("DoDrag start is Border");
        if (border.DataContext is not TaskItem taskItem) return;
        Console.WriteLine("DoDrag start is TaskItem");

        var ghostPos = GhostItem.Bounds.Position;
        _ghostPosition = new Point(ghostPos.X + _mouseOffset.X, ghostPos.Y + _mouseOffset.Y);

        var mousePos = e.GetPosition(MainContainer);
        var offsetX = mousePos.X - ghostPos.X;
        var offsetY = mousePos.Y - ghostPos.Y + _mouseOffset.X;
        GhostItem.RenderTransform = new TranslateTransform(offsetX, offsetY);

        if (DataContext is not InsertDragAndDropPageViewModel vm) return;
        vm.StartDrag(taskItem);
        taskItem.Background = "Red";
        _isInDragDrop = true;

        GhostItem.IsVisible = true;

        var dragData = new DataObject();
        dragData.Set(InsertDragAndDropPageViewModel.CustomFormat, taskItem);
        var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
        _isInDragDrop = false;
        Console.WriteLine($"DragAndDrop result: {result}");
        vm.EndDrag();
        GhostItem.IsVisible = false;
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        // Console.WriteLine($"DragOver current time {DateTime.Now}");

        var currentPosition = e.GetPosition(MainContainer);

        var offsetX = currentPosition.X - _ghostPosition.X;
        var offsetY = currentPosition.Y - _ghostPosition.Y;

        GhostItem.RenderTransform = new TranslateTransform(offsetX, offsetY);

        // set drag cursor icon
        e.DragEffects = DragDropEffects.Move;
        if (DataContext is not InsertDragAndDropPageViewModel vm) return;
        var data = e.Data.Get(InsertDragAndDropPageViewModel.CustomFormat);
        if (data is not TaskItem taskItem) return;
        if (!vm.IsDestinationValid(taskItem, (e.Source as Control)?.Name))
        {
            e.DragEffects = DragDropEffects.None;
        }

        // // 获取鼠标相对于 ItemsRepeater 的位置
        // Point mousePosition = e.GetPosition(ToDoItemsRepeater);
        // var isMouseInToDoItemsRepeater = new Rect(ToDoItemsRepeater.Bounds.Size).Contains(mousePosition);
        // if (!isMouseInToDoItemsRepeater) return;

        // // 遍历 ItemsRepeater 中的所有子元素
        // foreach (Control item in myItemsRepeater.GetRealizedElements())
        // {
        //     // 获取当前子元素的边界
        //     Rect itemBounds = item.Bounds;

        //     // 检查鼠标位置是否在子元素的边界内
        //     if (itemBounds.Contains(mousePosition))
        //     {
        //         // 鼠标位于此 DataTemplate 对应的元素上
        //         // 可以在这里添加相应的逻辑，比如改变元素样式
        //         item.Background = Avalonia.Media.Brushes.LightBlue;
        //     }
        //     else
        //     {
        //         // 鼠标不在此元素上，恢复默认样式
        //         item.Background = Avalonia.Media.Brushes.LightGray;
        //     }
        // }
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        var data = e.Data.Get(InsertDragAndDropPageViewModel.CustomFormat);
        Console.WriteLine("Drop");
        if (data is not TaskItem taskItem)
        {
            Console.WriteLine("No task item");
            return;
        }
        if (DataContext is not InsertDragAndDropPageViewModel vm) return;
        vm.Drop(taskItem, (e.Source as Control)?.Name);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        // Get the mouse position relative to MainContainer
        var mousePosition = e.GetPosition(MainContainer);

        // Check if the mouse position is within the bounds of MainContainer
        if (mousePosition.X >= 0 && mousePosition.X <= MainContainer.Bounds.Width &&
            mousePosition.Y >= 0 && mousePosition.Y <= MainContainer.Bounds.Height)
        {
            // Console.WriteLine($"Mouse is inside MainContainer current time {DateTime.Now}");
            // if (_isInDragDrop)
            // {
            //     GhostItem.IsVisible = true;
            // }
        }
        else
        {
            // Console.WriteLine($"Mouse is outside MainContainer current time {DateTime.Now}");
            // if (_isInDragDrop)
            // {
            //     GhostItem.IsVisible = false;
            // }
        }
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (_isInDragDrop)
        {
            GhostItem.IsVisible = true;
        }
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (_isInDragDrop)
        {
            GhostItem.IsVisible = false;
        }
    }

    private void OnPointerEnteredTodoItems(object? sender, PointerEventArgs e)
    {
        Console.WriteLine($"OnPointerEnteredTodoItems");

        if (!_isInDragDrop) return;

        if (DataContext is not InsertDragAndDropPageViewModel vm) return;

        var isInserted = false;
        foreach (Control item in ToDoItemsRepeater.Children)
        {
            Console.WriteLine($"OnPointerEnteredTodoItems item: {item.GetType()}");
            
            if (item.DataContext is TaskItem taskItem)
            {
                Console.WriteLine($"TaskItem found: {taskItem.Title}");

                var mousePosition = e.GetPosition(item);
                if (mousePosition.X >= 0 && mousePosition.X <= ToDoItemsRepeater.Bounds.Width &&
                    mousePosition.Y >= 0 && mousePosition.Y <= ToDoItemsRepeater.Bounds.Height)
                {
                    Console.WriteLine($"Mouse is inside {taskItem.Title}");
                    vm.InsertBeforeItem(taskItem.Title);
                    isInserted = true;
                    //break;
                }
            }            
        }
        if (!isInserted)
        {
            vm.AddItem();
        }
    }

    private void OnPointerExitedTodoItems(object? sender, PointerEventArgs e)
    {
        if (!_isInDragDrop) return;

        if (DataContext is not InsertDragAndDropPageViewModel vm) return;
        vm.RemoveItem();
    }

    private void OnPointerMovedTodoItems(object? sender, PointerEventArgs e)
    {
        if (!_isInDragDrop) return;

        if (DataContext is not InsertDragAndDropPageViewModel vm) return;

        var isInserted = false;
        foreach (Control item in ToDoItemsRepeater.Children)
        {
            Console.WriteLine($"OnPointerEnteredTodoItems item: {item.GetType()}");
            
            if (item.DataContext is TaskItem taskItem)
            {
                //Console.WriteLine($"TaskItem found: {taskItem.Title}");

                var mousePosition = e.GetPosition(item);
                if (mousePosition.X >= 0 && mousePosition.X <= ToDoItemsRepeater.Bounds.Width &&
                    mousePosition.Y >= 0 && mousePosition.Y <= ToDoItemsRepeater.Bounds.Height)
                {
                    Console.WriteLine($"Mouse is inside {taskItem.Title} time {DateTime.Now}");
                    vm.InsertBeforeItem(taskItem.Title);
                    isInserted = true;
                    break;
                }
            }            
        }
        if (!isInserted)
        {
            vm.RemoveItem();
            vm.AddItem();
        }
    }
}
