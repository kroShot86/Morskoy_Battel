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
        private string difficulty;
        private Random random = new Random();

        public Fight(int[,] player1Field, int[,] player2Field, string mode, string difficulty)
        {
            InitializeComponent();

            this.player1Field = player1Field;
            this.player2Field = player2Field;
            this.mode = mode;
            this.difficulty = difficulty;

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

            if (!ReferenceEquals(enemyCells[y, x], cell)) return;
            if (enemyField[y, x] == 2 || enemyField[y, x] == 3) return;

            bool hit = ProcessShot(x, y, enemyField, enemyCells);

            if (IsVictory(enemyField))
            {
                string winner;
                if (mode == "PvE") winner = isPlayer1Turn ? "Вы победили!" : "Бот победил!";
                else
                {
                    winner = isPlayer1Turn ? "Игрок 1 победил!" : "Игрок 2 победил!";
                    string opponent = isPlayer1Turn ? "Игрок 1" : "Игрок 2";
                    bool playerWon = isPlayer1Turn;
                    int opponentRating = 1200; // или из профиля
                    int ratingChange = playerWon ? 15 : -10;

                    StatsManager.Instance.AddGameResult(
                        mode: "PvP_afk",
                        opponentName: opponent,
                        opponentRating: opponentRating,
                        isWin: playerWon,
                        ratingChange: ratingChange
                    );
                }
                MessageBox.Show(winner, "Победа", MessageBoxButton.OK, MessageBoxImage.Information);
                GoToMainMenu();
                return;
            }

            if (!hit) isPlayer1Turn = !isPlayer1Turn;

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
            if (enemyField[y, x] == 2 || enemyField[y, x] == 3) return false;

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
            List<Point> targetQueue = new List<Point>();
            List<Point> hitCells = new List<Point>();
            bool hit;

            do
            {
                int x, y;

                if (difficulty == "Лёгкий" || targetQueue.Count == 0)
                {
                    do
                    {
                        x = random.Next(10);
                        y = random.Next(10);
                    } while (player1Field[y, x] == 2 || player1Field[y, x] == 3);
                }
                else
                {
                    x = (int)targetQueue[0].X;
                    y = (int)targetQueue[0].Y;
                    targetQueue.RemoveAt(0);
                }

                hit = ProcessShot(x, y, player1Field, player1Cells);

                if (IsVictory(player1Field))
                {
                    MessageBox.Show("Бот победил!", "Победа", MessageBoxButton.OK, MessageBoxImage.Information);
                    GoToMainMenu();
                    return;
                }

                if (hit && difficulty == "Сложный")
                {
                    hitCells.Add(new Point(x, y));
                    targetQueue.Clear();

                    if (hitCells.Count == 1)
                    {
                        targetQueue.AddRange(GetAdjacentUnhitCells(x, y));
                    }
                    else
                    {
                        bool horizontal = hitCells[0].Y == hitCells[1].Y;
                        bool vertical = hitCells[0].X == hitCells[1].X;

                        if (horizontal)
                        {
                            hitCells.Sort((a, b) => a.X.CompareTo(b.X));
                            Point left = new Point((int)hitCells[0].X - 1, (int)hitCells[0].Y);
                            Point right = new Point((int)hitCells[hitCells.Count - 1].X + 1, (int)hitCells[0].Y);
                            if (IsValidCell(left)) targetQueue.Add(left);
                            if (IsValidCell(right)) targetQueue.Add(right);
                        }
                        else if (vertical)
                        {
                            hitCells.Sort((a, b) => a.Y.CompareTo(b.Y));
                            Point top = new Point((int)hitCells[0].X, (int)hitCells[0].Y - 1);
                            Point bottom = new Point((int)hitCells[0].X, (int)hitCells[hitCells.Count - 1].Y + 1);
                            if (IsValidCell(top)) targetQueue.Add(top);
                            if (IsValidCell(bottom)) targetQueue.Add(bottom);
                        }
                        else
                        {
                            Point[] neighbors = GetAdjacentUnhitCells(x, y).ToArray();
                            foreach (var n in neighbors)
                                if (IsValidCell(n)) targetQueue.Add(n);
                        }
                    }
                }

                if (hit && difficulty == "Сложный") await Task.Delay(500);

                if (hit && IsShipDestroyed(player1Field, x, y))
                {
                    targetQueue.Clear();
                    hitCells.Clear();
                }

            } while (hit || (difficulty == "Сложный" && targetQueue.Count > 0));

            isPlayer1Turn = true;
            UpdateTurnText();
            UpdateFieldInteractivity();
        }

        private bool IsValidCell(Point p)
        {
            int x = (int)p.X;
            int y = (int)p.Y;
            return x >= 0 && x < 10 && y >= 0 && y < 10 && (player1Field[y, x] == 0 || player1Field[y, x] == 1);
        }

        private List<Point> GetAdjacentUnhitCells(int x, int y)
        {
            List<Point> neighbors = new List<Point>();
            if (x > 0 && (player1Field[y, x - 1] == 0 || player1Field[y, x - 1] == 1)) neighbors.Add(new Point(x - 1, y));
            if (x < 9 && (player1Field[y, x + 1] == 0 || player1Field[y, x + 1] == 1)) neighbors.Add(new Point(x + 1, y));
            if (y > 0 && (player1Field[y - 1, x] == 0 || player1Field[y - 1, x] == 1)) neighbors.Add(new Point(x, y - 1));
            if (y < 9 && (player1Field[y + 1, x] == 0 || player1Field[y + 1, x] == 1)) neighbors.Add(new Point(x, y + 1));
            return neighbors;
        }

        private void UpdateFieldInteractivity()
        {
            for (int y = 0; y < 10; y++)
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

        private void UpdateTurnText()
        {
            if (mode == "PvE") CurrentTurnText.Text = isPlayer1Turn ? "Ваш ход" : "Ход бота";
            else CurrentTurnText.Text = isPlayer1Turn ? "Ход игрока 1" : "Ход игрока 2";
        }

        private bool IsVictory(int[,] field)
        {
            for (int y = 0; y < 10; y++)
                for (int x = 0; x < 10; x++)
                    if (field[y, x] == 1) return false;
            return true;
        }

        private bool IsShipDestroyed(int[,] field, int x, int y)
        {
            bool horizontal = (x > 0 && (field[y, x - 1] == 1 || field[y, x - 1] == 3)) || (x < 9 && (field[y, x + 1] == 1 || field[y, x + 1] == 3));
            bool vertical = (y > 0 && (field[y - 1, x] == 1 || field[y - 1, x] == 3)) || (y < 9 && (field[y + 1, x] == 1 || field[y + 1, x] == 3));

            if (horizontal)
            {
                int xx = x;
                while (xx >= 0 && (field[y, xx] == 1 || field[y, xx] == 3)) { if (field[y, xx] == 1) return false; xx--; }
                xx = x + 1;
                while (xx < 10 && (field[y, xx] == 1 || field[y, xx] == 3)) { if (field[y, xx] == 1) return false; xx++; }
                return true;
            }

            if (vertical)
            {
                int yy = y;
                while (yy >= 0 && (field[yy, x] == 1 || field[yy, x] == 3)) { if (field[yy, x] == 1) return false; yy--; }
                yy = y + 1;
                while (yy < 10 && (field[yy, x] == 1 || field[yy, x] == 3)) { if (field[yy, x] == 1) return false; yy++; }
                return true;
            }

            return true;
        }

        private List<Point> GetShipHitCells(int[,] field, int x, int y)
        {
            List<Point> cells = new List<Point>();
            bool horizontal = (x > 0 && (field[y, x - 1] == 1 || field[y, x - 1] == 3)) || (x < 9 && (field[y, x + 1] == 1 || field[y, x + 1] == 3));
            bool vertical = (y > 0 && (field[y - 1, x] == 1 || field[y - 1, x] == 3)) || (y < 9 && (field[y + 1, x] == 1 || field[y + 1, x] == 3));

            if (horizontal)
            {
                int xx = x;
                while (xx >= 0 && (field[y, xx] == 1 || field[y, xx] == 3)) { if (field[y, xx] == 3) cells.Add(new Point(xx, y)); xx--; }
                xx = x + 1;
                while (xx < 10 && (field[y, xx] == 1 || field[y, xx] == 3)) { if (field[y, xx] == 3) cells.Add(new Point(xx, y)); xx++; }
            }
            else if (vertical)
            {
                int yy = y;
                while (yy >= 0 && (field[yy, x] == 1 || field[yy, x] == 3)) { if (field[yy, x] == 3) cells.Add(new Point(x, yy)); yy--; }
                yy = y + 1;
                while (yy < 10 && (field[yy, x] == 1 || field[yy, x] == 3)) { if (field[yy, x] == 3) cells.Add(new Point(x, yy)); yy++; }
            }
            else cells.Add(new Point(x, y));

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
            Grid grid = new Grid { Width = 400, Height = 400 };

            for (int i = 0; i < 10; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int y = 0; y < 10; y++)
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

            container.Child = grid;
        }

        private void Main_Click(object sender, RoutedEventArgs e) => GoToMainMenu();

        private void GoToMainMenu()
        {
            MainWindow w = new MainWindow();
            w.Show();
            Close();
        }
    }
}