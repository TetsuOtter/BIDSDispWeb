using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace TR.BIDSDispWeb
{
  public partial class MainPage : Page
  {
    public MainPage()
    {
      InitializeComponent();
      // Enter construction logic here...
    }
    readonly double OrigH = 800;
    readonly double OrigW = 1280;
    bool IsDebug = false;
    private void OnSizeChange(object sender, SizeChangedEventArgs e)
    {
      double NowHeight;
      double NowWidth;
      NowHeight = e.NewSize.Height;
      NowWidth = e.NewSize.Width;
      if (IsDebug || e.NewSize.IsEmpty) return;

      double ScaleX = NowWidth / OrigW;
      double ScaleY = NowHeight / OrigH;
      double scale = Math.Min(ScaleX, ScaleY);
      double MarX = 0;
      double MarY = 0;
      if (ScaleX == scale) MarY = (NowHeight - OrigH * ScaleX) / 2;
      if (ScaleY == scale) MarX = (NowWidth - OrigW * ScaleY) / 2;
      
      FrontFrame.Margin = new Thickness(MarX, MarY, MarX, MarY);
      ScaleTransform scl = new ScaleTransform { ScaleX = scale, ScaleY = scale };
      double Origin = 0;
      if (scale >= 1) Origin = 0.5;
      
      FrontFrame.RenderTransformOrigin = new Point(Origin, Origin);
      FrontFrame.RenderTransform = scl;
    }

    private void OnUnLoad(object sender, RoutedEventArgs e)
    {

    }

    private void OnLoad(object sender, RoutedEventArgs e)
    {
      TopPage.LightChanged += TopPage_LightChanged;
      FrontFrame.Source = new Uri("/", UriKind.Relative);
      OnSizeChange(null, new SizeChangedEventArgs(new Size(Width, Height)));
    }

    private void TopPage_LightChanged(object sender, EventArgs e)
    {
      double opt = 0;
      switch (LightAdjusterRec.Opacity)
      {
        case 0:
          opt = 0.2;
          break;
        case 0.2:
          opt = 0.3;
          break;
        case 0.3:
          opt = 0.4;
          break;
        case 0.4:
          opt = 0.6;
          break;
        case 0.6:
          opt = 0;
          break;
      }
      LightAdjusterRec.Opacity = opt;
    }
  }
}
