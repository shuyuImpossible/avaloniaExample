using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using avaloniaExample.ViewModels;
using System.Collections.Generic;
using avaloniaExample.ViewModels.SplitViewPane;
using avaloniaExample.Views;


namespace avaloniaExample;

public class ViewLocator : IDataTemplate
{
    private readonly Dictionary<Type, Func<Control?>> _locator = [];
    public ViewLocator()
    {
        RegisterViewFactory<MainViewModel, MainWindow>();
        RegisterViewFactory<HomePageViewModel, HomePageView>();
        RegisterViewFactory<ButtonPageViewModel, ButtonPageView>();
        RegisterViewFactory<TextPageViewModel, TextPageView>();
        RegisterViewFactory<ValueSelectionPageViewModel, ValueSelectionPageView>();
        RegisterViewFactory<ImagePageViewModel, ImagePageView>();
        RegisterViewFactory<GridPageViewModel, GridPageView>();
        RegisterViewFactory<DragAndDropPageViewModel, DragAndDropPageView>();
        RegisterViewFactory<CustomSplashScreenViewModel, CustomSplashScreenView>();
        RegisterViewFactory<LoginPageViewModel, LoginPageView>();
        RegisterViewFactory<SecretViewModel, SecretView>();
        RegisterViewFactory<ChartsPageViewModel, ChartsPageView>();
        RegisterViewFactory<InsertDragAndDropPageViewModel, InsertDragAndDropPageView>();
    }

    public Control? Build(object? param)
    {
        // if (param is null)
        //     return null;
        // var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        // var type = Type.GetType(name);
        // Console.WriteLine($"viewlocator build: {param.GetType().FullName}, {name}, {type}");

        // if (type != null)
        // {
        //     return (Control)Activator.CreateInstance(type)!;
        // }
        
        // return new TextBlock { Text = "Not Found: " + name };

        if (param is null)
        {
            return new TextBlock { Text = "No VM provided" };
        }

        _locator.TryGetValue(param.GetType(), out var factory);

        return factory?.Invoke() ?? new TextBlock { Text = $"VM Not Registered: {param.GetType()}" };
    }

    public bool Match(object? data)
    {
        Console.WriteLine($"viewlocator: {data?.GetType()}, {data is ViewModelBase}");
        return data is ViewModelBase;
    }

    private void RegisterViewFactory<TViewModel, TView>()
    where TViewModel : class
    where TView : Control
    => _locator.Add(
        typeof(TViewModel),
        Design.IsDesignMode
            ? Activator.CreateInstance<TView>
            : () => (TView)App.ServiceProvider?.GetService(typeof(TView))!);
}
