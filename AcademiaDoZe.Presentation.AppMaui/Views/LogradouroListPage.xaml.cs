using Microsoft.Maui.Controls;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LogradouroListPage : ContentPage
{
    public LogradouroListPage(LogradouroListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LogradouroListViewModel viewModel)
            await viewModel.LoadAsync();
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not LogradouroDto selected) return;
        try
        {
            await Shell.Current.GoToAsync($"{AppShell.LogradouroFormRoute}?id={selected.Id}", true);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Academia do Zé", $"Não foi possível abrir o logradouro. {ex.Message}", "OK");
        }
        finally
        {
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
