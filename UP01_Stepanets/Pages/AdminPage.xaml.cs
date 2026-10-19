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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
        }

        private void BtnAppointments_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigationFrame.Navigate(new AdminAppointmentsPage());
            MainWindow.Instance.UpdatePageTitle("Управление записями");
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigationFrame.Navigate(new AdminUsersPage());
            MainWindow.Instance.UpdatePageTitle("Управление пользователями");
        }


        private void BtnDiagram_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigationFrame.Navigate(new DiagramPage());
            MainWindow.Instance.UpdatePageTitle("Диаграмма записей");
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigationFrame.Navigate(new ExportPage());
            MainWindow.Instance.UpdatePageTitle("Экспорт отчетов");
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
    }
}
