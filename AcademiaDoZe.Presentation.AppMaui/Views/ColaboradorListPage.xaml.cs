using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ColaboradorListPage : ContentPage
{
    private readonly ColaboradorListViewModel _viewModel;

    public ColaboradorListPage(ColaboradorListViewModel viewModel)
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
        if (e.CurrentSelection.FirstOrDefault() is ColaboradorDto colaborador)
            await _viewModel.AbrirAsync(colaborador);
        if (sender is CollectionView collectionView)
            collectionView.SelectedItem = null;
    }
}
