using Microsoft.Win32;
using paint;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Paint
{
    public partial class MainWindow : Window
    {
        public static string log = "ExtraFiles.log";
        public bool savedImage = false;
        public bool openedImage = false;
        public string path;
        public BitmapImage image = new BitmapImage();
        public double height, width, dpiX, dpiY;
        public bool ctrlDown = false;
        public string openFileName;
        const double ScaleRate = 1.1;
        public MainWindow()
        {
            InitializeComponent();
            CheckExtraFiles();
        }

        public void save_Click(object sender, RoutedEventArgs e)
        {
            SaveFunction();
        }

        public void open_Click(object sender, RoutedEventArgs e)
        {
            OpenFunction();
        }

        private void newfile_Click(object sender, RoutedEventArgs e)
        {
            NewFileFunction();
        }

        private void canvas_Drop(object sender, DragEventArgs e)
        {
            ImageSource imageSource = e.Source as ImageSource;
                if (imageSource != null)
                {
                    ImageBrush img = new ImageBrush()
                    {
                        ImageSource = imageSource
                    };
                    canvas.Background = img;
                }
        }

        private void canvas_MouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            if(ctrlDown == true)
            {
                if (e.Delta > 0)
                {
                    st.ScaleX *= ScaleRate;
                }
                else
                {
                    st.ScaleX /= ScaleRate;
                }
                st.ScaleX = Math.Round(st.ScaleX, 2);
                if(st.ScaleX > 0.95 && st.ScaleX < 1.05)
                {
                    st.ScaleX = 1;
                }
                st.ScaleY = st.ScaleX;
                zoomMenu.Header = "Zoom: " + Convert.ToString(st.ScaleX * 100) + "%";
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                ctrlDown = true;
            }
            if(e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SaveFunction();
            }
            if(e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
            {
                OpenFunction();
            }
            if(e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                NewFileFunction();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                ctrlDown = false;
            }
        }
    }
}