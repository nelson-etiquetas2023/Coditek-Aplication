using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodixLabels;

public partial class UsersPage : ContentPage
{
    private ObservableCollection<UserDto> _allUsers = new();
    private ObservableCollection<UserDto> _filteredUsers = new();
    private string _searchText = string.Empty;
    private int _itemsPerPage = 10;

    public UsersPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadUsersAsync();
    }

    private async void LoadUsersAsync()
    {
        var apiUsers = await ApiUserService.GetUsersAsync();
        _allUsers = new ObservableCollection<UserDto>(apiUsers ?? new List<UserDto>());
        _filteredUsers = new ObservableCollection<UserDto>(_allUsers);
        UsersGrid.ItemsSource = _filteredUsers;
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = e.NewTextValue ?? string.Empty;
        FilterUsers();
    }

    private void FilterUsers()
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            _filteredUsers = new ObservableCollection<UserDto>(_allUsers);
        }
        else
        {
            _filteredUsers = new ObservableCollection<UserDto>(
                _allUsers.Where(u =>
                    (u.UserName ?? "").Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    (u.Email ?? "").Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    (u.Nickname ?? "").Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    (u.Roles != null && u.Roles.Any(r => r.Contains(_searchText, StringComparison.OrdinalIgnoreCase)))));
        }

        UsersGrid.ItemsSource = _filteredUsers;
        _itemsPerPage = _filteredUsers.Count();
        ItemsPerPagePickerUsers.SelectedItem = _itemsPerPage;
    }

    private void OnItemsPerPageChangedUsers(object sender, EventArgs e)
    {
        if (ItemsPerPagePickerUsers.SelectedItem is int selectedCount)
        {
            _itemsPerPage = selectedCount;
            var paged = _filteredUsers.Take(_itemsPerPage).ToList();
            UsersGrid.ItemsSource = paged;
        }
    }

    private async void OnNewClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//users/new");
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        var selectedUser = UsersGrid.SelectedItem as UserDto;
        if (selectedUser != null)
        {
            // Navegar a página de editar usuario
            await Shell.Current.GoToAsync($"//users/edit/{selectedUser.Id}");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var selectedUsers = UsersGrid.SelectedItems.Cast<UserDto>().ToList();
        if (selectedUsers.Any())
        {
            var confirmed = await Application.Current.MainPage.DisplayAlert(
                "Confirmar eliminación",
                $"¿Eliminar {selectedUsers.Count} usuario(s) seleccionado(s)?",
                "Sí", "No");

            if (confirmed)
            {
                foreach (var user in selectedUsers)
                {
                    await ApiUserService.DeleteUserAsync(user.Id);
                }
                LoadUsersAsync();
            }
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Alert", "Seleccione al menos un usuario", "OK");
        }
    }
}