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

namespace UP01_Stepanets
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public static Frame NavigationFrame { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            NavigationFrame = MainFrame;
            LoadTheme();
            NavigationFrame.Navigate(new Pages.AuthPage());
        }

        public void UpdatePageTitle(string title)
        {
            TxtPageTitle.Text = title;
        }
        private void LoadTheme()
        {
            try
            {
                string themeName = Properties.Settings.Default.CurrentTheme;
                if (string.IsNullOrEmpty(themeName)) themeName = "Dictionary.xaml";

                var uri = new Uri(themeName, UriKind.Relative);
                ResourceDictionary resourceDict = (ResourceDictionary)Application.LoadComponent(uri);

                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }
            catch
            {
                var uri = new Uri("Dictionary.xaml", UriKind.Relative);
                ResourceDictionary resourceDict = (ResourceDictionary)Application.LoadComponent(uri);
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }
        }
        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            string currentTheme = Properties.Settings.Default.CurrentTheme;
            string newTheme = (currentTheme == "Dictionary.xaml") ? "Theme2.xaml" : "Dictionary.xaml";

            try
            {
                var uri = new Uri(newTheme, UriKind.Relative);
                ResourceDictionary resourceDict = (ResourceDictionary)Application.LoadComponent(uri);

                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);

                Properties.Settings.Default.CurrentTheme = newTheme;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при смене темы: " + ex.Message);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += (o, t) => { DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"); };
            timer.Start();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите закрыть окно?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else { e.Cancel = false; }
        }
    }
}