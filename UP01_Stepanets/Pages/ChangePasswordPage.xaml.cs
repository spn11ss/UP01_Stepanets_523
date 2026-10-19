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
    /// Логика взаимодействия для ChangePasswordPage.xaml
    /// </summary>
    public partial class ChangePasswordPage : Page
    {
        public ChangePasswordPage()
        {
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(TbLogin.Text) ||
                string.IsNullOrWhiteSpace(PbOldPassword.Password) ||
                string.IsNullOrWhiteSpace(PbNewPassword.Password) ||
                string.IsNullOrWhiteSpace(PbConfirmPassword.Password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = Core.GetContext().users.FirstOrDefault(u => u.login == TbLogin.Text.Trim());
            if (user == null)
            {
                MessageBox.Show("Пользователь с таким логином не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string oldPasswordHash = AuthPage.GetHash(PbOldPassword.Password);
            if (user.passwordHash != oldPasswordHash)
            {
                MessageBox.Show("Неверный старый пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string newPassword = PbNewPassword.Password;
            if (newPassword.Length < 6)
            {
                MessageBox.Show("Новый пароль должен содержать минимум 6 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool hasDigit = false;
            bool hasLetter = false;

            foreach (char c in newPassword)
            {
                if (char.IsDigit(c)) hasDigit = true;
                if (char.IsLetter(c)) hasLetter = true;
                if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')))
                {
                    MessageBox.Show("Пароль должен содержать только английские буквы и цифры!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (!hasDigit)
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну цифру!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!hasLetter)
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну букву!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (hasLetter && hasDigit)
            {
                if (newPassword != PbConfirmPassword.Password)
                {
                    MessageBox.Show("Новые пароли не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            try
            {
                user.passwordHash = AuthPage.GetHash(newPassword);
                Core.GetContext().SaveChanges();

                MessageBox.Show("Пароль успешно изменен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                if (Core.CurrentUser != null)
                {
                    if (Core.CurrentUser.roleId == 3)
                    {
                        MainWindow.NavigationFrame.Navigate(new AdminPage());
                        MainWindow.Instance.UpdatePageTitle("Панель администратора");
                    }
                    else
                    {
                        MainWindow.NavigationFrame.Navigate(new UserPage());
                        MainWindow.Instance.UpdatePageTitle("Личный кабинет");
                    }
                }
                else
                {
                    MainWindow.NavigationFrame.Navigate(new AuthPage());
                    MainWindow.Instance.UpdatePageTitle("Авторизация");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser != null)
            {
                if (Core.CurrentUser.roleId == 3)
                {
                    MainWindow.NavigationFrame.Navigate(new AdminPage());
                    MainWindow.Instance.UpdatePageTitle("Панель администратора");
                }
                else
                {
                    MainWindow.NavigationFrame.Navigate(new UserPage());
                    MainWindow.Instance.UpdatePageTitle("Личный кабинет");
                }
            }
            else
            {
                MainWindow.NavigationFrame.Navigate(new AuthPage());
                MainWindow.Instance.UpdatePageTitle("Авторизация");
            }
        }
    }
}
