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
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace UP01_Stepanets.Pages
{
    /// <summary>
    /// Логика взаимодействия для ExportPage.xaml
    /// </summary>
    public partial class ExportPage : Page
    {
        private List<appointment> _appointments;

        public ExportPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            Core.GetContext().Configuration.LazyLoadingEnabled = true;
            _appointments = Core.GetContext().appointment.ToList();
            DgAppointments.ItemsSource = _appointments;
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_appointments == null || _appointments.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = $@"{desktopPath}\Отчет_Записи_{DateTime.Now:dd.MM.yyyy}.xlsx";

                Excel.Application excelApp = new Excel.Application { Visible = false };
                Excel.Workbook workbook = excelApp.Workbooks.Add();
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.ActiveSheet;

                worksheet.Cells[1, 1] = "Дата и время";
                worksheet.Cells[1, 2] = "Клиент";
                worksheet.Cells[1, 3] = "Мастер";
                worksheet.Cells[1, 4] = "Услуга";
                worksheet.Cells[1, 5] = "Цена (руб.)";
                worksheet.Cells[1, 6] = "Статус";

                Excel.Range headerRange = worksheet.Range["A1", "F1"];
                headerRange.Font.Bold = true;
                headerRange.Font.Name = "Times New Roman";
                headerRange.Font.Size = 12;
                headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                int row = 2;
                foreach (var app in _appointments)
                {
                    worksheet.Cells[row, 1] = app.appointmentDate.ToString("dd.MM.yyyy HH:mm");
                    worksheet.Cells[row, 2] = app.users?.FIO ?? "Не указан";
                    worksheet.Cells[row, 3] = app.users1?.FIO ?? "Не указан";
                    worksheet.Cells[row, 4] = app.service?.name ?? "Не указана";
                    worksheet.Cells[row, 5] = app.service?.price ?? 0;
                    worksheet.Cells[row, 6] = app.status;

                    Excel.Range dataRange = worksheet.Range[$"A{row}", $"F{row}"];
                    dataRange.Font.Name = "Times New Roman";
                    dataRange.Font.Size = 12;

                    row++;
                }

                worksheet.Columns.AutoFit();

                workbook.SaveAs(filePath);
                workbook.Close();
                excelApp.Quit();

                MessageBox.Show($"Отчет успешно сохранен на Рабочем столе:\n{filePath}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel.\n\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            if (_appointments == null || _appointments.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = $@"{desktopPath}\Отчет_Записи_{DateTime.Now:dd.MM.yyyy}.docx";

                Word.Application wordApp = new Word.Application { Visible = false };
                Word.Document doc = wordApp.Documents.Add();

                Word.Table table = doc.Tables.Add(doc.Content, _appointments.Count + 1, 6);
                table.Borders.Enable = 1;

                table.Cell(1, 1).Range.Text = "Дата";
                table.Cell(1, 2).Range.Text = "Клиент";
                table.Cell(1, 3).Range.Text = "Мастер";
                table.Cell(1, 4).Range.Text = "Услуга";
                table.Cell(1, 5).Range.Text = "Цена";
                table.Cell(1, 6).Range.Text = "Статус";

                table.Rows[1].Range.Font.Name = "Times New Roman";
                table.Rows[1].Range.Font.Size = 12;
                table.Rows[1].Range.Font.Bold = 1;
                table.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                int row = 2;
                foreach (var app in _appointments)
                {
                    table.Cell(row, 1).Range.Text = app.appointmentDate.ToString("dd.MM.yyyy HH:mm");
                    table.Cell(row, 2).Range.Text = app.users?.FIO ?? "Не указан";
                    table.Cell(row, 3).Range.Text = app.users1?.FIO ?? "Не указан";
                    table.Cell(row, 4).Range.Text = app.service?.name ?? "Не указана";
                    table.Cell(row, 5).Range.Text = (app.service?.price ?? 0).ToString();
                    table.Cell(row, 6).Range.Text = app.status;

                    table.Rows[row].Range.Font.Name = "Times New Roman";
                    table.Rows[row].Range.Font.Size = 12;
                    table.Rows[row].Range.Font.Bold = 0;

                    row++;
                }

                table.AutoFitBehavior(Word.WdAutoFitBehavior.wdAutoFitWindow);

                doc.SaveAs2(filePath);
                doc.Close();
                wordApp.Quit();

                MessageBox.Show($"Отчет успешно сохранен на Рабочем столе:\n{filePath}\n\nЗаписей в отчете: {_appointments.Count}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Word.\n\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigationFrame.Navigate(new AdminPage());
            MainWindow.Instance.UpdatePageTitle("Панель администратора");
        }
    }
}
