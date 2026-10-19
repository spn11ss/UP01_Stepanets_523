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
    /// Логика взаимодействия для EditAppointmentPage.xaml
    /// </summary>
    public partial class EditAppointmentPage : Page
    {
        private appointment _currentAppointment;

        public EditAppointmentPage(appointment selectedAppointment)
        {
            InitializeComponent();
            _currentAppointment = selectedAppointment;

            LoadComboBoxes();
            FillData();
        }
        private void LoadComboBoxes()
        {
            CbMaster.ItemsSource = Core.GetContext().users.Where(u => u.roleId == 2).ToList();
            CbService.ItemsSource = Core.GetContext().service.ToList();
        }

        private void FillData()
        {
            TbClientName.Text = _currentAppointment.users?.FIO ?? "Неизвестно";
            CbMaster.SelectedValue = _currentAppointment.masterId;

            if (_currentAppointment.appointmentDate != null)
            {
                DpDate.SelectedDate = _currentAppointment.appointmentDate.Date;
                TbTime.Text = _currentAppointment.appointmentDate.ToString("HH:mm");
            }

            foreach (ComboBoxItem item in CbStatus.Items)
            {
                if (item.Content.ToString() == _currentAppointment.status)
                {
                    CbStatus.SelectedItem = item;
                    break;
                }
            }
        }
        private void CbMaster_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbMaster.SelectedValue == null) return;

            int masterId = (int)CbMaster.SelectedValue;
            var masterServiceIds = Core.GetContext().master_service
                .Where(ms => ms.masterId == masterId)
                .Select(ms => ms.serviceId)
                .ToList();

            CbService.ItemsSource = Core.GetContext().service
                .Where(s => masterServiceIds.Contains(s.ID))
                .ToList();

            if (_currentAppointment.serviceId != 0)
            {
                CbService.SelectedValue = _currentAppointment.serviceId;
            }
        }

        private void TbTime_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != ':')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CbMaster.SelectedValue == null || CbService.SelectedValue == null ||
                DpDate.SelectedDate == null || string.IsNullOrWhiteSpace(TbTime.Text) || CbStatus.SelectedItem == null)
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
            string selectedStatus = (CbStatus.SelectedItem as ComboBoxItem).Content.ToString();

            bool isPastDate = selectedDateTime < DateTime.Now;
            bool isFinalStatus = selectedStatus == "Выполнена" || selectedStatus == "Отменена";
            bool isOriginalPastDate = _currentAppointment.appointmentDate < DateTime.Now;

            if (isPastDate && !isFinalStatus && !isOriginalPastDate)
            {
                MessageBox.Show("Нельзя перенести запись на время в прошлом! \n(Допускается только изменение статуса на 'Выполнена' или 'Отменена' для прошедших записей)",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                _currentAppointment.masterId = (int)CbMaster.SelectedValue;
                _currentAppointment.serviceId = (int)CbService.SelectedValue;
                _currentAppointment.status = selectedStatus;
                _currentAppointment.appointmentDate = selectedDateTime;

                Core.GetContext().SaveChanges();

                MessageBox.Show("Запись успешно обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
