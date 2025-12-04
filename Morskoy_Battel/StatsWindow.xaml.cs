using System.Windows;

namespace Morskoy_Battel
{
    public partial class StatsWindow : Window
    {
        public StatsWindow()
        {
            InitializeComponent();
            LoadStats();
        }

        private void LoadStats()
        {
            var records = StatsManager.Instance.GetHumanGameRecords();
            StatsDataGrid.ItemsSource = records;

            if (records.Count == 0)
            {
                MessageBox.Show("Нет записей об играх против человека.", "Статистика пуста", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}