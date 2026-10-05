using Microsoft.Maui.Controls;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LogradouroPage : ContentPage
{
    public LogradouroPage(LogradouroViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
