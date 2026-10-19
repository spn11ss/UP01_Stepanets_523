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
    /// Логика взаимодействия для UserPage.xaml
    /// </summary>
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
            Core.GetContext().Configuration.LazyLoadingEnabled = true;

            TxtUserInfo.Text = $"Добро пожаловать, {Core.CurrentUser.FIO}!";

            LoadFilters();
            UpdateData();
        }

        private void LoadFilters()
        {
            var categories = Core.GetContext().service_category.ToList();
            categories.Insert(0, new service_category { ID = -1, name = "Все" });
            CbCategory.ItemsSource = categories;
            CbCategory.DisplayMemberPath = "name";
            CbCategory.SelectedValuePath = "ID";
            CbCategory.SelectedIndex = 0;

            var masters = Core.GetContext().users.Where(u => u.roleId == 2).ToList();
            masters.Insert(0, new users { ID = -1, FIO = "Все" });
            CbMaster.ItemsSource = masters;
            CbMaster.DisplayMemberPath = "FIO";
            CbMaster.SelectedValuePath = "ID";
            CbMaster.SelectedIndex = 0;
        }

        private void UpdateData()
        {
            if (!IsInitialized) return;

            int userId = Core.CurrentUser.ID;

            List<appointment> allAppointments = Core.GetContext().appointment
                .Where(a => a.clientId == userId)
                .ToList();

            IEnumerable<appointment> filtered = allAppointments;

            string searchText = TbSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(a =>
                    a.service != null &&
                    a.service.name != null &&
                    a.service.name.ToLower().Contains(searchText));
            }

            if (CbCategory.SelectedItem != null)
            {
                int categoryId = (int)CbCategory.SelectedValue;
                if (categoryId != -1)
                {
                    filtered = filtered.Where(a =>
                        a.service != null &&
                        a.service.categoryId == categoryId);
                }
            }

            if (CbMaster.SelectedItem != null)
            {
                int masterId = (int)CbMaster.SelectedValue;
                if (masterId != -1)
                {
                    filtered = filtered.Where(a => a.masterId == masterId);
                }
            }

            int sortIndex = CbSort.SelectedIndex;
            switch (sortIndex)
            {
                case 0: filtered = filtered.OrderByDescending(a => a.appointmentDate); break;
                case 1: filtered = filtered.OrderBy(a => a.appointmentDate); break;
                case 2: filtered = filtered.OrderBy(a => a.service != null ? a.service.price : 0); break;
                case 3: filtered = filtered.OrderByDescending(a => a.service != null ? a.service.price : 0); break;
            }

            DgAppointments.ItemsSource = filtered.ToList();
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateData();
        private void CbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateData();

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            TbSearch.Clear();
            CbCategory.SelectedIndex = 0;
            CbMaster.SelectedIndex = 0;
            CbSort.SelectedIndex = 0;
            UpdateData();
        }

        private void BtnChangePass_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigationFrame.Navigate(new ChangePasswordPage());
            MainWindow.Instance.UpdatePageTitle("Смена пароля");
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из аккаунта?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Core.CurrentUser = null;
                MainWindow.NavigationFrame.Navigate(new AuthPage());
                MainWindow.Instance.UpdatePageTitle("Авторизация");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int appointmentId = (int)btn.Tag;

            var appointment = Core.GetContext().appointment.FirstOrDefault(a => a.ID == appointmentId);

            if (appointment == null)
            {
                MessageBox.Show("Запись не найдена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (appointment.status == "Отменена")
            {
                MessageBox.Show("Эта запись уже отменена!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (appointment.status == "Запрос на отмену")
            {
                MessageBox.Show("Запрос на отмену этой записи уже отправлен администратору и находится на рассмотрении!",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (appointment.status == "Выполнена")
            {
                MessageBox.Show("Невозможно отменить выполненную запись!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                "Вы действительно хотите отправить запрос на отмену записи?\nАдминистратор рассмотрит его и изменит статус записи.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                appointment.status = "Запрос на отмену";
                Core.GetContext().SaveChanges();
                MessageBox.Show("Запрос на отмену успешно отправлен администратору!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateData();
            }
        }
    }
}
