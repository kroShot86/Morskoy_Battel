using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private string mode;
        private Random random = new Random();

        public Fight(int[,] player1Field, int[,] player2Field, string mode)
        {
            InitializeComponent();

            this.player1Field = player1Field;
            this.player2Field = player2Field;
            this.mode = mode;

            CreateField(Player1FieldContainer, player1Cells);
            CreateField(Player2FieldContainer, player2Cells);

            UpdateTurnText();
            UpdateFieldInteractivity();
        }

        private async void Cell_Click(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Border cell)) return;

            Point p = (Point)cell.Tag;
            int x = (int)p.X;
            int y = (int)p.Y;

            int[,] enemyField;
            Border[,] enemyCells;

            if (isPlayer1Turn)
            {
                enemyField = player2Field;
                enemyCells = player2Cells;
            }
            else
            {
                if (mode == "PvE") return;
                enemyField = player1Field;
                enemyCells = player1Cells;
            }

            if (!ReferenceEquals(enemyCells[y, x], cell))
                return;

            if (enemyField[y, x] == 2 || enemyField[y, x] == 3)
                return;

            bool hit = ProcessShot(x, y, enemyField, enemyCells);

            if (IsVictory(enemyField))
            {
                string winner;
                if (mode == "PvE")
                {
                    if (isPlayer1Turn) winner = "Вы победили!";
                    else winner = "Бот победил!";
                }
                else
                {
                    if (isPlayer1Turn) winner = "Игрок 1 победил!";
                    else winner = "Игрок 2 победил!";
                }

                MessageBox.Show(winner, "Победа", MessageBoxButton.OK, MessageBoxImage.Information);
                GoToMainMenu();
                return;
            }

            if (!hit)
                isPlayer1Turn = !isPlayer1Turn;

            UpdateTurnText();
            UpdateFieldInteractivity();

            if (mode == "PvE" && !isPlayer1Turn)
            {
                await Task.Delay(700);
                await BotMove();
            }
        }

        private bool ProcessShot(int x, int y, int[,] enemyField, Border[,] enemyCells)
        {
            if (enemyField[y, x] == 2 || enemyField[y, x] == 3)
                return false;

            bool hit = false;

            if (enemyField[y, x] == 1)
            {
                enemyField[y, x] = 3;
                enemyCells[y, x].Background = Brushes.Orange;
                hit = true;

                if (IsShipDestroyed(enemyField, x, y))
                    MarkShipAsDestroyed(enemyField, enemyCells, x, y);
            }
            else
            {
                enemyField[y, x] = 2;
                enemyCells[y, x].Background = Brushes.Gray;
            }

            return hit;
        }

        private async Task BotMove()
        {
            bool hit;

            do
            {
                int x, y;

                do
                {
                    x = random.Next(10);
                    y = random.Next(10);
                }
                while (player1Field[y, x] == 2 || player1Field[y, x] == 3);

                hit = ProcessShot(x, y, player1Field, player1Cells);

                if (IsVictory(player1Field))
                {
                    MessageBox.Show("Бот победил!", "Победа", MessageBoxButton.OK, MessageBoxImage.Information);
                    GoToMainMenu();
                    return;
                }

                if (hit)
                {
                    UpdateTurnText();
                    UpdateFieldInteractivity();
                    await Task.Delay(700);
                }

            } while (hit);

            isPlayer1Turn = true;
            UpdateTurnText();
            UpdateFieldInteractivity();
        }

        private void UpdateFieldInteractivity()
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (mode == "PvE")
                    {
                        player1Cells[y, x].IsEnabled = false;
                        player2Cells[y, x].IsEnabled = isPlayer1Turn;
                    }
                    else
                    {
                        player1Cells[y, x].IsEnabled = !isPlayer1Turn;
                        player2Cells[y, x].IsEnabled = isPlayer1Turn;
                    }
                }
            }
        }

        private void UpdateTurnText()
        {
            if (mode == "PvE")
            {
                if (isPlayer1Turn) CurrentTurnText.Text = "Ваш ход";
                else CurrentTurnText.Text = "Ход бота";
            }
            else
            {
                if (isPlayer1Turn) CurrentTurnText.Text = "Ход игрока 1";
                else CurrentTurnText.Text = "Ход игрока 2";
            }
        }

        private bool IsVictory(int[,] field)
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (field[y, x] == 1)
                        return false;
                }
            }
            return true;
        }

        private bool IsShipDestroyed(int[,] field, int x, int y)
        {
            bool horizontal = false;
            bool vertical = false;

            if (x > 0 && (field[y, x - 1] == 1 || field[y, x - 1] == 3)) horizontal = true;
            if (x < 9 && (field[y, x + 1] == 1 || field[y, x + 1] == 3)) horizontal = true;
            if (y > 0 && (field[y - 1, x] == 1 || field[y - 1, x] == 3)) vertical = true;
            if (y < 9 && (field[y + 1, x] == 1 || field[y + 1, x] == 3)) vertical = true;

            if (horizontal)
            {
                int xx = x;
                while (xx >= 0 && (field[y, xx] == 1 || field[y, xx] == 3))
                {
                    if (field[y, xx] == 1) return false;
                    xx--;
                }

                xx = x + 1;
                while (xx < 10 && (field[y, xx] == 1 || field[y, xx] == 3))
                {
                    if (field[y, xx] == 1) return false;
                    xx++;
                }

                return true;
            }

            if (vertical)
            {
                int yy = y;
                while (yy >= 0 && (field[yy, x] == 1 || field[yy, x] == 3))
                {
                    if (field[yy, x] == 1) return false;
                    yy--;
                }

                yy = y + 1;
                while (yy < 10 && (field[yy, x] == 1 || field[yy, x] == 3))
                {
                    if (field[yy, x] == 1) return false;
                    yy++;
                }

                return true;
            }

            return true;
        }

        private List<Point> GetShipHitCells(int[,] field, int x, int y)
        {
            List<Point> cells = new List<Point>();

            bool horizontal = false;
            bool vertical = false;

            if (x > 0 && (field[y, x - 1] == 1 || field[y, x - 1] == 3)) horizontal = true;
            if (x < 9 && (field[y, x + 1] == 1 || field[y, x + 1] == 3)) horizontal = true;
            if (y > 0 && (field[y - 1, x] == 1 || field[y - 1, x] == 3)) vertical = true;
            if (y < 9 && (field[y + 1, x] == 1 || field[y + 1, x] == 3)) vertical = true;

            if (horizontal)
            {
                int xx = x;
                while (xx >= 0 && (field[y, xx] == 1 || field[y, xx] == 3))
                {
                    if (field[y, xx] == 3) cells.Add(new Point(xx, y));
                    xx--;
                }

                xx = x + 1;
                while (xx < 10 && (field[y, xx] == 1 || field[y, xx] == 3))
                {
                    if (field[y, xx] == 3) cells.Add(new Point(xx, y));
                    xx++;
                }
            }
            else if (vertical)
            {
                int yy = y;
                while (yy >= 0 && (field[yy, x] == 1 || field[yy, x] == 3))
                {
                    if (field[yy, x] == 3) cells.Add(new Point(x, yy));
                    yy--;
                }

                yy = y + 1;
                while (yy < 10 && (field[yy, x] == 1 || field[yy, x] == 3))
                {
                    if (field[yy, x] == 3) cells.Add(new Point(x, yy));
                    yy++;
                }
            }
            else
            {
                cells.Add(new Point(x, y));
            }

            return cells;
        }

        private void MarkShipAsDestroyed(int[,] field, Border[,] cells, int x, int y)
        {
            List<Point> hitCells = GetShipHitCells(field, x, y);

            for (int i = 0; i < hitCells.Count; i++)
            {
                int cx = (int)hitCells[i].X;
                int cy = (int)hitCells[i].Y;

                cells[cy, cx].Background = Brushes.Red;
            }
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
                    Border cell = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(16, 36, 59)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(31, 53, 87)),
                        BorderThickness = new Thickness(1),
                        Tag = new Point(x, y)
                    };

                    cell.MouseLeftButtonDown += Cell_Click;

                    Grid.SetRow(cell, y);
                    Grid.SetColumn(cell, x);

                    cells[y, x] = cell;
                    grid.Children.Add(cell);
                }
            }

            container.Child = grid;
        }

        private void Main_Click(object sender, RoutedEventArgs e)
        {
            GoToMainMenu();
        }

        private void GoToMainMenu()
        {
            MainWindow w = new MainWindow();
            w.Show();
            Close();
        }
    }
}
