using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Morskoy_Battel
{
    public partial class Fight : Window
    {
        private Border[,] player1Cells = new Border[10, 10];
        private Border[,] player2Cells = new Border[10, 10];

        private int[,] player1Field;
        private int[,] player2Field;

        private bool isPlayer1Turn = true;

        public Fight(int[,] player1Field, int[,] player2Field)
        {
            InitializeComponent();

            this.player1Field = player1Field;
            this.player2Field = player2Field;

            CreateField(Player1FieldContainer, player1Cells);
            CreateField(Player2FieldContainer, player2Cells);

            UpdateFieldInteractivity();
        }

        private void CreateField(Border container, Border[,] cells)
        {
            Grid grid = new Grid();
            grid.Width = 400;
            grid.Height = 400;

            for (int i = 0; i < 10; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Border cell = new Border();
                    cell.Background = new SolidColorBrush(Color.FromRgb(16, 36, 59));
                    cell.BorderBrush = new SolidColorBrush(Color.FromRgb(31, 53, 87));
                    cell.BorderThickness = new Thickness(1);
                    cell.Tag = new Point(x, y);
                    cell.MouseLeftButtonDown += Cell_Click;

                    Grid.SetRow(cell, y);
                    Grid.SetColumn(cell, x);

                    cells[y, x] = cell;
                    grid.Children.Add(cell);
                }
            }

            container.Child = grid;
        }

        private void Cell_Click(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Border cell))
            {
                return;
            }

            Point point = (Point)cell.Tag;
            int x = (int)point.X;
            int y = (int)point.Y;

            int[,] enemyField;
            Border[,] enemyCells;
            string currentPlayerName;
            string enemyPlayerName;

            if (isPlayer1Turn)
            {
                enemyField = player2Field;
                enemyCells = player2Cells;
                currentPlayerName = "Игрок 1";
                enemyPlayerName = "Игрок 2";
            }
            else
            {
                enemyField = player1Field;
                enemyCells = player1Cells;
                currentPlayerName = "Игрок 2";
                enemyPlayerName = "Игрок 1";
            }

            if (enemyField[y, x] == 2 || enemyField[y, x] == 3)
            {
                return;
            }

            bool hit = false;

            if (enemyField[y, x] == 1)
            {
                cell.Background = Brushes.Orange;
                enemyField[y, x] = 3;
                hit = true;

                if (IsShipDestroyed(enemyField, x, y))
                {
                    MarkShipAsDestroyed(enemyField, enemyCells, x, y);
                }
            }
            else
            {
                cell.Background = Brushes.Gray;
                enemyField[y, x] = 2;
            }

            if (IsVictory(enemyField))
            {
                MessageBox.Show(currentPlayerName + " победил!", "Победа", MessageBoxButton.OK, MessageBoxImage.Information);
                GoToMainMenu();
                return;
            }

            if (!hit)
            {
                isPlayer1Turn = !isPlayer1Turn;
            }

            if (isPlayer1Turn)
            {
                CurrentTurnText.Text = "Ход игрока 1";
            }
            else
            {
                CurrentTurnText.Text = "Ход игрока 2";
            }

            UpdateFieldInteractivity();
        }

        private bool IsVictory(int[,] field)
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (field[y, x] == 1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void GoToMainMenu()
        {
            MainWindow win = new MainWindow();
            win.Show();
            this.Close();
        }

        private bool IsShipDestroyed(int[,] field, int x, int y)
        {
            int xx = x - 1;
            while (xx >= 0 && field[y, xx] != 0)
            {
                if (field[y, xx] == 1)
                {
                    return false;
                }
                xx--;
            }

            xx = x + 1;
            while (xx < 10 && field[y, xx] != 0)
            {
                if (field[y, xx] == 1)
                {
                    return false;
                }
                xx++;
            }

            int yy = y - 1;
            while (yy >= 0 && field[yy, x] != 0)
            {
                if (field[yy, x] == 1)
                {
                    return false;
                }
                yy--;
            }

            yy = y + 1;
            while (yy < 10 && field[yy, x] != 0)
            {
                if (field[yy, x] == 1)
                {
                    return false;
                }
                yy++;
            }

            return true;
        }

        private void MarkShipAsDestroyed(int[,] field, Border[,] cells, int x, int y)
        {
            int xx = x;
            while (xx >= 0 && field[y, xx] != 0)
            {
                if (field[y, xx] == 3)
                {
                    cells[y, xx].Background = Brushes.Red;
                }
                xx--;
            }

            xx = x + 1;
            while (xx < 10 && field[y, xx] != 0)
            {
                if (field[y, xx] == 3)
                {
                    cells[y, xx].Background = Brushes.Red;
                }
                xx++;
            }

            int yy = y;
            while (yy >= 0 && field[yy, x] != 0)
            {
                if (field[yy, x] == 3)
                {
                    cells[yy, x].Background = Brushes.Red;
                }
                yy--;
            }

            yy = y + 1;
            while (yy < 10 && field[yy, x] != 0)
            {
                if (field[yy, x] == 3)
                {
                    cells[yy, x].Background = Brushes.Red;
                }
                yy++;
            }
        }

        private void UpdateFieldInteractivity()
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (isPlayer1Turn)
                    {
                        player1Cells[y, x].IsEnabled = false;
                        player2Cells[y, x].IsEnabled = true;
                    }
                    else
                    {
                        player1Cells[y, x].IsEnabled = true;
                        player2Cells[y, x].IsEnabled = false;
                    }
                }
            }
        }

        private void Main_Click(object sender, RoutedEventArgs e)
        {
            GoToMainMenu();
        }
    }
}
