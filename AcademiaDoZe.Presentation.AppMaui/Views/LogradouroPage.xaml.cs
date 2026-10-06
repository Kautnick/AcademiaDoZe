using Microsoft.Maui.Controls;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LogradouroPage : ContentPage, IQueryAttributable
{
    private readonly LogradouroViewModel _viewModel;

    public LogradouroPage(LogradouroViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var rawId) && int.TryParse(rawId?.ToString(), out var id))
            _ = _viewModel.CarregarAsync(id);
    }
}
