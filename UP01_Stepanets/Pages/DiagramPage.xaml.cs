using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Forms.DataVisualization.Charting;

namespace UP01_Stepanets.Pages
{
    /// <summary>
    /// Логика взаимодействия для DiagramPage.xaml
    /// </summary>
    public partial class DiagramPage : Page
    {
        public DiagramPage()
        {
            InitializeComponent();
            BuildChart();
        }

        private void BuildChart()
        {

            Chart chart = new Chart();
            chart.Dock = System.Windows.Forms.DockStyle.Fill;

            ChartArea chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = "Мастера";
            chartArea.AxisY.Title = "Количество записей";
            chartArea.AxisX.Interval = 1;
            chart.ChartAreas.Add(chartArea);

            Series series = new Series("Записи");
            series.ChartType = SeriesChartType.Column;
            series.IsValueShownAsLabel = true;

            chart.Series.Add(series);

            var masterStats = Core.GetContext().appointment
                .Where(a => a.users1 != null && a.users1.roleId == 2)
                .GroupBy(a => a.masterId)
                .Select(g => new
                {
                    MasterId = g.Key,
                    Count = g.Count()
                })
                .ToList();

            foreach (var stat in masterStats)
            {
                var master = Core.GetContext().users.FirstOrDefault(u => u.ID == stat.MasterId);
                string masterName = master?.FIO ?? $"Мастер #{stat.MasterId}";

                string shortName = ShortenFIO(masterName);

                series.Points.AddXY(shortName, stat.Count);
            }
            if (masterStats.Count == 0)
            {
                series.Points.AddXY("Нет данных", 0);
            }

            WfhChart.Child = chart;
        }

        private string ShortenFIO(string fio)
        {
            if (string.IsNullOrWhiteSpace(fio)) return "Неизвестно";

            string[] parts = fio.Trim().Split(' ');
            if (parts.Length == 1) return parts[0];
            if (parts.Length == 2) return $"{parts[0]} {parts[1][0]}.";
            return $"{parts[0]} {parts[1][0]}.{parts[2][0]}.";
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigationFrame.Navigate(new AdminPage());
            MainWindow.Instance.UpdatePageTitle("Панель администратора");
        }
    }
}
