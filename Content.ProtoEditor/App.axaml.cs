using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Content.IntegrationTests;
using Content.ProtoEditor.Services;
using Content.ProtoEditor.ViewModels;
using Content.ProtoEditor.ViewModels.Properties;
using Content.ProtoEditor.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Content.ProtoEditor;

public sealed class App : Application
{
    public App()
    {
        PoolManager.Startup();

        // Set up Dependency Injection for our application
        var collection = new ServiceCollection();
        collection.AddSingleton<DependencyProvider>();
        collection.AddSingleton<BackgroundWorkerProvider>();
        collection.AddSingleton<IPrototypeProvider, PrototypeProvider>();
        collection.AddSingleton<PropertyViewModelFactory>();

        collection.AddSingleton<MainWindowViewModel>();
        collection.AddSingleton<PrototypeListViewModel>();
        collection.AddSingleton<PrototypeComponentViewModel>();

        Services = collection.BuildServiceProvider();
    }

    public ServiceProvider Services { get; }

    ~App()
    {
        PoolManager.Shutdown();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        GenerateTemplates();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>(),
            };
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

    private void GenerateTemplates()
    {
        var assembly = typeof(App).Assembly;

        var viewModels = assembly.GetTypes()
            .Where(t => typeof(PropertyViewModel).IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var vmType in viewModels)
        {
            var viewTypeName = vmType.FullName!.Replace("ViewModels", "Views").Replace("ViewModel", "View");
            var viewType = assembly.GetType(viewTypeName);

            if (viewType is null)
                continue; // Skip if no matching view

            var template = new FuncDataTemplate(vmType, (_, _) => (Control)Activator.CreateInstance(viewType)!);

            DataTemplates.Add(template);
        }
    }
}
