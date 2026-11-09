using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodixLabels;

public partial class ConfigPage : ContentPage
{
    private ObservableCollection<ConfigDto> _allConfigs = new();
    private ObservableCollection<ConfigDto> _filteredConfigs = new();
    private string _searchText = string.Empty;
    private int _itemsPerPage = 10;

    public ConfigPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadConfigsAsync();
    }

    private async void LoadConfigsAsync()
    {
        var apiConfigs = await ApiConfigService.GetConfigsAsync();
        _allConfigs = new ObservableCollection<ConfigDto>(apiConfigs ?? new List<ConfigDto>());
        _filteredConfigs = new ObservableCollection<ConfigDto>(_allConfigs);
        ConfigsGrid.ItemsSource = _filteredConfigs;
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = e.NewTextValue ?? string.Empty;
        FilterConfigs();
    }

    private void FilterConfigs()
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            _filteredConfigs = new ObservableCollection<ConfigDto>(_allConfigs);
        }
        else
        {
            _filteredConfigs = new ObservableCollection<ConfigDto>(
                _allConfigs.Where(c =>
                    (c.Key ?? "").Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.Value ?? "").Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description ?? "").Contains(_searchText, StringComparison.OrdinalIgnoreCase)));
        }

        ConfigsGrid.ItemsSource = _filteredConfigs;
        _itemsPerPage = _filteredConfigs.Count();
        ItemsPerPagePickerConfig.SelectedItem = _itemsPerPage;
    }

    private void OnItemsPerPageChangedConfig(object sender, EventArgs e)
    {
        if (ItemsPerPagePickerConfig.SelectedItem is int selectedCount)
        {
            _itemsPerPage = selectedCount;
            var paged = _filteredConfigs.Take(_itemsPerPage).ToList();
            ConfigsGrid.ItemsSource = paged;
        }
    }

    private async void OnNewClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//config/new");
    }

    private async void OnDeleteSelectedConfig(object sender, EventArgs e)
    {
        var selectedItems = ConfigsGrid.SelectedItems.Cast<ConfigDto>().ToList();
        if (selectedItems.Any())
        {
            var confirmed = await Application.Current.MainPage.DisplayAlert(
                "Confirmar eliminación",
                $"¿Eliminar {selectedItems.Count} configuración(n)(es) seleccionada(s)?",
                "Sí", "No");

            if (confirmed)
            {
                foreach (var item in selectedItems)
                {
                    await ApiConfigService.DeleteConfigAsync(item.Id);
                }
                LoadConfigsAsync();
            }
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Alert", "Seleccione al menos una configuración", "OK");
        }
    }
}