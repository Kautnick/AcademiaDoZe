using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class AppShell : Shell
{
    public const string LogradouroFormRoute = "logradouro-form";
    public const string AlunoFormRoute = "aluno-form";
    public const string ColaboradorFormRoute = "colaborador-form";

    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(LogradouroFormRoute, typeof(Views.LogradouroPage));
        Routing.RegisterRoute(AlunoFormRoute, typeof(Views.AlunoPage));
        Routing.RegisterRoute(ColaboradorFormRoute, typeof(Views.ColaboradorPage));
    }
}
