using Microsoft.Maui.Controls;

namespace CodixLabels;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnProductsClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("//products");
    }

    private void OnConfigClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("//config");
    }

    private void OnUsersClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("//users");
    }

    private void OnStatsClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("//stats");
    }
}