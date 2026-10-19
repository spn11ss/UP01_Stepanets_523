using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;
        private string captchaCode = "";

        public AuthPage()
        {
            InitializeComponent();
            GenerateCaptcha();
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        private void BtnEnter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxLogin.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SpCaptcha.Visibility == Visibility.Visible)
            {
                if (TbCaptchaInput.Text.Trim() != captchaCode)
                {
                    MessageBox.Show("Неверно введена капча!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    GenerateCaptcha();
                    TbCaptchaInput.Clear();
                    return;
                }
            }

            string hashedPassword = GetHash(PasswordBox.Password);

            var user = Core.GetContext().users
                .FirstOrDefault(u => u.login == TextBoxLogin.Text.Trim() && u.passwordHash == hashedPassword);

            if (user == null)
            {
                failedAttempts++;
                MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                PasswordBox.Clear();

                if (failedAttempts >= 3 && SpCaptcha.Visibility == Visibility.Hidden)
                {
                    SpCaptcha.Visibility = Visibility.Visible;
                    GenerateCaptcha();
                }
                return;
            }

            if (user.roleId == 2)
            {
                MessageBox.Show("Мастера не используют данное приложение для входа. Для управления записями обратитесь к администратору.",
                                "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Clear();
                failedAttempts++;
                return;
            }
            failedAttempts = 0;
            SpCaptcha.Visibility = Visibility.Hidden;
            TbCaptchaInput.Clear();

            Core.CurrentUser = user;

            if (user.roleId == 3)
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

        private void BtnReg_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigationFrame.Navigate(new RegPage());
            MainWindow.Instance.UpdatePageTitle("Регистрация");
        }

        private void BtnChangePass_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigationFrame.Navigate(new ChangePasswordPage());
            MainWindow.Instance.UpdatePageTitle("Смена пароля");
        }

        private void BtnSubmitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (TbCaptchaInput.Text.Trim() == captchaCode)
            {
                MessageBox.Show("Капча введена успешно, можете продолжить авторизацию", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                SpCaptcha.Visibility = Visibility.Hidden;
                TbCaptchaInput.Clear();
                failedAttempts = 0;
            }
            else
            {
                MessageBox.Show("Неверно введена капча!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                GenerateCaptcha();
                TbCaptchaInput.Clear();
            }
        }

        private void GenerateCaptcha()
        {
            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz123456789!@#$%&?";
            Random rnd = new Random();
            captchaCode = new string(Enumerable.Repeat(chars, 6).Select(s => s[rnd.Next(s.Length)]).ToArray());
            TbCaptchaText.Text = captchaCode;
        }
    }
}
