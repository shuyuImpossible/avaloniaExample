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
using System.Threading.Tasks;

namespace avaloniaExample;

public partial class App : Application
{
    public static IServiceProvider ?ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        InitializeServiceProvider();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();


            // Adding custom splashScreen here
            // for fluent splashScreen, refer to https://www.youtube.com/watch?v=-Ii4QmcYQUU
            var splashSceenVm = new CustomSplashScreenViewModel();
            var splashScreen = new CustomSplashScreenView {
                DataContext = splashSceenVm
            };
            desktop.MainWindow = splashScreen;
            splashScreen.Show();

            try {
                splashSceenVm.StartupMessage = "Searching for devices...";
                await Task.Delay(1000, splashSceenVm.CancellationToken);
                splashSceenVm.StartupMessage = "Loading data...";
                await Task.Delay(1000, splashSceenVm.CancellationToken);
                splashSceenVm.StartupMessage = "Configuring device...";
                await Task.Delay(1000, splashSceenVm.CancellationToken);
            } 
            catch (TaskCanceledException) {
                splashScreen.Close();
                return;
            }

            var viewModel = ServiceProvider?.GetService<MainViewModel>();
            if (viewModel != null) 
            {
                desktop.MainWindow = new MainWindow(viewModel);
                desktop.MainWindow.Show();
                splashScreen.Close();
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
        services.AddSingleton<InsertDragAndDropPageViewModel>();

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
        services.AddTransient<InsertDragAndDropPageView>();
        ServiceProvider = services.BuildServiceProvider();
    }
}