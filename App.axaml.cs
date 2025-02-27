using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using avaloniaExample.ViewModels;
using avaloniaExample.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using avaloniaExample.ViewModels.SplitViewPane;

namespace avaloniaExample;

public partial class App : Application
{
    public static IServiceProvider ?ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        InitializeServiceProvider();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            var viewModel = ServiceProvider?.GetService<MainViewModel>();
            if (viewModel != null) 
            {
                desktop.MainWindow = new MainWindow(viewModel);
            }
            else
            {
                throw new InvalidOperationException("MainViewModel not found in the service provider.");
            }
        }
        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private static void InitializeServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<HomePageViewModel>();
        services.AddTransient<ButtonPageViewModel>();
        services.AddTransient<TextPageViewModel>();
        services.AddTransient<ValueSelectionPageViewModel>();
        services.AddTransient<ImagePageViewModel>();
        services.AddSingleton<GridPageViewModel>();
        services.AddSingleton<DragAndDropPageViewModel>();
        services.AddSingleton<CustomSplashScreenViewModel>();
        services.AddSingleton<LoginPageViewModel>();
        services.AddSingleton<SecretViewModel>();
        services.AddTransient<ChartsPageViewModel>();

        services.AddSingleton<MainWindow>();
        services.AddTransient<HomePageView>();
        services.AddTransient<ButtonPageView>();
        services.AddTransient<TextPageView>();
        services.AddTransient<ValueSelectionPageView>();
        services.AddTransient<ImagePageView>();
        services.AddTransient<GridPageView>();
        services.AddTransient<DragAndDropPageView>();
        services.AddTransient<CustomSplashScreenView>();
        services.AddTransient<LoginPageView>();
        services.AddTransient<SecretView>();
        services.AddTransient<ChartsPageView>();
        ServiceProvider = services.BuildServiceProvider();
    }
}