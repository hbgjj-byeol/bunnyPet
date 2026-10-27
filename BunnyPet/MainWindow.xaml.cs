using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BunnyPet;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(180) };
    private readonly Random _random = new();
    private readonly Dictionary<int, BitmapImage> _frames = new();
    private Point _dragStart;
    private Point _windowStart;
    private bool _dragging;
    private bool _talkOff = false;
    private int _frame = 1;
    private int _mode = 0; // 0 idle, 1 walk, 2 sleep, 3 wash, 4 cheer, 5 punch, 6 hop
    private int _ticks = 0;
    private double _vx = 1.2;
    private double _vy = 0.5;

    public MainWindow()
    {
        InitializeComponent();
        for (int i = 1; i <= 12; i++)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri($"pack://application:,,,/BunnyPet;component/Assets/bunny_{i:00}.png");
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            _frames[i] = bitmap;
        }
        Left = Math.Max(0, SystemParameters.WorkArea.Width - Width - 80);
        Top = Math.Max(0, SystemParameters.WorkArea.Height - Height - 80);
        ShowFrame(1);
        _timer.Tick += Tick;
        _timer.Start();
    }

    private void ShowFrame(int frame)
    {
        _frame = frame;
        BunnyImage.Source = _frames[frame];
    }

    private void Tick(object? sender, EventArgs e)
    {
        if (_dragging) return;
        _ticks++;
        switch (_mode)
        {
            case 1: // walk: use adjacent walking poses
                ShowFrame((_ticks / 4) % 2 == 0 ? 2 : 3);
                Left += _vx;
                Top += _vy;
                KeepOnScreen();
                break;
            case 2: ShowFrame(7); break; // sleep
            case 3: ShowFrame(8); break; // wash
            case 4: ShowFrame(9); break; // cheer
            case 5: ShowFrame(11); break; // punch
            case 6: ShowFrame((_ticks / 3) % 2 == 0 ? 5 : 10); break; // hop/wiggle
            default:
                ShowFrame(_ticks % 24 < 20 ? 1 : 4);
                if (_random.Next(100) < 2)
                {
                    _mode = _random.Next(0, 7);
                    if (_mode == 0) _mode = 1;
                    if (_mode == 1)
                    {
                        _vx = _random.Next(0, 2) == 0 ? -1.8 : 1.8;
                        _vy = _random.Next(-1, 2) * 0.7;
                    }
                    _ticks = 0;
                }
                break;
        }
        if (_ticks > 30 && _mode != 0)
        {
            _mode = 0;
            _ticks = 0;
        }
    }

    private void KeepOnScreen()
    {
        var area = SystemParameters.WorkArea;
        if (Left < area.Left || Left + Width > area.Right) _vx = -_vx;
        if (Top < area.Top || Top + Height > area.Bottom) _vy = -_vy;
        Left = Math.Clamp(Left, area.Left, area.Right - Width);
        Top = Math.Clamp(Top, area.Top, area.Bottom - Height);
    }

    private void BunnyImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragging = true;
        _dragStart = e.GetPosition(this);
        _windowStart = new Point(Left, Top);
        BunnyImage.CaptureMouse();
        e.Handled = true;
    }

    private void BunnyImage_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_dragging || e.LeftButton != MouseButtonState.Pressed) return;
        var p = e.GetPosition(this);
        Left = _windowStart.X + p.X - _dragStart.X;
        Top = _windowStart.Y + p.Y - _dragStart.Y;
    }

    private void BunnyImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _dragging = false;
        BunnyImage.ReleaseMouseCapture();
    }

    private void BunnyImage_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount >= 2)
        {
            _mode = _random.Next(2, 7);
            _ticks = 0;
        }
    }

    private void BunnyImage_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        var menu = new ContextMenu();
        var talk = new MenuItem { Header = _talkOff ? "대화 켜기" : "대화 끄기" };
        talk.Click += (_, _) => _talkOff = !_talkOff;
        var actions = new MenuItem { Header = "행동 바꾸기" };
        AddAction(actions, "걷기", 1);
        AddAction(actions, "잠자기", 2);
        AddAction(actions, "세수하기", 3);
        AddAction(actions, "응원하기", 4);
        AddAction(actions, "펀치", 5);
        AddAction(actions, "점프/흔들기", 6);
        var exit = new MenuItem { Header = "종료" };
        exit.Click += (_, _) => Close();
        menu.Items.Add(talk);
        menu.Items.Add(actions);
        menu.Items.Add(new Separator());
        menu.Items.Add(exit);
        menu.IsOpen = true;
        e.Handled = true;
    }

    private void AddAction(ItemsControl menu, string label, int mode)
    {
        var item = new MenuItem { Header = label };
        item.Click += (_, _) => { _mode = mode; _ticks = 0; };
        menu.Items.Add(item);
    }
}
