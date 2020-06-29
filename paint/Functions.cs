using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Paint
{

    public partial class MainWindow : Window
    {

        public void CheckExtraFiles()
        {
            if (!File.Exists(log))
            {
                FileStream f = new FileStream(log, FileMode.Create);
                File.Create(f.Name);
                f.Close();
                f.Dispose();
            }
            StreamReader sr = new StreamReader(log);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                try
                {
                    File.Delete(line);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error: Could not delete file (" + line + "). Original error:" + ex.Message);
                }
            }
            sr.Close();
            sr.Dispose();
            File.Create(log);
        }

        public void NewFileFunction()
        {
            if (openedImage == true)
            {
                MainWindow newWindow = new MainWindow();
                //newWindow.Activate();
                newWindow.Show();
                if(savedImage == true)
                {
                    if(MessageBox.Show("Image has been saved. Do you still want to keep this window opened?",
                        "Closing",
                        MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    {
                        Window.Close();
                    }   
                }
                else
                {
                    if (MessageBox.Show("Image has not been saved. Do you want to close it wihout saving it?",
                        "Closing",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Window.Close();
                    }
                    else
                    {
                        Window.Activate();
                        SaveFunction();
                    }
                }
            }
        }

        public void OpenFunction()
        {
            OpenFileDialog openFile = new OpenFileDialog()
            {
                Filter = "Image Files(*.png, *.jpg, *.bmp, *.gif) |*.png; *.jpg; *.bmp; *.gif| All files(*.*) | *.*",
                Title = "Open Image"
            };
            if (openFile.ShowDialog() == true && openedImage != true)
            {
                try
                {
                    if ((openFile.OpenFile()) != null)
                    {
                        newfile.IsEnabled = true;
                        openedImage = true;
                        open.IsEnabled = false;
                        save.IsEnabled = true;
                        openFileName = openFile.FileName;
                        image.BeginInit();
                        image.UriSource = new Uri(openFileName, UriKind.Relative);
                        image.EndInit();
                        ImageBrush brush = new ImageBrush()
                        {
                            ImageSource = new BitmapImage(new Uri(openFileName, UriKind.Relative))
                        };
                        height = image.Height;
                        width = image.Width;
                        dpiX = image.DpiX;
                        dpiX = image.DpiY;
                        brush.Stretch = Stretch.Uniform;
                        canvas.Background = brush;
                        canvas.Height = height;
                        canvas.Width = width;
                        Window.MinHeight = 20 + canvas.Height + canvas.Margin.Bottom + canvas.Margin.Top;
                        Window.MinWidth = canvas.Width + canvas.Margin.Right + canvas.Margin.Left;
                        Window.Height = 20 + canvas.Height + canvas.Margin.Bottom + canvas.Margin.Top + 80;
                        Window.Width = canvas.Width + canvas.Margin.Right + canvas.Margin.Left + 20;
                        FillRectangles();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        public void SaveFunction()
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap(image.PixelWidth * 2, image.PixelHeight * 2, 72, 72, PixelFormats.Pbgra32);
            rtb.Render(canvas);
            rtb.Render(rectangle1);
            rtb.Render(rectangle2);
            rtb.Render(rectangle3);
            rtb.Render(rectangle4);
            rtb.Render(rectangle5);
            rtb.Render(rectangle6);
            rtb.Render(rectangle7);
            rtb.Render(rectangle8);
            //Bitmap bit = new Bitmap(openFileName);
            //rtb.Render((RenderTargetBitmap)TrimBitmap(bit));
            SaveFileDialog saveFileDialog1 = new SaveFileDialog()
            {
                Filter = "PNG Image|*.png|JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif",
                Title = "Save Image"
            };
            saveFileDialog1.ShowDialog();
            try
            {
                FileStream fs = (FileStream)saveFileDialog1.OpenFile();
                path = fs.Name;
                savedImage = true;
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                        pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
                        pngEncoder.Save(fs);
                        break;

                    case 2:
                        JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
                        jpegEncoder.Frames.Add(BitmapFrame.Create(rtb));
                        jpegEncoder.Save(fs);
                        break;

                    case 3:
                        BmpBitmapEncoder bmpEncoder = new BmpBitmapEncoder();
                        bmpEncoder.Frames.Add(BitmapFrame.Create(rtb));
                        bmpEncoder.Save(fs);
                        break;

                    case 4:
                        GifBitmapEncoder gifEncoder = new GifBitmapEncoder();
                        gifEncoder.Frames.Add(BitmapFrame.Create(rtb));
                        gifEncoder.Save(fs);
                        break;
                }
                fs.Close();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not write file to disk. Original error: " + ex.Message);
            }
            if(path != null)
            {
                Bitmap bitmap = new Bitmap(path);
                TrimBitmap(bitmap, path, dpiY);
                
                //File.Delete(path);
            }
        }

        static void TrimBitmap(Bitmap source, string fs, double dpiY)
        {
            Rectangle srcRect = default(Rectangle);
            BitmapData data = null;
            try
            {
                data = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                byte[] buffer = new byte[data.Height * data.Stride];
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);

                int xMin = int.MaxValue,
                    xMax = int.MinValue,
                    yMin = int.MaxValue,
                    yMax = int.MinValue;

                bool foundPixel = false;

                // Find xMin
                for (int x = 0; x < data.Width; x++)
                {
                    bool stop = false;
                    for (int y = 0; y < data.Height; y++)
                    {
                        byte alpha = buffer[y * data.Stride + 4 * x + 3];
                        if (alpha != 0)
                        {
                            xMin = x;
                            stop = true;
                            foundPixel = true;
                            break;
                        }
                    }
                    if (stop)
                        break;
                }

                // Image is empty...
                //if (!foundPixel)
                  //  return null;

                // Find yMin
                for (int y = 0; y < data.Height; y++)
                {
                    bool stop = false;
                    for (int x = xMin; x < data.Width; x++)
                    {
                        byte alpha = buffer[y * data.Stride + 4 * x + 3];
                        if (alpha != 0)
                        {
                            yMin = y;
                            stop = true;
                            break;
                        }
                    }
                    if (stop)
                        break;
                }

                // Find xMax
                for (int x = data.Width - 1; x >= xMin; x--)
                {
                    bool stop = false;
                    for (int y = yMin; y < data.Height; y++)
                    {
                        byte alpha = buffer[y * data.Stride + 4 * x + 3];
                        if (alpha != 0)
                        {
                            xMax = x;
                            stop = true;
                            break;
                        }
                    }
                    if (stop)
                        break;
                }

                // Find yMax
                for (int y = data.Height - 1; y >= yMin; y--)
                {
                    bool stop = false;
                    for (int x = xMin; x <= xMax; x++)
                    {
                        byte alpha = buffer[y * data.Stride + 4 * x + 3];
                        if (alpha != 0)
                        {
                            yMax = y;
                            stop = true;
                            break;
                        }
                    }
                    if (stop)
                        break;
                }

                srcRect = Rectangle.FromLTRB(xMin, yMin, xMax, yMax);
            }
            finally
            {
                if (data != null)
                    source.UnlockBits(data);
            }
            
            Bitmap dest = new Bitmap(srcRect.Width, srcRect.Height);
            dest.SetResolution(96, 96);
            Rectangle destRect = new Rectangle(0, 0, srcRect.Width, srcRect.Height);
            using (Graphics graphics = Graphics.FromImage(dest))
            {
                graphics.DrawImage(source, destRect, srcRect, GraphicsUnit.Pixel);
            }
            dest.Save("aa.png" , ImageFormat.Png);
            File.Copy("aa.png", fs.Replace(".png", "-NEW.png"));
            
            File.AppendAllText(log, fs);
            File.AppendAllText(log, Environment.NewLine);
        }        

        public void ScaleCanvas(double ScaleRate)
        {
            ScaleTransform scale = new ScaleTransform(canvas.LayoutTransform.Value.M11 * ScaleRate, canvas.LayoutTransform.Value.M22 * ScaleRate);
            canvas.LayoutTransform = scale;
            canvas.UpdateLayout();
        }

        private static string HexConverter(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        private void FillRectangles()
        {
            Random random = new Random();
            Bitmap bitmap = new Bitmap(openFileName);


            var converter = new BrushConverter();
            int x = random.Next(Convert.ToInt32(bitmap.Width));
            int y = random.Next(Convert.ToInt32(bitmap.Height) / 8);
            System.Drawing.Color color = bitmap.GetPixel(x, y);
            var b = (System.Windows.Media.Brush)converter.ConvertFromString(HexConverter(color));
            rectangle1.Fill = b;

            x = random.Next(Convert.ToInt32(bitmap.Width));
            y = random.Next(Convert.ToInt32(bitmap.Height) / 8, Convert.ToInt32(bitmap.Height) / 4);
            color = bitmap.GetPixel(x, y);
            b = (System.Windows.Media.Brush)converter.ConvertFromString(HexConverter(color));
            rectangle2.Fill = b;

            x = random.Next(Convert.ToInt32(bitmap.Width));
            y = random.Next(Convert.ToInt32(bitmap.Height) / 4, Convert.ToInt32(bitmap.Height) * 3 / 8);
            color = bitmap.GetPixel(x, y);
            b = (System.Windows.Media.Brush)converter.ConvertFromString(HexConverter(color));
            rectangle3.Fill = b;

            x = random.Next(Convert.ToInt32(bitmap.Width));
            y = random.Next(Convert.ToInt32(bitmap.Height) * 3 / 8, Convert.ToInt32(bitmap.Height) / 2);
            color = bitmap.GetPixel(x, y);
            b = (System.Windows.Media.Brush)converter.ConvertFromString(HexConverter(color));
            rectangle4.Fill = b;

            x = random.Next(Convert.ToInt32(bitmap.Width));
            y = random.Next(Convert.ToInt32(bitmap.Height) / 2, Convert.ToInt32(bitmap.Height) * 3 / 4);
            color = bitmap.GetPixel(x, y);
            b = (System.Windows.Media.Brush)converter.ConvertFromString(HexConverter(color));
            rectangle5.Fill = b;

            x = random.Next(Convert.ToInt32(bitmap.Width));
            y = random.Next(Convert.ToInt32(bitmap.Height) * 3 / 4, Convert.ToInt32(bitmap.Height) * 7 / 8);
            color = bitmap.GetPixel(x, y);
            b = (System.Windows.Media.Brush)converter.ConvertFromString(HexConverter(color));
            rectangle6.Fill = b;

            x = random.Next(Convert.ToInt32(bitmap.Width));
            y = random.Next(Convert.ToInt32(bitmap.Height) * 7 / 8, Convert.ToInt32(bitmap.Height));
            color = bitmap.GetPixel(x, y);
            b = (System.Windows.Media.Brush)converter.ConvertFromString(HexConverter(color));
            rectangle7.Fill = b;

            x = random.Next(Convert.ToInt32(bitmap.Width));
            y = random.Next(Convert.ToInt32(bitmap.Height) * 7 / 8, Convert.ToInt32(bitmap.Height));
            color = bitmap.GetPixel(x, y);
            b = (System.Windows.Media.Brush)converter.ConvertFromString(HexConverter(color));
            rectangle8.Fill = b;
            
        }
    }
}