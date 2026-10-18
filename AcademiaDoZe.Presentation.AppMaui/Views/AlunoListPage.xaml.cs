using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AlunoListPage : ContentPage
{
    private readonly AlunoListViewModel _viewModel;

    public AlunoListPage(AlunoListViewModel viewModel)
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

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is AlunoDto aluno)
            await _viewModel.AbrirAsync(aluno);
        if (sender is CollectionView collectionView)
            collectionView.SelectedItem = null;
    }
}
