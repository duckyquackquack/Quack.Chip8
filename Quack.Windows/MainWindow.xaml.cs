using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Quack.Chip8.Core.Implementation;

namespace Quack.WPF
{
    public partial class MainWindow : Window
    {
        private readonly CancellationTokenSource _emulatorCancellationTokenSource;
        private readonly WriteableBitmap _canvas;
        private readonly Chip8Interpreter _chip8Interpreter;
        private readonly byte[] _displayBuffer;
        private readonly int _bytesPerPixel;
        private readonly Int32Rect _displayRect;

        private readonly Dictionary<Key, byte> _keyMap;

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            _keyMap = GetKeyMap();

            _emulatorCancellationTokenSource = new CancellationTokenSource();
            const string path = "C:\\Users\\User\\source\\repos\\Quack.Chip8\\Programs\\Pong2";

            // Passes these:
            //const string path = "C:\\Users\\User\\source\\repos\\Quack.Chip8\\Programs\\test_opcode.ch8";
            //const string path = "C:\\Users\\User\\source\\repos\\Quack.Chip8\\Programs\\BC_test.ch8";
            //const string path = "C:\\Users\\User\\source\\repos\\Quack.Chip8\\Programs\\c8_test.c8";

            _chip8Interpreter = new Chip8Interpreter();
            _chip8Interpreter.Load(path);

            _canvas = new WriteableBitmap(_chip8Interpreter.Display.DisplayWidth, _chip8Interpreter.Display.DisplayHeight, 96, 96,
                PixelFormats.Bgr24, null);

            _bytesPerPixel = 3;
            _displayBuffer = new byte[_chip8Interpreter.Display.DisplayWidth * _chip8Interpreter.Display.DisplayHeight * _bytesPerPixel];
            _displayRect = new Int32Rect(0, 0, _chip8Interpreter.Display.DisplayWidth, _chip8Interpreter.Display.DisplayHeight);

            emulatorDisplay.Source = _canvas;

            new TaskFactory().StartNew(Run, _emulatorCancellationTokenSource.Token);
        }

        private void Run()
        {
            const int targetFps = 60;
            const double oneSecondMillis = 1000.0;
            const double oneFrameMillis = oneSecondMillis / targetFps;

            var previousDateTime = DateTime.Now.Subtract(TimeSpan.FromMilliseconds(oneFrameMillis));
            while (!_emulatorCancellationTokenSource.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var elapsedMs = (now - previousDateTime).TotalMilliseconds;

                _chip8Interpreter.Update(elapsedMs);
                if (_chip8Interpreter.Display.RequiresRedraw)
                    Draw();
                previousDateTime = now;

                Thread.Sleep(Math.Max(0, (int)(oneFrameMillis - (DateTime.Now - now).TotalMilliseconds)));
            }
        }

        // TODO, bind to underlying emulator display array
        private void Draw()
        {
            int stride = _chip8Interpreter.Display.DisplayWidth * _bytesPerPixel;

            for (int row = 0; row < _chip8Interpreter.Display.DisplayWidth; row++)
            {
                for (int col = 0; col < _chip8Interpreter.Display.DisplayHeight; col++)
                {
                    var color = Convert.ToByte(_chip8Interpreter.Display[row, col] * 255);

                    // BGR
                    _displayBuffer[(col * stride) + (row * _bytesPerPixel) + 0] = color;
                    _displayBuffer[(col * stride) + (row * _bytesPerPixel) + 1] = color;
                    _displayBuffer[(col * stride) + (row * _bytesPerPixel) + 2] = color;
                    // _displayBuffer[(row * stride) + (col * _bytesPerPixel) + 3] = 255;
                }
            }

            Application.Current.Dispatcher.Invoke(() => _canvas.WritePixels(_displayRect, _displayBuffer, stride, 0));
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _emulatorCancellationTokenSource.Cancel(false);
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_keyMap.ContainsKey(e.Key))
                _chip8Interpreter.KeyState[_keyMap[e.Key]] = true;
        }

        private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (_keyMap.ContainsKey(e.Key))
                _chip8Interpreter.KeyState[_keyMap[e.Key]] = false;
        }

        private Dictionary<Key, byte> GetKeyMap()
        {
            return new Dictionary<Key, byte>
            {
                { Key.D1, 0x1},
                { Key.D2, 0x2},
                { Key.D3, 0x3},
                { Key.D4, 0xC},
                { Key.Q, 0x4},
                { Key.W, 0x5},
                { Key.E, 0x6},
                { Key.R, 0xD},
                { Key.A, 0x7},
                { Key.S, 0x8},
                { Key.D, 0x9},
                { Key.F, 0xE},
                { Key.Z, 0xA},
                { Key.X, 0x0},
                { Key.C, 0xB},
                { Key.V, 0xF}
            };
        }
    }
}
