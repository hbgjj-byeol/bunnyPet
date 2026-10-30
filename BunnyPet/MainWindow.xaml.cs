using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BunnyPet;
public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(260) };
    private readonly Random _random = new();
    private readonly Dictionary<int, BitmapImage> _frames = new();
    private int _mode, _ticks;
    private double _vx = 1.0, _vy = 0.35;

    public MainWindow()
    {
        InitializeComponent();
        for (int i=1;i<=12;i++)
        {
            var bitmap=new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource=new Uri($"pack://application:,,,/BunnyPet;component/Assets/bunny_{i:00}.png");
            bitmap.CacheOption=BitmapCacheOption.OnLoad;
            bitmap.EndInit(); bitmap.Freeze(); _frames[i]=bitmap;
        }
        Left=Math.Max(0,SystemParameters.WorkArea.Width-Width-60);
        Top=Math.Max(0,SystemParameters.WorkArea.Height-Height-60);
        ShowFrame(1); _timer.Tick+=Tick; _timer.Start();
    }
    private void ShowFrame(int frame)=>BunnyImage.Source=_frames[frame];
    private void Tick(object? sender,EventArgs e)
    {
        _ticks++;
        switch(_mode)
        {
            case 1: ShowFrame((_ticks/3)%2==0?2:3); Left+=_vx; Top+=_vy; KeepOnScreen(); break;
            case 2: ShowFrame(7); break;
            case 3: ShowFrame(8); break;
            case 4: ShowFrame(9); break;
            case 5: ShowFrame(11); break;
            case 6: ShowFrame((_ticks/3)%2==0?5:10); break;
            default:
                ShowFrame(_ticks%20<16?1:4);
                if(_random.Next(100)<2)
                {
                    _mode=_random.Next(1,7);
                    if(_mode==1){_vx=_random.Next(2)==0?-1.25:1.25;_vy=_random.Next(-1,2)*0.35;}
                    _ticks=0;
                }
                break;
        }
        if(_ticks>24&&_mode!=0){_mode=0;_ticks=0;}
    }
    private void KeepOnScreen()
    {
        var a=SystemParameters.WorkArea;
        if(Left<a.Left||Left+Width>a.Right)_vx=-_vx;
        if(Top<a.Top||Top+Height>a.Bottom)_vy=-_vy;
        Left=Math.Clamp(Left,a.Left,a.Right-Width); Top=Math.Clamp(Top,a.Top,a.Bottom-Height);
    }
    private void BunnyImage_MouseLeftButtonDown(object sender,MouseButtonEventArgs e)
    {
        try{DragMove();}catch(InvalidOperationException){}
        e.Handled=true;
    }
    private void BunnyImage_MouseLeftButtonUp(object sender,MouseButtonEventArgs e){}
    private void BunnyImage_MouseMove(object sender,MouseEventArgs e){}
    private void BunnyImage_MouseRightButtonUp(object sender,MouseButtonEventArgs e)
    {
        var menu=new ContextMenu();
        var actions=new MenuItem{Header="행동 바꾸기"};
        AddAction(actions,"걷기",1); AddAction(actions,"잠자기",2);
        AddAction(actions,"세수하기",3); AddAction(actions,"응원하기",4);
        AddAction(actions,"펀치",5); AddAction(actions,"점프",6);
        var exit=new MenuItem{Header="종료"}; exit.Click+=(_,_)=>Close();
        menu.Items.Add(actions); menu.Items.Add(new Separator()); menu.Items.Add(exit);
        menu.IsOpen=true; e.Handled=true;
    }
    private void AddAction(ItemsControl menu,string label,int mode)
    {
        var item=new MenuItem{Header=label}; item.Click+=(_,_)=>{_mode=mode;_ticks=0;}; menu.Items.Add(item);
    }
}
