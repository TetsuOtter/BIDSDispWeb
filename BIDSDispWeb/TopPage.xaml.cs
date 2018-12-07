using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace TR.BIDSDispWeb
{
  public partial class TopPage : Page
  {
    static public event EventHandler LightChanged;
    public TopPage()
    {
      InitializeComponent();
    }
    Grid showingGrid;
    Grid ShowingGrid
    {
      get
      {
        return showingGrid;
      }
      set
      {
        if (ShowingGrid != null)
        {
          ShowingGrid.Visibility = Visibility.Collapsed;
          value.Visibility = Visibility.Visible;
          showingGrid = value;
        }
        else
        {
          value.Visibility = Visibility.Visible;
          showingGrid = value;
        }
        if (value == TopGrid && dt.IsEnabled) dt.Stop();
      }
    }
    string IPAddr = string.Empty;
    bool _IsSkyWayConnected = false;
    bool IsSkyWayConnected
    {
      get
      {
        return _IsSkyWayConnected;
      }
      set
      {
        _IsSkyWayConnected = value;
        TopGrid_SelectGrid.IsEnabled = value;
        if (value)
        {
          IsSkyWayConnectedBlock.Text = "接続成功";
          IsSkyWayConnectedBlock.Background = new SolidColorBrush(Colors.Black);
          IsSkyWayConnectedBlock.Foreground = new SolidColorBrush(Colors.White);
        }
        else
        {
          IsSkyWayConnectedBlock.Text = "未接続";
          IsSkyWayConnectedBlock.Background = new SolidColorBrush(Colors.Red);
          IsSkyWayConnectedBlock.Foreground = new SolidColorBrush(Colors.Black);
        }
      }
    }

    class HandleClass
    {
      /// <summary>
      /// ブレーキハンドル位置
      /// </summary>
      public int B;
      /// <summary>
      /// ノッチハンドル位置
      /// </summary>
      public int P;
      /// <summary>
      /// レバーサーハンドル位置
      /// </summary>
      public int R;
      /// <summary>
      /// 定速制御状態
      /// </summary>
      public int C;
    }
    class StateClass
    {
      /// <summary>
      /// 列車位置[m]
      /// </summary>
      public double Z;
      /// <summary>
      /// 列車速度[km/h]
      /// </summary>
      public float V;
      /// <summary>
      /// 0時からの経過時間[ms]
      /// </summary>
      public int T;
      /// <summary>
      /// BC圧力[kPa]
      /// </summary>
      public float BC;
      /// <summary>
      /// MR圧力[kPa]
      /// </summary>
      public float MR;
      /// <summary>
      /// ER圧力[kPa]
      /// </summary>
      public float ER;
      /// <summary>
      /// BP圧力[kPa]
      /// </summary>
      public float BP;
      /// <summary>
      /// SAP圧力[kPa]
      /// </summary>
      public float SAP;
      /// <summary>
      /// 電流[A]
      /// </summary>
      public float I;

      public int Hour;
      public int Minute;
      public int Second;
      public int MSecond;
    }
    StateClass STC = new StateClass();
    HandleClass HDC = new HandleClass();
    private void CheckConnect(object sender, RoutedEventArgs e)
    {
      Console.WriteLine(AddrBox.Text);
      IsSkyWayConnected = true;
    }
    enum ObjInfo
    {
      ThatsNULL, ThatsBOOL, ThatsINT, ThatsFLOAT, ThatsDOUBLE, ThatsSTRING, ThatsERROR
    };

    private void SelectPage(object sender, RoutedEventArgs e)
    {
      ShowingGrid = PageSelectGrid;
    }
    DispatcherTimer dt = new DispatcherTimer();
    private void OnLoad(object sender, RoutedEventArgs e)
    {
      ErrorGridShow("abc", "def", 0);
      dt.Interval = new TimeSpan(0, 0, 0, 0, WaitTime);
      OldWaitTime = WaitTime;
      dt.Tick += LoopDoingVOID;
      ShowingGrid = TopGrid;
      IsSkyWayConnected = false;
      dt.Start();
    }

    int OldWaitTime = 0;
    StateClass OSTC = new StateClass();
    HandleClass OHDC = new HandleClass();
    bool IsDoorClosed = false;
    bool oIsDoorClosed = false;

    private void StateSet(double TrainLocation, float Speed, float BC, float MR, float ER, float BP, float SAP, float Current, int PNotch, int BNotch, int Reverser, int Door, int TimeHH, int TimeMM, int TimeSS, int TimeMS)
    {
      STC = new StateClass() { Z = TrainLocation, BC = BC, BP = BP, ER = ER, Hour = TimeHH, I = Current, Minute = TimeMM, MR = MR, MSecond = TimeMS, SAP = SAP, Second = TimeSS, V = Speed };
      HDC = new HandleClass() { B = BNotch, P = PNotch, R = Reverser };
      if (Door == 1) IsDoorClosed = true;
      else IsDoorClosed = false;
    }
    private void ErrorGridShow(string Title,string Content,int ButtonMode)
    {
      if (!string.IsNullOrWhiteSpace(Title) || !string.IsNullOrWhiteSpace(Content))
      {
        ErrorShowGrid_Title.Text = Title;
        ErrorShowGrid_Content.Text = Content;
        ErrorShowGrid.Visibility = Visibility.Visible;
      }
      switch (ButtonMode)
      {
        case 1://「閉じる」ボタン
          ErrorShowGrid_Button.Content = "閉じる";
          ErrorShowGrid_Button.Click += ErrorShowGrid_Close;
          break;
        default:
          ErrorShowGrid_Button.Content = "トップへ";
          ErrorShowGrid_Button.Click += ErrorShowGrid_GoTop;
          break;//「トップへ」ボタン
      }
    }

    private void ErrorShowGrid_Close(object s, object e)
    {
      ErrorShowGrid.Visibility = Visibility.Collapsed;
    }
    private void ErrorShowGrid_GoTop(object s, object e)
    {
      ShowingGrid = TopGrid;
      ErrorShowGrid.Visibility = Visibility.Collapsed;
    }


    private void LoopDoingVOID(object sender, object e)
    {
      if (OldWaitTime != WaitTime)
      {
        OldWaitTime = WaitTime;
        dt.Interval = new TimeSpan(0, 0, 0, 0, WaitTime);
      }

      StateSet(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

      if (!Equals(OSTC, STC))
      {
        SPGrid_TR01_SPSet = STC.V;
        SPGrid_TR01_PressSet(STC.MR, STC.ER, STC.BC, STC.BP);
        SPGrid_TR01_TimeSet((byte)STC.Hour, (byte)STC.Minute, (byte)STC.Second);
        SPGrid_TR01_CurrentSet = STC.I;
        SPGrid_TR01_LocationSet = STC.Z;
        OSTC = STC;
      }
      if (!Equals(OHDC, HDC))
      {
        SPGrid_TR01_ReverserSet = HDC.R;
        SPGrid_TR01_HandleSet(HDC.B, HDC.P);
        OHDC = HDC;
      }
      if (!Equals(oIsDoorClosed, IsDoorClosed))
      {
        SPGrid_TR01_IsDoorClosed = IsDoorClosed;
        oIsDoorClosed = IsDoorClosed;
      }
    }

    private void OnUnLoad(object sender, RoutedEventArgs e)
    {

      dt.Stop();
    }


    //Grobal
    private void LightChagne(object sender, RoutedEventArgs e) => LightChanged?.Invoke(null, null);

    private void GridSelected(object sender, RoutedEventArgs e)
    {
      switch (((Button)sender).Content.ToString())
      {
        case "ＴＲ０１型速度計":
          ShowingGrid = SPGrid_TR01;
          break;
      }
    }

    int WaitTime = 200;
    private void fpsChange(object sender, RoutedEventArgs e)
    {
      WaitTime += 50;
      if (WaitTime > 550) WaitTime = 50;
      switch (WaitTime)
      {
        case 50:
          ((Button)sender).Content = "🕐";
          break;
        case 100:
          ((Button)sender).Content = "🕑";
          break;
        case 150:
          ((Button)sender).Content = "🕒";
          break;
        case 200:
          ((Button)sender).Content = "🕓";
          break;
        case 250:
          ((Button)sender).Content = "🕔";
          break;
        case 300:
          ((Button)sender).Content = "🕕";
          break;
        case 350:
          ((Button)sender).Content = "🕖";
          break;
        case 400:
          ((Button)sender).Content = "🕗";
          break;
        case 450:
          ((Button)sender).Content = "🕘";
          break;
        case 500:
          ((Button)sender).Content = "🕙";
          break;
        case 550:
          ((Button)sender).Content = "🕚";
          break;
      }
    }




    //////////////////////////////////////////////////////////////////////////
    //
    //SPGrid
    //////////////////////////////////////////////////////////////////////////
    ////
    ////TR01
    //////////////////////////////////////////////////////////////////////////
    readonly Thickness[] SPGrid_TR01_SP90Thick =
{
          new Thickness(890,570,0,0),
          new Thickness(850,450,0,0),
          new Thickness(920,360,0,0),
          new Thickness(1040,360,0,0),
          new Thickness(1110,450,0,0),
          new Thickness(1110,520,0,0)
        };
    readonly string[] SPGrid_TR01_SP90MemNumStr =
    {
          "0","20","40","60","80","90"
        };

    readonly Thickness[] SPGrid_TR01_SP120Thick =
    {
          new Thickness(870,560,0,0),
          new Thickness(850,460,0,0),
          new Thickness(890,380,0,0),
          new Thickness(980,350,0,0),
          new Thickness(1070,380,0,0),
          new Thickness(1095,460,0,0),
          new Thickness(1075,560,0,0)
        };
    readonly Thickness[] SPGrid_TR01_SP140Thick =
    {
          new Thickness(875,570,0,0),
          new Thickness(850,480,0,0),
          new Thickness(875,400,0,0),
          new Thickness(940,355,0,0),
          new Thickness(1025,355,0,0),
          new Thickness(1070,400,0,0),
          new Thickness(1095,480,0,0),
          new Thickness(1070,570,0,0)
        };
    readonly Thickness[] SPGrid_TR01_SP160Thick =
    {
          new Thickness(870,560,0,0),
          new Thickness(850,480,0,0),
          new Thickness(870,415,0,0),
          new Thickness(910,370,0,0),
          new Thickness(980,350,0,0),
          new Thickness(1035,370,0,0),
          new Thickness(1075,415,0,0),
          new Thickness(1095,480,0,0),
          new Thickness(1075,560,0,0)
        };
    readonly Thickness[] SPGrid_TR01_SP240Thick =
    {
          new Thickness(870,560,0,0),
          new Thickness(850,460,0,0),
          new Thickness(890,380,0,0),
          new Thickness(975,350,0,0),
          new Thickness(1055,380,0,0),
          new Thickness(1095,460,0,0),
          new Thickness(1075,560,0,0)
        };
    readonly Thickness[] SPGrid_TR01_SP360Thick =
    {
          new Thickness(870,560,0,0),
          new Thickness(850,460,0,0),
          new Thickness(890,380,0,0),
          new Thickness(980,350,0,0),
          new Thickness(1070,380,0,0),
          new Thickness(1095,460,0,0),
          new Thickness(1095,460,0,0),
          new Thickness(1095,460,0,0),
          new Thickness(1075,560,0,0),
          new Thickness(1075,560,0,0)
        };


    private bool SPGrid_TR01_IsTickDefault = true;
    private int SPGrid_TR01_maxSP = 160;
    private Windows.UI.Xaml.Shapes.Path SPGrid_TR01_nowShowingSPMem = null;
    private Windows.UI.Xaml.Shapes.Path SPGrid_TR01_NowShowingSPMem
    {
      get
      {
        if (SPGrid_TR01_nowShowingSPMem == null) return SPGrid_TR01_SP160Memori;
        else return SPGrid_TR01_nowShowingSPMem;
      }
      set
      {
        if (SPGrid_TR01_nowShowingSPMem != null) SPGrid_TR01_nowShowingSPMem.Visibility = Visibility.Collapsed;
        else SPGrid_TR01_SP160Memori.Visibility = Visibility.Collapsed;
        value.Visibility = Visibility.Visible;
        SPGrid_TR01_nowShowingSPMem = value;
        SPGrid_TR01_SPSet = SPGrid_TR01_SPSet;
      }
    }

    private void SPGrid_TR01_MaxSPChangeEv(object sender, RoutedEventArgs e)
    {
      switch (SPGrid_TR01_MaxSP)
      {
        case 90:
          SPGrid_TR01_MaxSP = 120;
          return;
        case 120:
          SPGrid_TR01_MaxSP = 140;
          return;
        case 140:
          SPGrid_TR01_MaxSP = 160;
          return;
        case 160:
          SPGrid_TR01_MaxSP = 240;
          return;
        case 240:
          SPGrid_TR01_MaxSP = 320;
          return;
        case 320:
          //MaxSP = 360;
          SPGrid_TR01_MaxSP = 90;//Max360km/hモードは未実装
          return;
        case 360:
          SPGrid_TR01_MaxSP = 90;
          return;
        default:
          SPGrid_TR01_MaxSP = 160;
          return;
      }
    }

    private int SPGrid_TR01_MaxSP
    {
      get { return SPGrid_TR01_maxSP; }
      set
      {
        if (SPGrid_TR01_NowShowingSPMem == null) SPGrid_TR01_NowShowingSPMem = SPGrid_TR01_SP90Memori;
        SPGrid_TR01_maxSP = value;
        Thickness[] tkns = new Thickness[10];
        string[] numstr = new string[10];
        for (int i = 0; i < 10; i++)
        {
          tkns[i] = new Thickness(0, 0, 0, 0);
          numstr[i] = string.Empty;
        }
        switch (value)
        {
          case 90:
            for (int i = 0; i < 6; i++)
            {
              tkns[i] = SPGrid_TR01_SP90Thick[i];
              numstr[i] = SPGrid_TR01_SP90MemNumStr[i];
            }
            SPGrid_TR01_NowShowingSPMem = SPGrid_TR01_SP90Memori;
            break;
          case 120:
            for (int i = 0; i < 7; i++)
            {
              tkns[i] = SPGrid_TR01_SP120Thick[i];
              numstr[i] = (i * 20).ToString();
            }
            SPGrid_TR01_NowShowingSPMem = SPGrid_TR01_SP120Memori;
            break;
          case 140:
            for (int i = 0; i < 8; i++)
            {
              tkns[i] = SPGrid_TR01_SP140Thick[i];
              numstr[i] = (i * 20).ToString();
            }
            SPGrid_TR01_NowShowingSPMem = SPGrid_TR01_SP140Memori;
            break;
          case 160:
            for (int i = 0; i < 9; i++)
            {
              tkns[i] = SPGrid_TR01_SP160Thick[i];
              numstr[i] = (i * 20).ToString();
            }
            SPGrid_TR01_NowShowingSPMem = SPGrid_TR01_SP160Memori;
            break;
          case 240://120と共通
            for (int i = 0; i < 7; i++)
            {
              tkns[i] = SPGrid_TR01_SP240Thick[i];
              numstr[i] = (i * 40).ToString();
            }
            SPGrid_TR01_NowShowingSPMem = SPGrid_TR01_SP120Memori;
            break;
          case 320://160と共通
            for (int i = 0; i < 9; i++)
            {
              tkns[i] = SPGrid_TR01_SP160Thick[i];
              numstr[i] = (i * 40).ToString();
            }
            SPGrid_TR01_NowShowingSPMem = SPGrid_TR01_SP160Memori;
            break;
          case 360://未実装
            for (int i = 0; i < 10; i++)
            {
              tkns[i] = SPGrid_TR01_SP360Thick[i];
              numstr[i] = (i * 40).ToString();
            }
            break;
          default:
            SPGrid_TR01_maxSP = 160;
            for (int i = 0; i < 9; i++)
            {
              tkns[i] = SPGrid_TR01_SP160Thick[i];
              numstr[i] = (i * 20).ToString();
            }
            SPGrid_TR01_NowShowingSPMem = SPGrid_TR01_SP160Memori;
            break;
        }
        TextBlock[] tb =
        {
              SPGrid_TR01_SPMemNum0,
              SPGrid_TR01_SPMemNum1,
              SPGrid_TR01_SPMemNum2,
              SPGrid_TR01_SPMemNum3,
              SPGrid_TR01_SPMemNum4,
              SPGrid_TR01_SPMemNum5,
              SPGrid_TR01_SPMemNum6,
              SPGrid_TR01_SPMemNum7,
              SPGrid_TR01_SPMemNum8,
              SPGrid_TR01_SPMemNum9,
            };
        for (int i = 0; i < 10; i++)
        {
          if (numstr[i] != string.Empty) tb[i].Visibility = Visibility.Visible;
          else tb[i].Visibility = Visibility.Collapsed;
          tb[i].Text = numstr[i];
          tb[i].Margin = tkns[i];
        }
      }
    }
    private byte[] SPGrid_TR01_NowTime = new byte[3] { 0, 0, 0 };
    private void SPGrid_TR01_TimeSet(byte Hour, byte Minute, byte Second)
    {
      if (SPGrid_TR01_NowTime[0] != Hour) SPGrid_TR01_HourNum.Text = Hour.ToString("D");
      if (SPGrid_TR01_NowTime[1] != Minute) SPGrid_TR01_MinuteNum.Text = Minute.ToString("D2");//10進数2桁
      if (SPGrid_TR01_NowTime[2] != Second) SPGrid_TR01_SecondNum.Text = Second.ToString("D2");//10進数2桁
      SPGrid_TR01_NowTime = new byte[3] { Hour, Minute, Second };
    }

    double NowSP = 0;
    private double SPGrid_TR01_SPSet
    {
      get
      {
        return NowSP;
      }
      set
      {
        NowSP = Math.Abs(value);
        double UrGosa = NowSP - Math.Floor(NowSP);//11.9-11
        double LrGosa = Math.Ceiling(NowSP) - NowSP;//12-11.9
        //測定誤差を、数値との離れ具合でランダムに決定する。
        Random rdm = new Random();
        if (UrGosa < 0.15 && rdm.Next((int)Math.Floor(UrGosa * 10)) == 0)
        {
          NowSP -= 1;
        }
        if(LrGosa < 0.15 && rdm.Next((int)Math.Floor(LrGosa * 10)) == 0)
        {
          NowSP += 1;
        }
        NowSP = Math.Ceiling(NowSP);
        SPGrid_TR01_SpeedNumBlock.Text = NowSP.ToString();
        if (NowSP >= SPGrid_TR01_MaxSP && NowSP < 360) SPGrid_TR01_MaxSPChangeEv(null, null);
        double Angle = 0;
        switch (SPGrid_TR01_MaxSP)
        {
          case 90:
            Angle = -40;
            Angle += 2.6 * NowSP;
            break;
          case 120:
            Angle = -30;
            Angle += 2 * NowSP;
            break;
          case 140:
            Angle = -36;
            Angle += 1.8 * NowSP;
            break;
          case 160:
            Angle = -30;
            Angle += 1.5 * NowSP;
            break;
          case 240:
            Angle = -30;
            Angle += 2 * Math.Round(NowSP / 2);
            break;
          case 320:
            Angle = -30;
            Angle += 1.5 * Math.Round(NowSP / 2);
            break;
          case 360:
            Angle = -36;
            Angle += 2.8 * Math.Round(NowSP / 4);
            break;
        }
        SPGrid_TR01_SPHari.Angle = Angle;
      }
    }


    //RWRW
    private void SPGrid_TR01_PressSet(double MR, double ER, double BC, double BP)
    {
      if (!SPGrid_TR01_IsTickDefault)
      {
        MR = Math.Ceiling(MR / 100) * 100;
        ER = Math.Ceiling(ER / 100) * 100;
        BC = Math.Ceiling(BC / 100) * 100;
        BP = Math.Ceiling(BP / 100) * 100;
      }
      //*2.6 / 10kPa
      SPGrid_TR01_MRHari.Angle = MR * 0.26 - 40;
      SPGrid_TR01_ERHari.Angle = ER * 0.26 - 40;
      SPGrid_TR01_BCHari.Angle = BC * 0.26 - 40;
      SPGrid_TR01_BPHari.Angle = BP * 0.26 - 40;
    }

    private double SPGrid_TR01_CurrentSet
    {
      set
      {
        double I = value;
        double PlusW = 0;
        double MinusW = 0;
        if (!SPGrid_TR01_IsTickDefault) I = Math.Ceiling(I / 10) * 10;

        if (I >= 0) { PlusW = I * 0.3; MinusW = 0; }
        if (I < 0) { MinusW = Math.Abs(I) * 0.3; PlusW = 0; }
        //SPGrid_TR01_PlusCurrentBar.Width = PlusW;
        //SPGrid_TR01_MinusCurrentBar.Width = MinusW;
        SPGrid_TR01_PlusCurrentBar.Margin = new Thickness(50 + PlusW, 130, 0, 0);
        SPGrid_TR01_MinusCurrentBar.Margin = new Thickness(50 + MinusW, 230, 0, 0);
      }
    }

    private double SPGrid_TR01_LocationSet
    {
      set => SPGrid_TR01_LocationTextBlock.Text = value.ToString("F1");
    }

    private int SPGrid_TR01_ReverserSet
    {
      set
      {
        int num = value;
        Visibility[] vs = new Visibility[3] { Visibility.Collapsed, Visibility.Collapsed, Visibility.Collapsed };
        if (num < 0) vs[2] = Visibility.Visible;
        else if (num > 0) vs[0] = Visibility.Visible;
        else vs[1] = Visibility.Visible;
        SPGrid_TR01_RHandF.Visibility = vs[0];
        SPGrid_TR01_RHandN.Visibility = vs[1];
        SPGrid_TR01_RHandR.Visibility = vs[2];
      }
    }
    private void SPGrid_TR01_HandleSet(int B, int P)
    {
      SPGrid_TR01_BHandNum.Text = B.ToString("D");
      SPGrid_TR01_PHandNum.Text = P.ToString("D");
    }
    private bool SPGrid_TR01_IsDoorClosed
    {
      set
      {
        if (value) SPGrid_TR01_IsDoorClosedPath.Visibility = Visibility.Visible;
        else SPGrid_TR01_IsDoorClosedPath.Visibility = Visibility.Collapsed;
      }
    }
  }
}
