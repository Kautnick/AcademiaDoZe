using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class AppShell : Shell
{
    public const string LogradouroFormRoute = "logradouro-form";

    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(LogradouroFormRoute, typeof(Views.LogradouroPage));
    }
}
