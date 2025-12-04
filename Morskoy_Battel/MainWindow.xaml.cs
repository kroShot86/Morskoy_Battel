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

        string regim;
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
            Rasstonovka win = new Rasstonovka();
            win.Show();
            this.Close();
        }

        private void PvP_afk_Click(object sender, RoutedEventArgs e)
        {
            regim = "PvP_afk";
            OpenRasstanovka(sender, e);
        }

        private void PvE_Click(object sender, RoutedEventArgs e)
        {
            regim = "PvE";
            OpenRasstanovka(sender, e);
        }

        private void EvE_Click(object sender, RoutedEventArgs e)
        {
            regim = "EvE";
            OpenRasstanovka(sender, e);
        }

        private void PvP_on_Click(object sender, RoutedEventArgs e)
        {
            regim = "PvP_on";
            OpenRasstanovka(sender, e);
        }
    }
}
