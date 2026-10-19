using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UP01_Stepanets.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminUsersPage.xaml
    /// </summary>
    public partial class AdminUsersPage : Page
    {
        public AdminUsersPage()
        {
            InitializeComponent();
            Core.GetContext().Configuration.LazyLoadingEnabled = true;
            LoadFilters();
            UpdateData();
        }

        private void LoadFilters()
        {
            var roles = Core.GetContext().role.Where(r => r.ID != 3).ToList();
            roles.Insert(0, new role { ID = -1, name = "Все" });
            CbRole.ItemsSource = roles;
            CbRole.DisplayMemberPath = "name";
            CbRole.SelectedValuePath = "ID";
            CbRole.SelectedIndex = 0;
            CbSort.SelectedIndex = 0;
        }

        private void UpdateData()
        {
            if (!IsInitialized) return;

            List<users> allUsers = Core.GetContext().users.ToList();
            IEnumerable<users> filtered = allUsers;

            string searchText = TbSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(u =>
                    (u.FIO != null && u.FIO.ToLower().Contains(searchText)) ||
                    (u.login != null && u.login.ToLower().Contains(searchText)));
            }

            if (CbRole.SelectedItem != null)
            {
                int roleId = (int)CbRole.SelectedValue;
                if (roleId != -1)
                {
                    filtered = filtered.Where(u => u.roleId == roleId);
                }
            }

            int sortIndex = CbSort.SelectedIndex;
            switch (sortIndex)
            {
                case 0: filtered = filtered.OrderBy(u => u.FIO); break;
                case 1: filtered = filtered.OrderByDescending(u => u.FIO); break;
                case 2: filtered = filtered.OrderByDescending(u => u.createdAt); break;
            }

            DgUsers.ItemsSource = filtered.ToList();
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateData();
        private void CbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateData();

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            TbSearch.Clear();
            CbRole.SelectedIndex = 0;
            CbSort.SelectedIndex = 0;
            UpdateData();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigationFrame.Navigate(new EditUserPage(null));
            MainWindow.Instance.UpdatePageTitle("Добавление пользователя");
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DgUsers.SelectedItem is users selectedUser)
            {
                MainWindow.NavigationFrame.Navigate(new EditUserPage(selectedUser));
                MainWindow.Instance.UpdatePageTitle("Редактирование пользователя");
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedUsers = DgUsers.SelectedItems.Cast<users>().ToList();
            if (selectedUsers.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одного пользователя для удаления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы действительно хотите удалить {selectedUsers.Count} пользователей?\n(Все их записи будут также удалены)",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    foreach (var user in selectedUsers)
                    {
                        var appointments = Core.GetContext().appointment
                            .Where(a => a.clientId == user.ID || a.masterId == user.ID).ToList();
                        Core.GetContext().appointment.RemoveRange(appointments);

                        var masterServices = Core.GetContext().master_service
                            .Where(ms => ms.masterId == user.ID).ToList();
                        Core.GetContext().master_service.RemoveRange(masterServices);

                        Core.GetContext().users.Remove(user);
                    }

                    Core.GetContext().SaveChanges();
                    MessageBox.Show("Пользователи успешно удалены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    UpdateData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigationFrame.Navigate(new AdminPage());
            MainWindow.Instance.UpdatePageTitle("Панель администратора");
        }
    }
}
