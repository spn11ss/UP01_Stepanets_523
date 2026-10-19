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
    /// Логика взаимодействия для EditUserPage.xaml
    /// </summary>
    public partial class EditUserPage : Page
    {
        private readonly users _currentUser;
        private readonly bool _isEditMode;

        public EditUserPage(users userToEdit)
        {
            InitializeComponent();
            _currentUser = userToEdit;
            _isEditMode = (userToEdit != null);

            TxtTitle.Text = _isEditMode ? "Редактирование пользователя" : "Добавление пользователя";
            if (!_isEditMode) TxtPassHint.Visibility = Visibility.Collapsed;

            LoadRoles();
            if (_isEditMode) FillData();
        }

        private void LoadRoles()
        {
            CbRole.ItemsSource = Core.GetContext().role.Where(r => r.ID != 3).ToList();
        }

        private void FillData()
        {
            TbFio.Text = _currentUser.FIO;
            TbPhone.Text = _currentUser.phone;
            TbEmail.Text = _currentUser.email;
            TbLogin.Text = _currentUser.login;
            CbRole.SelectedValue = _currentUser.roleId;
        }


        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbFio.Text) ||
                string.IsNullOrWhiteSpace(TbPhone.Text) ||
                string.IsNullOrWhiteSpace(TbEmail.Text) ||
                string.IsNullOrWhiteSpace(TbLogin.Text) ||
                CbRole.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TbFio.Text.Trim().Length < 5 || !TbFio.Text.Contains(" "))
            {
                MessageBox.Show("ФИО должно содержать имя и фамилию (минимум 5 символов и пробел)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!TbPhone.Text.StartsWith("+7") && !TbPhone.Text.StartsWith("8"))
            {
                MessageBox.Show("Телефон должен начинаться с +7 или 8!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string cleanPhone = TbPhone.Text.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");
            if (!cleanPhone.All(char.IsDigit))
            {
                MessageBox.Show("Номер телефона должен содержать только цифры!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (cleanPhone.Length != 11)
            {
                MessageBox.Show("Номер телефона должен состоять ровно из 11 цифр!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!TbEmail.Text.Contains("@") || !TbEmail.Text.Contains("."))
            {
                MessageBox.Show("Email должен содержать символы '@' и '.'!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (TbEmail.Text.IndexOf('@') > TbEmail.Text.IndexOf('.'))
            {
                MessageBox.Show("Неверный формат Email (точка должна быть после @)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int currentUserId = _currentUser != null ? _currentUser.ID : 0;

            var existingUserByLogin = Core.GetContext().users.FirstOrDefault(u => u.login == TbLogin.Text.Trim() && u.ID != currentUserId);
            if (existingUserByLogin != null)
            {
                MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var existingUserByEmail = Core.GetContext().users.FirstOrDefault(u => u.email == TbEmail.Text.Trim() && u.ID != currentUserId);
            if (existingUserByEmail != null)
            {
                MessageBox.Show("Пользователь с таким email уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var existingUserByPhone = Core.GetContext().users.FirstOrDefault(u => u.phone == TbPhone.Text.Trim() && u.ID != currentUserId);
            if (existingUserByPhone != null)
            {
                MessageBox.Show("Пользователь с таким телефоном уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!_isEditMode && string.IsNullOrWhiteSpace(PbPassword.Password))
            {
                MessageBox.Show("При добавлении пользователя пароль обязателен!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(PbPassword.Password))
            {
                string password = PbPassword.Password;
                if (password.Length < 6)
                {
                    MessageBox.Show("Пароль должен содержать минимум 6 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool hasDigit = false;
                bool hasLetter = false;

                foreach (char c in password)
                {
                    if (char.IsDigit(c)) hasDigit = true;
                    if (char.IsLetter(c)) hasLetter = true;

                    if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')))
                    {
                        MessageBox.Show("Пароль должен содержать только английские буквы и цифры!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (!hasLetter)
                {
                    MessageBox.Show("Пароль должен содержать хотя бы одну букву!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!hasDigit)
                {
                    MessageBox.Show("Пароль должен содержать хотя бы одну цифру!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            try
            {
                if (_isEditMode)
                {
                    _currentUser.FIO = TbFio.Text.Trim();
                    _currentUser.phone = TbPhone.Text.Trim();
                    _currentUser.email = TbEmail.Text.Trim();
                    _currentUser.login = TbLogin.Text.Trim();
                    _currentUser.roleId = (int)CbRole.SelectedValue;

                    if (!string.IsNullOrWhiteSpace(PbPassword.Password))
                    {
                        _currentUser.passwordHash = AuthPage.GetHash(PbPassword.Password);
                    }
                }
                else
                {
                    var newUser = new users
                    {
                        FIO = TbFio.Text.Trim(),
                        phone = TbPhone.Text.Trim(),
                        email = TbEmail.Text.Trim(),
                        login = TbLogin.Text.Trim(),
                        roleId = (int)CbRole.SelectedValue,
                        passwordHash = AuthPage.GetHash(PbPassword.Password),
                        isActive = true,
                        createdAt = DateTime.Now
                    };
                    Core.GetContext().users.Add(newUser);
                }

                Core.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                MainWindow.NavigationFrame.Navigate(new AdminUsersPage());
                MainWindow.Instance.UpdatePageTitle("Управление пользователями");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigationFrame.Navigate(new AdminUsersPage());
            MainWindow.Instance.UpdatePageTitle("Управление пользователями");
        }
    }
}
