using Snake.Gameelements;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int _elementSize = 20;
        int _numberOfColumns;
        int _numberOfRows;
        double _gameWidth;
        double _gameHeight;

        DispatcherTimer _gameLoopTimer;
        List<SnakeElement> _snakeelements;
        Appel _appels;
        Random _random;
        Direction _currentDirection;
        SnakeElement _staartbackup;

        SerialPort _serialPort;
        byte[] _data;
        const int START_ADRESS = 0;
        const int NUMBER_OF_DMX_BYTES = 512;
        DispatcherTimer _dispatcherTimer;

        public MainWindow()
        {
            InitializeComponent();

            cbxPortName.Items.Add("None");
            foreach (string s in SerialPort.GetPortNames())
                cbxPortName.Items.Add(s);

            _serialPort = new SerialPort();
            _serialPort.BaudRate = 250000;
            _serialPort.StopBits = StopBits.Two;

            _data = new byte[NUMBER_OF_DMX_BYTES];

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = TimeSpan.FromSeconds(0.1);
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.Start();

        }

        //DMX

        private void SenddmxData (byte[] data, SerialPort serialPort)
        {
            data[0] = 0;

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.BreakState = true;
                Thread.Sleep(10000);
                serialPort.BreakState = false;
                Thread.Sleep(10000);

                serialPort.Write(data, 0, data.Length);
            }
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            SenddmxData(_data, _serialPort);
        }

        private void cbxPortName_SelectionChanged(object sneder, SelectedCellsChangedEventArgs e)
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();

                if (cbxPortName.SelectedItem.ToString() != "None")
                {
                    _serialPort.PortName = cbxPortName.SelectedItem.ToString();
                    _serialPort.Open();
                }
            }
        }

        private void GameOver()
        {
            _data[START_ADRESS + 10] = Convert.ToByte(255);
            _data[START_ADRESS + 22] = Convert.ToByte(255);
            _data[START_ADRESS + 34] = Convert.ToByte(255);
            _data[START_ADRESS + 12] = Convert.ToByte(0);
            _data[START_ADRESS + 24] = Convert.ToByte(0);
            _data[START_ADRESS + 36] = Convert.ToByte(0);
        }
            


        protected override void OnContentRendered(EventArgs e)
        {
            InitializeGame();
            base.OnContentRendered(e);
            GameOver();
        }

        void InitializeGame()
        {
            _random = new Random(DateTime.Now.Millisecond / DateTime.Now.Second);
            InitializeTimer();
            DrawGameWorld();
            InitializeSnake();
            DrawSnake();
            GameOver();
        }

        private void MainGameLoop(object sender, EventArgs e)
        {
            MoveSnake();
            CheckCollision();
            DrawSnake();
            CreateAppel();
            DrawAppel();
            Score();
        }
        private void DrawGameWorld()
        {
            _gameWidth = Snakebord.ActualWidth;
            _gameHeight = Snakebord.ActualHeight;
            _numberOfColumns = (int)_gameWidth / _elementSize;
            _numberOfRows = (int)_gameHeight / _elementSize;

            for (int i = 0; i < _numberOfRows; i++)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.X1 = 0;
                line.Y1 = i * _elementSize;
                line.X2 = _gameWidth;
                line.Y2 = i * _elementSize;
                Snakebord.Children.Add(line);
            }

            for (int i = 0; i < _numberOfColumns; i++)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.X1 = i * _elementSize;
                line.Y1 = 0;
                line.X2 = i * _elementSize;
                line.Y2 = _gameHeight;
                Snakebord.Children.Add(line);
            }
        }

        private void DrawAppel()
        {
            if (_appels == null)
                return;
            if (!Snakebord.Children.Contains(_appels.UIElement))
                Snakebord.Children.Add(_appels.UIElement);

            Canvas.SetLeft(_appels.UIElement, _appels.X);
            Canvas.SetTop(_appels.UIElement, _appels.Y);
        }

        private void CreateAppel()
        {
            if (_appels != null)
                return;
            _appels = new Appel(_elementSize)
            {
                X = _random.Next(0, _numberOfColumns) * _elementSize,
                Y = _random.Next(0, _numberOfRows) * _elementSize
            };

        }

        private void DrawSnake()
        {
            foreach (var snakeElement in _snakeelements)
            {
                if (!Snakebord.Children.Contains(snakeElement.UIElement))
                    Snakebord.Children.Add(snakeElement.UIElement);

                Canvas.SetLeft(snakeElement.UIElement, snakeElement.X);
                Canvas.SetTop(snakeElement.UIElement, snakeElement.Y);
            }
        }
        private void GrowSnake()
        {
            _snakeelements.Add(new SnakeElement(_elementSize) { X = _staartbackup.X, Y = _staartbackup.Y });
        }
        private void Rapperspelen()
        {
            _gameLoopTimer.Interval = _gameLoopTimer.Interval - TimeSpan.FromSeconds(0.01);
        }

        private void InitializeSnake()
        {
            _snakeelements = new List<SnakeElement>();
            _snakeelements.Add(new SnakeElement(_elementSize)
            {
                X = (_numberOfColumns / 2) * _elementSize,
                Y = (_numberOfRows / 2) * _elementSize,
                IsHead = true
            });

            _currentDirection = Direction.Right;
        }

        private void InitializeTimer()
        {
            _gameLoopTimer = new DispatcherTimer();
            _gameLoopTimer.Interval = TimeSpan.FromSeconds(0.5);
            _gameLoopTimer.Tick += MainGameLoop;
            _gameLoopTimer.Start();
        }

        private void CheckCollision()
        {
            CheckCollisionWithWorldBounds();
            CheckCollisionSelf();
            CheckColliosionWorldItems();
        }

        private void CheckCollisionWithWorldBounds()
        {
            SnakeElement snakehead = GetSnakeHead();
            if (snakehead.X > _gameWidth - _elementSize || snakehead.X < 0 || snakehead.Y < 0 || snakehead.Y > _gameHeight - _elementSize)
            {
                MessageBox.Show("Game Over! Je hebt tegen de grenzen gebotst!");
                ResetGame();
                InitializeGame();
            }
        }

        private void CheckCollisionSelf()
        {
            SnakeElement snakehead = GetSnakeHead();

            if (snakehead != null)
            {
                foreach (var SnakeElement in _snakeelements)
                {
                    if (!SnakeElement.IsHead)
                    {
                        if (SnakeElement.X == snakehead.X && SnakeElement.Y == snakehead.Y)
                        {
                            MessageBox.Show("Game Over! Je botste tegen jezelf!");
                            ResetGame();
                            GameOver();
                            InitializeGame();
                            break;
                        }

                    }
                }
            }
        }

        private void CheckColliosionWorldItems()
        {
            if (_appels == null)
                return;
            SnakeElement hoofd = _snakeelements[0];
            if (hoofd.X == _appels.X && hoofd.Y == _appels.Y)
            {
                Snakebord.Children.Remove(_appels.UIElement);
                GrowSnake();
                Rapperspelen();
                _appels = null;
            }
        }

        private SnakeElement GetSnakeHead()
        {
            SnakeElement snakehead = null;
            foreach (var SnakeElement in _snakeelements)
            {
                if (SnakeElement.IsHead)
                {
                    snakehead = SnakeElement;
                    break;
                }
            }
            return snakehead;
        }

        private void MoveSnake()
        {
            SnakeElement hoofd = _snakeelements[0];
            SnakeElement staart = _snakeelements[_snakeelements.Count - 1];
            _staartbackup = new SnakeElement(_elementSize)
            {
                X = staart.X,
                Y = staart.Y
            };

            hoofd.IsHead = false;
            staart.IsHead = true;
            staart.X = hoofd.X;
            staart.Y = hoofd.Y;
            switch (_currentDirection)
            {
                case Direction.Right:
                    staart.X += _elementSize;
                    break;
                case Direction.Left:
                    staart.X -= _elementSize;
                    break;
                case Direction.Up:
                    staart.Y -= _elementSize;
                    break;
                case Direction.Down:
                    staart.Y += _elementSize;
                    break;
                default:
                    break;
            }

            _snakeelements.RemoveAt(_snakeelements.Count - 1);
            _snakeelements.Insert(0, staart);

        }

        private void KeyWasReleased(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.I:
                    if (_currentDirection != Direction.Down)
                        _currentDirection = Direction.Up;
                    _currentDirection = Direction.Up;
                    break;

                case Key.J:
                    _currentDirection = Direction.Left;
                    break;

                case Key.K:
                    _currentDirection = Direction.Down;
                    break;

                case Key.L:
                    _currentDirection = Direction.Right;
                    break;

            }
        }

        void ResetGame()
        {
            if (_gameLoopTimer != null)
            {
                _gameLoopTimer.Stop();
                _gameLoopTimer.Tick -= MainGameLoop;
                _gameLoopTimer = null;
            }

            if (Snakebord != null)
            {
                Snakebord.Children.Clear();
            }
            _appels = null;

            if (_snakeelements != null)
            {
                _snakeelements.Clear();
                _snakeelements = null;
            }
            _staartbackup = null;

        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
            MessageBox.Show("Je hebt het spel gestopt!");
            GameOver();
            this.Close();
        }

        private void Herstarten_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
            InitializeGame();
        }

        private void Score()
        {
            score.Content = (_snakeelements.Count) - 1;
        }
    }
}

enum Direction
{
    Right,
    Left, 
    Up, 
    Down
}