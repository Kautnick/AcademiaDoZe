using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AlunoPage : ContentPage, IQueryAttributable
{
    private readonly AlunoViewModel _viewModel;

    public AlunoPage(AlunoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var id = query.TryGetValue("id", out var rawId) && int.TryParse(rawId?.ToString(), out var parsedId) ? parsedId : 0;
        _ = _viewModel.InicializarAsync(id);
    }
}
