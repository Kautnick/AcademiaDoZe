using Microsoft.Maui.Controls;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is DashboardListViewModel viewModel)
            await viewModel.LoadAsync();
    }
}
