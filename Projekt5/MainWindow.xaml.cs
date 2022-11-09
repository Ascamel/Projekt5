using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
using Path = System.IO.Path;
using Encoder = System.Drawing.Imaging.Encoder;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Color = System.Drawing.Color;
using System.IO;
using System.Collections;

namespace Projekt5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int[] red = null;
        private int[] green = null;
        private int[] blue = null;

        string imgPath;

        Bitmap picture;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadImageButtonClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                imgPath = op.FileName;
                MyImage.Source = new BitmapImage(new Uri(imgPath));

                picture = new(imgPath);               
            }

            red = new int[256];
            green = new int[256];
            blue = new int[256];
            for (int x = 0; x < picture.Width; x++)
            {
                for (int y = 0; y < picture.Height; y++)
                {
                    Color pixel = picture.GetPixel(x, y);
                    red[pixel.R]++;
                    green[pixel.G]++;
                    blue[pixel.B]++;
                }
            }

        }

        /// <summary>
        /// Oblicza tablice LUT dla histogramu podanej składowej
        /// </summary>
        /// <param name="values">histogram dla składowej</param>
        /// <returns>tablica LUT do rozciagniecia histogramu</returns>
        private int[] calculateLUT(int[] values)
        {
            //poszukaj wartości minimalnej
            int minValue = 0;
            for (int i = 0; i < 256; i++)
            {
                if (values[i] != 0)
                {
                    minValue = i;
                    break;
                }
            }

            //poszukaj wartości maksymalnej
            int maxValue = 255;
            for (int i = 255; i >= 0; i--)
            {
                if (values[i] != 0)
                {
                    maxValue = i;
                    break;
                }
            }

            //przygotuj tablice zgodnie ze wzorem
            int[] result = new int[256];
            double a = 255.0 / (maxValue - minValue);
            for (int i = 0; i < 256; i++)
            {
                result[i] = (int)(a * (i - minValue));
            }

            return result;
        }

        private void TranformButtonClick(object sender, RoutedEventArgs e)
        {

            if(Expand.IsChecked.GetValueOrDefault())
            {
                //Tablice LUT dla skladowych
                int[] LUTred = calculateLUT(red);
                int[] LUTgreen = calculateLUT(green);
                int[] LUTblue = calculateLUT(blue);

                //Przetworz obraz i oblicz nowy histogram
                red = new int[256];
                green = new int[256];
                blue = new int[256];
                Bitmap oldBitmap = new(picture);
                Bitmap newBitmap = new(picture);
                for (int x = 0; x < picture.Width; x++)
                {
                    for (int y = 0; y < picture.Height; y++)
                    {
                        Color pixel = picture.GetPixel(x, y);
                        Color newPixel = Color.FromArgb(LUTred[pixel.R], LUTgreen[pixel.G], LUTblue[pixel.B]);
                        picture.SetPixel(x, y, newPixel);
                        red[newPixel.R]++;
                        green[newPixel.G]++;
                        blue[newPixel.B]++;
                    }
                }

                MemoryStream memoryStream = new();
                picture.Save(memoryStream, ImageFormat.Png);
                BitmapImage bitmapImage = new();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                MyImage.Source = bitmapImage;
            }
            else if(Equalize.IsChecked.GetValueOrDefault())
            {
                int size = picture.Width * picture.Height;
                int[] red = new int[256];
                int[] green = new int[256];
                int[] blue = new int[256];
                double[] rdense = new double[256];
                double[] gdense = new double[256];
                double[] bdense = new double[256];
                for (int i = 0; i < picture.Width; ++i)
                    for (int j = 0; j < picture.Height; ++j)
                    {
                        System.Drawing.Color pixel = picture.GetPixel(i, j);
                        red[Convert.ToInt16(pixel.R)] += 1;
                        green[Convert.ToInt16(pixel.G)] += 1;
                        blue[Convert.ToInt16(pixel.B)] += 1;
                    }
                for (int i = 0; i < 256; i++)
                {
                    rdense[i] = (red[i] * 1.0) / size;
                    gdense[i] = (green[i] * 1.0) / size;
                    bdense[i] = (blue[i] * 1.0) / size;
                }

                for (int i = 1; i < 256; i++)
                {
                    rdense[i] += rdense[i - 1];
                    gdense[i] += gdense[i - 1];
                    bdense[i] += bdense[i - 1];
                }

                for (int i = 0; i < picture.Width; ++i)
                    for (int j = 0; j < picture.Height; ++j)
                    {
                        System.Drawing.Color pixel = picture.GetPixel(i, j);
                        int rr = Convert.ToInt16(pixel.R),
                            gg = Convert.ToInt16(pixel.G),
                            bb = Convert.ToInt16(pixel.B);
                        int r,
                            g,
                            b;
                        if (rr == 0)
                        {
                            r = 0;
                        }
                        else
                        {
                            r = Convert.ToInt16(
                                rdense[Convert.ToInt16(pixel.R)] * Convert.ToInt16(pixel.R)
                            );
                        }
                        if (gg == 0)
                        {
                            g = 0;
                        }
                        else
                        {
                            g = Convert.ToInt16(
                                rdense[Convert.ToInt16(pixel.G)] * Convert.ToInt16(pixel.G)
                            );
                        }
                        if (bb == 0)
                        {
                            b = 0;
                        }
                        else
                        {
                            b = Convert.ToInt16(
                                rdense[Convert.ToInt16(pixel.B)] * Convert.ToInt16(pixel.B)
                            );
                        }

                        pixel = System.Drawing.Color.FromArgb(r, g, b);
                        picture.SetPixel(i, j, pixel);
                    }

                MemoryStream memoryStream = new();
                picture.Save(memoryStream, ImageFormat.Png);
                BitmapImage bitmapImage = new();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                MyImage.Source = bitmapImage;
            }
            if (red == null)
            {
                return;
            }

        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }

        private void TranformButtonClick2(object sender, RoutedEventArgs e)
        {

            if (HandButton.IsChecked.GetValueOrDefault())
            {
                int threshold = Convert.ToInt32(HandText.Text);
                int average = 0;

                for (int i = 0; i < picture.Width; ++i)
                {
                    for (int j = 0; j < picture.Height; ++j)
                    {
                        System.Drawing.Color pixel = picture.GetPixel(i, j);
                        int r = Convert.ToInt16(pixel.R),
                            g = Convert.ToInt16(pixel.G),
                            b = Convert.ToInt16(pixel.B);

                        average = (r+g+b)/3;

                        if (average > threshold)
                        {
                            pixel = Color.White;
                            picture.SetPixel(i, j, pixel);
                        }
                        else
                        {
                            pixel = Color.Black;
                            picture.SetPixel(i, j, pixel);
                        }
                    }
                }
            }
            else if (PercentBlack.IsChecked.GetValueOrDefault())
            {
                List<MyValue> LUT = new();

                MyValue myValue;

                int percentage = Convert.ToInt32(HandText.Text);
                int average = 0;

                for (int i = 0; i < picture.Width; ++i)
                {
                    for (int j = 0; j < picture.Height; ++j)
                    {
                        System.Drawing.Color pixel = picture.GetPixel(i, j);
                        int r = Convert.ToInt16(pixel.R),
                            g = Convert.ToInt16(pixel.G),
                            b = Convert.ToInt16(pixel.B);

                        average = (r+g+b)/3;

                        myValue = new(i, j, average);
                        LUT.Add(myValue);
                    }
                }

                List<MyValue> SortedList = LUT.OrderBy(o => o.value).ToList();
                int tmp = 0;

                for (int i = 0; i<percentage*SortedList.Count*0.01;i++)
                {
                    picture.SetPixel(SortedList[i].i, SortedList[i].j, Color.Black);
                    tmp++;
                }

                for(int i = tmp ;i < SortedList.Count; i++)
                {
                    picture.SetPixel(SortedList[i].i, SortedList[i].j, Color.White);
                }
            }
            else if(IterationButton.IsChecked.GetValueOrDefault())
            {
                List<MyValue> LUT = new();

                MyValue myValue;

                int threshold = 127;


                int average = 0;

                for (int i = 0; i < picture.Width; ++i)
                {
                    for (int j = 0; j < picture.Height; ++j)
                    {
                        System.Drawing.Color pixel = picture.GetPixel(i, j);
                        int r = Convert.ToInt16(pixel.R),
                            g = Convert.ToInt16(pixel.G),
                            b = Convert.ToInt16(pixel.B);

                        average = (r + g + b) / 3;

                        myValue = new(i, j, average);
                        LUT.Add(myValue);
                    }
                }

                List<MyValue> SortedList = LUT.OrderBy(o => o.value).ToList();

                int TW = ThresholdWhite(SortedList,threshold);
                int TB = ThresholdBlack(SortedList,threshold);

                while(TW != TB)
                {
                    threshold = (TW + TB) / 2;
                    TW = ThresholdWhite(SortedList, threshold);
                    TB = ThresholdBlack(SortedList, threshold);
                }

            }

            MemoryStream memoryStream = new();
            picture.Save(memoryStream, ImageFormat.Png);
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            MyImage2.Source = bitmapImage;
        }

        public int ThresholdBlack(List<MyValue> myValues,int threshold)
        {
            int newthreshold = 0;
            int sum = 0;
            int iter = 0;

            for(int i = 0; i < threshold * myValues.Count * 0.01; i++)
            {
                sum+= myValues[i].value;
                iter++;
            }

            newthreshold = sum / iter;

            return newthreshold;
        }

        public int ThresholdWhite(List<MyValue> myValues, int threshold)
        {
            int newthreshold = 0;
            int sum = 0;
            int iter = 0;

            for (int i = threshold; i < myValues.Count; i++)
            {
                sum += myValues[i].value;
                iter++;
            }

            newthreshold = sum / iter;

            return newthreshold;
        }

        private void LoadImageButtonClicked2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                imgPath = op.FileName;
                MyImage2.Source = new BitmapImage(new Uri(imgPath));

                picture = new(imgPath);
            }
        }
    }

    public class MyValue
    {
        public int i { get; set; }
        public int j { get; set; }
        public int value { get; set; }

        public MyValue(int i, int j, int value)
        {
            this.i=i;
            this.j=j;
            this.value=value;
        }
    }
}
