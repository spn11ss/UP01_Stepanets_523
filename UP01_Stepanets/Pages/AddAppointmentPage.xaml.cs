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
    /// Логика взаимодействия для AddAppointmentPage.xaml
    /// </summary>
    public partial class AddAppointmentPage : Page
    {
        public AddAppointmentPage()
        {
            InitializeComponent();
            DpDate.SelectedDate = DateTime.Now.Date;
            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            CbClient.ItemsSource = Core.GetContext().users.Where(u => u.roleId == 1).ToList();
            CbMaster.ItemsSource = Core.GetContext().users.Where(u => u.roleId == 2).ToList();
            CbService.ItemsSource = Core.GetContext().service.ToList();
        }

        private void CbMaster_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbMaster.SelectedValue == null)
            {
                CbService.ItemsSource = Core.GetContext().service.ToList();
                return;
            }

            int masterId = (int)CbMaster.SelectedValue;
            var masterServiceIds = Core.GetContext().master_service
                .Where(ms => ms.masterId == masterId)
                .Select(ms => ms.serviceId)
                .ToList();

            CbService.ItemsSource = Core.GetContext().service
                .Where(s => masterServiceIds.Contains(s.ID))
                .ToList();
            CbService.SelectedIndex = -1;
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CbClient.SelectedValue == null || CbMaster.SelectedValue == null ||
                CbService.SelectedValue == null || DpDate.SelectedDate == null ||
                string.IsNullOrWhiteSpace(TbTime.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DateTime.TryParse(TbTime.Text.Trim(), out DateTime parsedTime))
            {
                MessageBox.Show("Неверный формат времени! Используйте формат ЧЧ:мм", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime selectedDateTime = DpDate.SelectedDate.Value.Date + parsedTime.TimeOfDay;

            if (selectedDateTime < DateTime.Now)
            {
                MessageBox.Show("Нельзя создать запись на дату и время в прошлом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var newAppointment = new appointment
                {
                    clientId = (int)CbClient.SelectedValue,
                    masterId = (int)CbMaster.SelectedValue,
                    serviceId = (int)CbService.SelectedValue,
                    appointmentDate = selectedDateTime,
                    status = "Подтверждена"
                };

                Core.GetContext().appointment.Add(newAppointment);
                Core.GetContext().SaveChanges();

                MessageBox.Show("Запись успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.NavigationFrame.Navigate(new AdminAppointmentsPage());
                MainWindow.Instance.UpdatePageTitle("Управление записями");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigationFrame.Navigate(new AdminAppointmentsPage());
            MainWindow.Instance.UpdatePageTitle("Управление записями");
        }
    }
}
