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
    /// Логика взаимодействия для AdminAppointmentsPage.xaml
    /// </summary>
    public partial class AdminAppointmentsPage : Page
    {
        public AdminAppointmentsPage()
        {
            InitializeComponent();
            Core.GetContext().Configuration.LazyLoadingEnabled = true;
            LoadFilters();
            UpdateData();
        }

        private void LoadFilters()
        {
            var masters = Core.GetContext().users.Where(u => u.roleId == 2).ToList();
            masters.Insert(0, new users { ID = -1, FIO = "Все" });
            CbMaster.ItemsSource = masters;
            CbMaster.DisplayMemberPath = "FIO";
            CbMaster.SelectedValuePath = "ID";
            CbMaster.SelectedIndex = 0;

            var categories = Core.GetContext().service_category.ToList();
            categories.Insert(0, new service_category { ID = -1, name = "Все" });
            CbCategory.ItemsSource = categories;
            CbCategory.DisplayMemberPath = "name";
            CbCategory.SelectedValuePath = "ID";
            CbCategory.SelectedIndex = 0;

            CbStatus.SelectedIndex = 0;
            CbSort.SelectedIndex = 0;
        }

        private void UpdateData()
        {
            if (!IsInitialized) return;

            List<appointment> allAppointments = Core.GetContext().appointment.ToList();
            IEnumerable<appointment> filtered = allAppointments;

            string searchText = TbSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(a =>
                    (a.users != null && a.users.FIO != null && a.users.FIO.ToLower().Contains(searchText)) ||
                    (a.service != null && a.service.name != null && a.service.name.ToLower().Contains(searchText)));
            }

            if (CbMaster.SelectedItem != null)
            {
                int masterId = (int)CbMaster.SelectedValue;
                if (masterId != -1)
                {
                    filtered = filtered.Where(a => a.masterId == masterId);
                }
            }

            if (CbCategory.SelectedItem != null)
            {
                int categoryId = (int)CbCategory.SelectedValue;
                if (categoryId != -1)
                {
                    filtered = filtered.Where(a =>
                        a.service != null && a.service.categoryId == categoryId);
                }
            }

            string selectedStatus = (CbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (selectedStatus != null && selectedStatus != "Все")
            {
                filtered = filtered.Where(a => a.status == selectedStatus);
            }

            int sortIndex = CbSort.SelectedIndex;
            switch (sortIndex)
            {
                case 0: filtered = filtered.OrderByDescending(a => a.appointmentDate); break;
                case 1: filtered = filtered.OrderBy(a => a.appointmentDate); break;
                case 2: filtered = filtered.OrderBy(a => a.users != null ? a.users.FIO : ""); break;
                case 3: filtered = filtered.OrderBy(a => a.service != null ? a.service.price : 0); break;
            }

            DgAppointments.ItemsSource = filtered.ToList();
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateData();
        private void CbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateData();

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            TbSearch.Clear();
            CbMaster.SelectedIndex = 0;
            CbCategory.SelectedIndex = 0;
            CbStatus.SelectedIndex = 0;
            CbSort.SelectedIndex = 0;
            UpdateData();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigationFrame.Navigate(new AddAppointmentPage());
            MainWindow.Instance.UpdatePageTitle("Добавление записи");
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DgAppointments.SelectedItem is appointment selectedAppointment)
            {
                MainWindow.NavigationFrame.Navigate(new EditAppointmentPage(selectedAppointment));
                MainWindow.Instance.UpdatePageTitle("Редактирование записи");
            }
            else
            {
                MessageBox.Show("Выберите запись для редактирования!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedAppointments = DgAppointments.SelectedItems.Cast<appointment>().ToList();
            if (selectedAppointments.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одну запись для удаления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы действительно хотите удалить {selectedAppointments.Count} записей?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    foreach (var appointment in selectedAppointments)
                    {
                        Core.GetContext().appointment.Remove(appointment);
                    }
                    Core.GetContext().SaveChanges();

                    MessageBox.Show("Записи успешно удалены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
