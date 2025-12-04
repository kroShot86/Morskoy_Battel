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

namespace Morskoy_Battel
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string mod;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenRasstanovka(object sender, RoutedEventArgs e)
        {
            Rasstonovka win = new Rasstonovka(mod);
            win.Show();
            this.Close();
        }

        private void PvP_afk_Click(object sender, RoutedEventArgs e)
        {
            mod = "PvP_afk";
            OpenRasstanovka(sender, e);
        }

        private void PvE_Click(object sender, RoutedEventArgs e)
        {
            mod = "PvE";
            OpenRasstanovka(sender, e);
        }

        private void PvP_on_Click(object sender, RoutedEventArgs e)
        {
            mod = "PvP_on";
            OpenRasstanovka(sender, e);
        }
    }
}
