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
using System.Windows.Shapes;

namespace Morskoy_Battel
{
    /// <summary>
    /// Логика взаимодействия для Fight.xaml
    /// </summary>
    public partial class Fight : Window
    {
        public Fight(int[,] player1Field, int[,] player2Field)
        {
            InitializeComponent();
        }

        private void Main_Click(object sender, RoutedEventArgs e)
        {
            MainWindow win = new MainWindow();
            win.Show();
            this.Close();
            return;
        }
    }
}
