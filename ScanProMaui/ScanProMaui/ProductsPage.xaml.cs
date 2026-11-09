using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CollectionView;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodixLabels;

public partial class ProductsPage : ContentPage
{
    private ObservableCollection<ProductDto> _allProducts = new();
    private ObservableCollection<ProductDto> _filteredProducts = new();
    private string _searchText = string.Empty;
    private int _itemsPerPage = 10;

    public ProductsPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadProductsAsync();
    }

    private async void LoadProductsAsync()
    {
        // Cargar productos desde la API
        var apiProducts = await ApiProductService.GetProductsAsync();
        _allProducts = new ObservableCollection<ProductDto>(apiProducts ?? new List<ProductDto>());
        _filteredProducts = new ObservableCollection<ProductDto>(_allProducts);
        ProductsGrid.ItemsSource = _filteredProducts;
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = e.NewTextValue ?? string.Empty;
        FilterProducts();
    }

    private void FilterProducts()
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            _filteredProducts = new ObservableCollection<ProductDto>(_allProducts);
        }
        else
        {
            _filteredProducts = new ObservableCollection<ProductDto>(
                _allProducts.Where(p =>
                    (p.Name ?? "").Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    (p.Code ?? "").Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    (p.Description ?? "").Contains(_searchText, StringComparison.OrdinalIgnoreCase)));
        }

        ProductsGrid.ItemsSource = _filteredProducts;
        _itemsPerPage = _filteredProducts.Count();
        ItemsPerPagePicker.SelectedItem = _itemsPerPage;
    }

    private void OnItemsPerPageChanged(object sender, EventChangedEventArgs e)
    {
        if (ItemsPerPagePicker.SelectedItem is int selectedCount)
        {
            _itemsPerPage = selectedCount;
            // Aplicar paginación simple - mostrar los primeros N items
            var paged = _filteredProducts.Take(_itemsPerPage).ToList();
            ProductsGrid.ItemsSource = paged;
        }
    }

    private async void OnNewClicked(object sender, EventArgs e)
    {
        // Navegar a página de crear producto
        await Shell.Current.GoToAsync("//products/new");
    }

    private async void OnDeleteSelected(object sender, EventArgs e)
    {
        var selectedItems = ProductsGrid.SelectedItems.Cast<ProductDto>().ToList();
        if (selectedItems.Any())
        {
            var confirmed = await Application.Current.MainPage.DisplayAlert(
                "Confirmar eliminación",
                $"¿Eliminar {selectedItems.Count} producto(s) seleccionado(s)?",
                "Sí", "No");

            if (confirmed)
            {
                foreach (var item in selectedItems)
                {
                    await ApiProductService.DeleteProductAsync(item.Id);
                }
                LoadProductsAsync();
            }
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Alert", "Seleccione al menos un producto", "OK");
        }
    }
}