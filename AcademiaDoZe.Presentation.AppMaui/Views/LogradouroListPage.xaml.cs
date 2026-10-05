using Microsoft.Maui.Controls;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LogradouroListPage : ContentPage
{
    public LogradouroListPage(LogradouroListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault();
        if (selected == null) return;
        // Navigate to edit page
        await Shell.Current.GoToAsync(nameof(LogradouroPage), true);
        ((CollectionView)sender).SelectedItem = null;
    }
}
