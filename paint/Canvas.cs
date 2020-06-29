using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Paint
{
    public partial class MainWindow : Window
    {
        Point currentPoint = new Point();

        public void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                currentPoint = e.GetPosition(this);
        }

        public void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line line = new Line()
                {
                    Stroke = SystemColors.WindowFrameBrush,
                    X1 = currentPoint.X - 20,
                    Y1 = currentPoint.Y - 40,
                    X2 = e.GetPosition(this).X - 20,
                    Y2 = e.GetPosition(this).Y - 40
                };
                currentPoint = e.GetPosition(this);

                canvas.Children.Add(line);
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Line line = new Line()
                {
                    StrokeThickness = 10,
                    Stroke = Brushes.DarkCyan,
                    X1 = currentPoint.X - 20,
                    Y1 = currentPoint.Y - 40,
                    X2 = e.GetPosition(this).X - 20,
                    Y2 = e.GetPosition(this).Y - 40
                };
                currentPoint = e.GetPosition(this);

                canvas.Children.Add(line);
            }
        }
    }
}