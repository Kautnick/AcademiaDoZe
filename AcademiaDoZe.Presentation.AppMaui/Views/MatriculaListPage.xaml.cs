using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class MatriculaListPage : ContentPage
{
    private readonly MatriculaListViewModel _viewModel;

    public MatriculaListPage(MatriculaListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is MatriculaListItem item)
            await _viewModel.AbrirAsync(item.MatriculaId);
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is MatriculaListItem item)
            await _viewModel.ExcluirAsync(item);
    }
}
