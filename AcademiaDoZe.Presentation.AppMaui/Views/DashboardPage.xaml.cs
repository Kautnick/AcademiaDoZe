using Microsoft.Maui.Controls;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class DashboardPage : ContentPage
{
    public string ThemeName { get; private set; } = AppThemeManager.CurrentThemeName;

    public DashboardPage(DashboardListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        ThemeName = AppThemeManager.CurrentThemeName;
    }

    private void OnCycleThemeClicked(object sender, EventArgs e)
    {
        ThemeName = AppThemeManager.CycleTheme();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is DashboardListViewModel viewModel)
            await viewModel.LoadAsync();
    }
}
