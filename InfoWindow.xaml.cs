/********************************************************************************************
        Author: Sergey Stoyan
        sergey.stoyan@gmail.com
        http://www.cliversoft.com
********************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Media;
using System.Windows.Interop;
using System.Threading;
using System.Windows.Media.Animation;

namespace Cliver.CisteraScreenCapture
{
    public partial class InfoWindow : Window
    {
        static readonly List<InfoWindow> ws = new List<InfoWindow>();

        static InfoWindow()
        {
        }

        static Window invisible_owner_w = null;
        static System.Windows.Threading.Dispatcher dispatcher = null;

        public static InfoWindow Create(string text, string image_url, string action_name, Action action, string sound_file = null, Brush box_brush = null, Brush button_brush = null)
        {
            return Create(ProgramRoutines.GetAppName(), text, image_url, action_name, action, sound_file, box_brush, button_brush);
        }

        public static InfoWindow Create(string title, string text, string image_url, string action_name, Action action, string sound_file = null, Brush box_brush = null, Brush button_brush = null)
        {
            InfoWindow w = null;

            if (text.Length > Settings.View.InfoToastMaxTextLength)
                text = text.Remove(Settings.View.InfoToastMaxTextLength, text.Length - Settings.View.InfoToastMaxTextLength) + "<...>";

            Action a = () =>
            {
                w = new InfoWindow(title, text, image_url, action_name, action);
                w.SetAppearance(box_brush, button_brush);
                WindowInteropHelper h = new WindowInteropHelper(w);
                h.EnsureHandle();
                w.Show();
                ThreadRoutines.StartTry(() =>
                {
                    Thread.Sleep(Settings.View.InfoToastLifeTimeInSecs * 1000);
                    w.Dispatcher.BeginInvoke((Action)(() => { w.Close(); }));
                });
                if (string.IsNullOrWhiteSpace(sound_file))
                    sound_file = Settings.View.InfoSoundFile;
                SoundPlayer sp = new SoundPlayer(sound_file);
                sp.Play();
            };

            lock (ws)
            {
                if (dispatcher == null)
                {//!!!the following code does not work in static constructor because creates a deadlock!!!
                    Thread t = ThreadRoutines.StartTry(() =>
                    {
                        if (invisible_owner_w == null)
                        {//this window is used to hide notification windows from Alt+Tab panel
                            invisible_owner_w = new Window();
                            invisible_owner_w.Width = 0;
                            invisible_owner_w.Height = 0;
                            invisible_owner_w.WindowStyle = WindowStyle.ToolWindow;
                            invisible_owner_w.ShowInTaskbar = false;
                            invisible_owner_w.Show();
                            invisible_owner_w.Hide();
                        }

                        if (dispatcher == null)
                        {
                            dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                            System.Windows.Threading.Dispatcher.Run();
                        }
                    }, null, null, true, ApartmentState.STA);
                }
            }
            if (dispatcher == null && !SleepRoutines.WaitForCondition(() => { return dispatcher != null; }, 3000))
                throw new Exception("Could not get dispatcher.");
            dispatcher.Invoke(a);
            return w;
        }
        static Thread t = null;

        InfoWindow()
        {
            InitializeComponent();
        }

        internal void SetAppearance(Brush box_brush, Brush button_brush)
        {
            //InfoControl ic = (InfoControl)FindName("InfoControl");
            InfoControl ic = (InfoControl)grid.Children[0];
            if (box_brush != null)
                ic.box.Background = box_brush;
            if (button_brush != null)
                ic.button.Background = button_brush;
        }

        InfoWindow(string title, string text, string image_url, string action_name, Action action)
        {
            InitializeComponent();

            Loaded += Window_Loaded;
            Closing += Window_Closing;
            Closed += Window_Closed;
            PreviewMouseDown += delegate
            {
                try
                {//might be closed already
                    Close();
                }
                catch { }
            };

            Topmost = true;
            Owner = invisible_owner_w;

            InfoControl ic = new InfoControl(title, text, image_url, action_name, action, true);
            ic.Name = "InfoControl";
            grid.Children.Add(ic);

            //LayoutTransform = new ScaleTransform(0.1, 0.1);
            //UpdateLayout();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var a = new DoubleAnimation(0, 1, (Duration)TimeSpan.FromMilliseconds(300));
            this.BeginAnimation(UIElement.OpacityProperty, a);

            Rect wa = SystemParameters.WorkArea;
            Storyboard sb;
            DoubleAnimation da;
            lock (ws)
            {
                if (ws.Count > 0)
                {
                    Window w = ws[ws.Count - 1];
                    this.Top = w.Top - this.ActualHeight;
                }
                else
                    this.Top = wa.Bottom - this.ActualHeight - Settings.View.InfoToastBottom;

                ws.Add(this);

                if (Top < 0)
                {
                    foreach (Window w in ws)
                    {
                        sb = new Storyboard();
                        da = new DoubleAnimation(w.Top + this.Height, (Duration)TimeSpan.FromMilliseconds(300));
                        Storyboard.SetTargetProperty(da, new PropertyPath("(Top)")); //Do not miss the '(' and ')'
                        sb.Children.Add(da);
                        w.BeginStoryboard(sb);
                    }
                }
            }
            sb = new Storyboard();
            da = new DoubleAnimation(wa.Right, wa.Right - Width - Settings.View.InfoToastRight, (Duration)TimeSpan.FromMilliseconds(300));
            Storyboard.SetTargetProperty(da, new PropertyPath("(Left)")); //Do not miss the '(' and ')'
            sb.Children.Add(da);
            BeginStoryboard(sb);
        }

        //new double Top
        //{
        //    get
        //    {
        //        return (double)this.Invoke(() => { return base.Top; });
        //    }
        //    set
        //    {
        //        base.Top = value;
        //    }
        //}

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            var a = new System.Windows.Media.Animation.DoubleAnimation(0, (Duration)TimeSpan.FromMilliseconds(300));
            a.Completed += (s, _) => this.Close();
            this.BeginAnimation(UIElement.OpacityProperty, a);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            lock (ws)
            {
                Rect wa = System.Windows.SystemParameters.WorkArea;
                if (Top + Height > wa.Bottom)
                    return;

                int i = ws.IndexOf(this);
                for (int j = i + 1; j < ws.Count; j++)
                {
                    Window w = ws[j];
                    Storyboard sb = new Storyboard();
                    DoubleAnimation da = new DoubleAnimation(w.Top + this.Height, (Duration)TimeSpan.FromMilliseconds(300));
                    Storyboard.SetTargetProperty(da, new PropertyPath("(Top)")); //Do not miss the '(' and ')'
                    sb.Children.Add(da);
                    w.BeginStoryboard(sb);
                }

                ws.Remove(this);
            }
        }
    }
}