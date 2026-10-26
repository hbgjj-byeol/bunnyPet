using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BunnyPet;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(45) };
    private readonly Random _random = new();
    private double _vx = 1.1;
    private double _vy = 0.0;
    private double _homeY;
    private int _ticks;
    private int _actionTicks;
    private PetAction _action = PetAction.Idle;
    private bool _paused;
    private bool _dragging;
    private Point _dragOffset;

    private enum PetAction { Idle, Walk, Hop, Wash, Cheer, Sleep, Punch }

    public MainWindow()
    {
        InitializeComponent();
        _timer.Tick += Timer_Tick;
        ContextMenu = BuildContextMenu();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Left = Math.Max(0, (SystemParameters.PrimaryScreenWidth - Width) / 2);
        Top = Math.Max(0, SystemParameters.PrimaryScreenHeight - Height - 90);
        _homeY = Top;
        _timer.Start();
    }

    private ContextMenu BuildContextMenu()
    {
        var menu = new ContextMenu();
        var pause = new MenuItem { Header = "이동 일시정지" };
        pause.Click += (_, _) => { _paused = !_paused; pause.Header = _paused ? "이동 다시 시작" : "이동 일시정지"; };
        var action = new MenuItem { Header = "다른 행동 하기" };
        action.Click += (_, _) => PickRandomAction();
        var exit = new MenuItem { Header = "토끼 종료" };
        exit.Click += (_, _) => Close();
        menu.Items.Add(pause);
        menu.Items.Add(action);
        menu.Items.Add(new Separator());
        menu.Items.Add(exit);
        return menu;
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount >= 2) { PickRandomAction(); return; }
        _dragging = true;
        _dragOffset = e.GetPosition(this);
        CaptureMouse();
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _dragging = false;
        ReleaseMouseCapture();
    }

    private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        ContextMenu.IsOpen = true;
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragging && e.LeftButton == MouseButtonState.Pressed)
        {
            var point = e.GetPosition(null);
            Left += point.X - _dragOffset.X;
            Top += point.Y - _dragOffset.Y;
            _homeY = Top;
        }
    }

    private void PickRandomAction()
    {
        _action = (PetAction)_random.Next(Enum.GetValues(typeof(PetAction)).Length);
        _actionTicks = 0;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_paused || _dragging) return;
        _ticks++;
        _actionTicks++;
        if (_actionTicks > _random.Next(70, 150)) PickRandomAction();

        switch (_action)
        {
            case PetAction.Walk:
                Left += _vx;
                Top += _vy;
                if (Left < 0 || Left > SystemParameters.PrimaryScreenWidth - Width) _vx *= -1;
                if (Top < 0 || Top > SystemParameters.PrimaryScreenHeight - Height) _vy *= -1;
                if (_random.Next(30) == 0) { _vx = (_random.NextDouble() * 2.8) - 1.4; _vy = (_random.NextDouble() * 1.8) - 0.9; }
                break;
            case PetAction.Hop:
                Top = _homeY - Math.Abs(Math.Sin(_actionTicks / 5.0)) * 18;
                if (_actionTicks > 35) { Top = _homeY; PickRandomAction(); }
                break;
        }

        PetCanvas.RenderTransform = (_action is PetAction.Idle or PetAction.Wash or PetAction.Cheer)
            ? new TranslateTransform(0, Math.Sin(_ticks / 7.0) * 2)
            : Transform.Identity;
    }
}
