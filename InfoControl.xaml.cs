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

namespace Cliver.CisteraScreenCapture
{
    /// <summary>
    /// Interaction logic for InfoControl.xaml
    /// </summary>
    public partial class InfoControl : UserControl
    {
        public InfoControl()
        {
            InitializeComponent();
        }

        internal InfoControl(string title, string text, string image_url, string action_name, Action action, bool close_on_button_click)
        {
            InitializeComponent();

            this.title.Text = title;
            this.text.Text = text;
            //var request = System.Net.WebRequest.Create(image_url);
            //request.BeginGetResponse((r) =>
            //{
            //    if (!r.IsCompleted)
            //        return;
            //    using (var stream = ((System.Net.WebRequest)r.AsyncState).EndGetResponse(r).GetResponseStream())
            //    {
            //        image.Image = Bitmap.FromStream(stream);
            //    }
            //}, request);
            if (image_url != null)
            {
                if (!image_url.Contains(":"))
                    image_url = Log.AppDir + image_url;
                try
                {
                    image.Source = new BitmapImage(new Uri(image_url));
                }
                catch
                {
                }
            }
            else
            {
                image_container.Width = 0;
                image_container.Margin = new Thickness( 0);
            }
            if (action_name != null)
                this.button.Content = action_name;
            this.button.Click += (object sender, RoutedEventArgs e) =>
            {
                action?.Invoke();
                if (close_on_button_click)
                {
                    Window w = Window.GetWindow(this);
                    try
                    {//might be closed already
                        w.Close();
                    }
                    catch { }
                }
            };
        }
    }
}
