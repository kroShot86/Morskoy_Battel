using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Morskoy_Battel
{
    public partial class Rasstonovka : Window
    {
        private Border[,] playerCells = new Border[10, 10];
        private ShipControl selectedShip = null;
        private List<ShipControl> placedShips = new List<ShipControl>();
        private Random random = new Random();

        private int[,] player1Field = new int[10, 10];
        private int[,] player2Field = new int[10, 10];
        private bool isFirstPlayer = true;
        private int[,] field = new int[10, 10];

        public Rasstonovka()
        {
            InitializeComponent();
            CreatePlayerField();
            AddShips();
        }

        private void SaveCurrentField(int[,] target)
        {
            for (int y = 0; y < 10; y++)
                for (int x = 0; x < 10; x++)
                    target[y, x] = playerCells[y, x].Child != null ? 1 : 0;
        }

        private void ResetVisual()
        {
            for (int row = 0; row < 10; row++)
                for (int col = 0; col < 10; col++)
                    playerCells[row, col].Child = null;

            placedShips.Clear();
            AddShips();
        }

        private void ClearFieldArray()
        {
            for (int y = 0; y < 10; y++)
                for (int x = 0; x < 10; x++)
                    field[y, x] = 0;
        }

        private void StartSecondPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (Next_player.Content == "В бой!")
            {
                Fight win = new Fight(player1Field, player2Field);
                win.Show();
                this.Close();
                return;
            }

            if (placedShips.Count != 10)
            {
                MessageBox.Show("Расставьте все корабли!");
                return;
            }

            if (isFirstPlayer)
            {
                SaveCurrentField(player1Field);
                MessageBox.Show("Игрок 1 готов. Теперь расставляет Игрок 2!");

                isFirstPlayer = false;

                ClearFieldArray();
                ResetVisual();

                Next_player.Content = "Игрок 2 готов";
            }
            else
            {
                SaveCurrentField(player2Field);
                MessageBox.Show("Оба игрока готовы!");

                Next_player.Content = "В бой!";
                
            }
        }

        private void CreatePlayerField()
        {
            Grid fieldGrid = new Grid { Width = 450, Height = 450, Background = Brushes.Transparent };

            for (int i = 0; i < 10; i++)
            {
                fieldGrid.RowDefinitions.Add(new RowDefinition());
                fieldGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    Border cell = new Border
                    {
                        Width = 45,
                        Height = 45,
                        Background = new SolidColorBrush(Color.FromRgb(16, 36, 59)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(31, 53, 87)),
                        BorderThickness = new Thickness(1),
                        AllowDrop = true,
                        Tag = new Point(col, row)
                    };

                    cell.Drop += Cell_Drop;
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);

                    playerCells[row, col] = cell;
                    fieldGrid.Children.Add(cell);
                }
            }

            FieldContainer.Child = fieldGrid;
        }

        private void AddShips()
        {
            ShipsPanel.Children.Clear();

            // 4
            ShipsPanel.Children.Add(CreateShip(4));
            // 3
            ShipsPanel.Children.Add(CreateShip(3));
            ShipsPanel.Children.Add(CreateShip(3));
            // 2
            ShipsPanel.Children.Add(CreateShip(2));
            ShipsPanel.Children.Add(CreateShip(2));
            ShipsPanel.Children.Add(CreateShip(2));
            // 1
            ShipsPanel.Children.Add(CreateShip(1));
            ShipsPanel.Children.Add(CreateShip(1));
            ShipsPanel.Children.Add(CreateShip(1));
            ShipsPanel.Children.Add(CreateShip(1));
        }

        private ShipControl CreateShip(int size)
        {
            ShipControl ship = new ShipControl(size);
            ship.MouseLeftButtonDown += (s, e) => selectedShip = ship;
            return ship;
        }

        private void AutoPlaceShips_Click(object sender, RoutedEventArgs e)
        {
            ClearFieldArray();
            ResetVisual();

            int[] shipSizes = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };

            placedShips.Clear();

            foreach (int size in shipSizes)
            {
                bool placed = false;

                while (!placed)
                {
                    int startX = random.Next(10);
                    int startY = random.Next(10);
                    bool isHorizontal = random.Next(2) == 0;

                    if (CanPlaceShip(startX, startY, size, isHorizontal))
                    {
                        var ship = new ShipControl(size)
                        {
                            IsHorizontal = isHorizontal
                        };
                        ship.UpdateSize();

                        PlaceShipOnField(ship, startX, startY, addToPlacedShips: true);

                        placed = true;
                    }
                }
            }

            ShipsPanel.Children.Clear();
        }

        private void ClearField_Click(object sender, RoutedEventArgs e)
        {
            ResetVisual();
            ClearFieldArray();
        }

        private void Cell_Drop(object sender, DragEventArgs e)
        {
            if (!GetShipFromData(e.Data, out ShipControl ship)) return;
            if (!(sender is Border cell)) return;

            Point p = (Point)cell.Tag;
            int startX = (int)p.X;
            int startY = (int)p.Y;

            if (placedShips.Contains(ship))
                RemoveShipFromField(ship);

            if (!CanPlaceShip(startX, startY, ship.Size, ship.IsHorizontal))
            {
                ShowPlacementError(ship);
                return;
            }

            PlaceShipOnField(ship, startX, startY);
        }

        private bool CanPlaceShip(int x, int y, int size, bool horizontal)
        {
            if (horizontal)
            {
                if (x + size > 10) return false;
                for (int i = 0; i < size; i++)
                    if (!IsCellAvailableForShip(x + i, y)) return false;
            }
            else
            {
                if (y + size > 10) return false;
                for (int i = 0; i < size; i++)
                    if (!IsCellAvailableForShip(x, y + i)) return false;
            }

            return true;
        }

        private bool IsCellAvailableForShip(int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    int xx = x + dx;
                    int yy = y + dy;

                    if (xx >= 0 && xx < 10 && yy >= 0 && yy < 10)
                        if (playerCells[yy, xx].Child != null)
                            return false;
                }
            return true;
        }

        private void PlaceShipOnField(ShipControl ship, int x, int y, bool addToPlacedShips = false)
        {
            if (ship.Parent == ShipsPanel)
                ShipsPanel.Children.Remove(ship);

            ship.OccupiedCells.Clear();

            for (int i = 0; i < ship.Size; i++)
            {
                int xx = x + (ship.IsHorizontal ? i : 0);
                int yy = y + (ship.IsHorizontal ? 0 : i);

                Border part = CreateShipPart(ship, i);

                playerCells[yy, xx].Child = part;
                ship.OccupiedCells.Add(new Point(xx, yy));
            }

            if (addToPlacedShips && !placedShips.Contains(ship))
                placedShips.Add(ship);
        }

        private Border CreateShipPart(ShipControl ship, int index)
        {
            Border part = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(100, 255, 218)),
                CornerRadius = new CornerRadius(2)
            };

            if (index == 0)
            {
                part.Child = new TextBlock
                {
                    Text = ship.Size.ToString(),
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }

            part.PreviewMouseLeftButtonDown += (s, e) =>
            {
                selectedShip = ship;
                RemoveShipFromField(ship);
                ShipsPanel.Children.Add(ship);

                DataObject d = new DataObject("PlacedShip", ship);
                DragDrop.DoDragDrop(part, d, DragDropEffects.Move);
            };

            return part;
        }

        private void RemoveShipFromField(ShipControl ship)
        {
            foreach (Point p in ship.OccupiedCells)
                playerCells[(int)p.Y, (int)p.X].Child = null;

            ship.OccupiedCells.Clear();
            placedShips.Remove(ship);
        }

        private bool GetShipFromData(IDataObject data, out ShipControl ship)
        {
            ship = null;

            if (data.GetDataPresent("ShipControl"))
                ship = data.GetData("ShipControl") as ShipControl;
            else if (data.GetDataPresent("PlacedShip"))
                ship = data.GetData("PlacedShip") as ShipControl;

            return ship != null;
        }

        private void ShowPlacementError(ShipControl ship)
        {
            MessageBox.Show("Нельзя разместить корабль здесь!");
            if (!ShipsPanel.Children.Contains(ship))
                ShipsPanel.Children.Add(ship);
        }

        private void RotateShip_Click(object sender, RoutedEventArgs e)
        {
            if (selectedShip == null) return;

            selectedShip.Rotate();

            if (placedShips.Contains(selectedShip))
            {
                Point pos = selectedShip.OccupiedCells[0];
                RemoveShipFromField(selectedShip);

                if (CanPlaceShip((int)pos.X, (int)pos.Y, selectedShip.Size, selectedShip.IsHorizontal))
                    PlaceShipOnField(selectedShip, (int)pos.X, (int)pos.Y);
                else
                    ShowPlacementError(selectedShip);
            }
        }
    }

    public class ShipControl : Border
    {
        public int Size { get; private set; }
        public bool IsHorizontal { get; set; } = true;
        public List<Point> OccupiedCells { get; set; } = new List<Point>();

        public ShipControl(int size)
        {
            Size = size;
            Background = new SolidColorBrush(Color.FromRgb(100, 255, 218));
            Cursor = Cursors.Hand;
            CornerRadius = new CornerRadius(4);

            UpdateSize();

            Child = new TextBlock
            {
                Text = Size.ToString(),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            PreviewMouseLeftButtonDown += (s, e) =>
            {
                DataObject data = new DataObject("ShipControl", this);
                DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            };
        }

        public void Rotate()
        {
            IsHorizontal = !IsHorizontal;
            UpdateSize();
        }

        public void UpdateSize()
        {
            if (IsHorizontal)
            {
                Width = Size * 25;
                Height = 25;
            }
            else
            {
                Width = 25;
                Height = Size * 25;
            }
        }
    }
}
