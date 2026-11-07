using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class MatriculaPage : ContentPage, IQueryAttributable
{
    private readonly MatriculaViewModel _viewModel;

    public MatriculaPage(MatriculaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var id = query.TryGetValue("id", out var rawId) && int.TryParse(rawId?.ToString(), out var parsedId)
            ? parsedId
            : 0;
        _ = _viewModel.InicializarAsync(id);
    }
}
