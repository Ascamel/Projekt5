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
using System.Threading;

namespace Projekt5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

        }

        public int[] CalculateLUT(int[] pixels)
        {
            int[] LUT = new int[256];

            int min = Array.FindIndex(pixels, x => x > 0);
            int max = Array.FindLastIndex(pixels, x => x > 0);

            for (int i = 0; i < 256; i++)
            {
                LUT[i] = (255 / (max - min)) * (i - min);
            }
            return LUT;
        }

        public int[] CalculateLUT2(int[] pixels, int size)
        {
            int[] LUT = new int[256];
            float[] DISTRIBUTION = new float[256];
            int min = Array.FindIndex(pixels, x => x > 0);

            float sum = 0.0F;

            for(int i = 0; i < 256; i++)
            {
                DISTRIBUTION[i] = (pixels[i]+sum)/size;
                sum += pixels[i];
            }

            for(int i = 0; i < 256; i++)
            {
                LUT[i] = (int)(((DISTRIBUTION[i] - DISTRIBUTION[min]) / 1 - DISTRIBUTION[min]) * 255);
            }


            return LUT;
        }

        private void TranformButtonClick(object sender, RoutedEventArgs e)
        {

            if (Expand.IsChecked.GetValueOrDefault())
            {
                int[] LUTR = new int[256];
                int[] LUTG = new int[256];
                int[] LUTB = new int[256];

                int[] RValues = new int[256];
                int[] GValues = new int[256];
                int[] BValues = new int[256];


                for (int i = 0; i < picture.Width; ++i)
                {
                    for (int j = 0; j < picture.Height; ++j)
                    {
                        System.Drawing.Color pixel = picture.GetPixel(i, j);

                        RValues[pixel.R]++;
                        GValues[pixel.G]++;
                        BValues[pixel.B]++;
                    }
                }

                LUTR = CalculateLUT(RValues);
                LUTG = CalculateLUT(GValues);
                LUTB = CalculateLUT(BValues);

                for (int i = 0; i < picture.Width; ++i)
                {
                    for (int j = 0; j < picture.Height; ++j)
                    {
                        System.Drawing.Color pixel = picture.GetPixel(i, j);

                        int r = LUTR[pixel.R];
                        int g = LUTG[pixel.G];
                        int b = LUTB[pixel.B];

                        System.Drawing.Color newpixel = Color.FromArgb(r, g, b);


                        picture.SetPixel(i, j, newpixel);
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
            else if (Equalize.IsChecked.GetValueOrDefault())
            {
                int[] LUTR = new int[256];
                int[] LUTG = new int[256];
                int[] LUTB = new int[256];

                int[] RValues = new int[256];
                int[] GValues = new int[256];
                int[] BValues = new int[256];


                for (int i = 0; i < picture.Width; ++i)
                {
                    for (int j = 0; j < picture.Height; ++j)
                    {
                        System.Drawing.Color pixel = picture.GetPixel(i, j);

                        RValues[pixel.R]++;
                        GValues[pixel.G]++;
                        BValues[pixel.B]++;
                    }
                }

                int picturesize = picture.Width * picture.Height;

                LUTR = CalculateLUT2(RValues, picturesize);
                LUTG = CalculateLUT2(GValues, picturesize);
                LUTB = CalculateLUT2(BValues, picturesize);

                for (int i = 0; i < picture.Width; ++i)
                {
                    for (int j = 0; j < picture.Height; ++j)
                    {
                        System.Drawing.Color pixel = picture.GetPixel(i, j);

                        int r = LUTR[pixel.R];
                        int g = LUTG[pixel.G];
                        int b = LUTB[pixel.B];

                        System.Drawing.Color newpixel = Color.FromArgb(r, g, b);


                        picture.SetPixel(i, j, newpixel);
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

                int TB = ThresholdBlack(SortedList, threshold);
                int TW = ThresholdWhite(SortedList,threshold);

                while(true)
                {
                    if (((TW + TB) / 2) != threshold)
                    {
                        threshold = (TW + TB) / 2;
                    }
                    else                    
                    {
                        break;
                    }
                        
                    TB = ThresholdBlack(SortedList, threshold);
                    TW = ThresholdWhite(SortedList, threshold);
                }

                HandText.Text = threshold.ToString();

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

            int tmp = myValues.FindIndex(o => o.value == threshold);

            for(int i = 0; i < tmp; i++)
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

            int tmp = myValues.FindIndex(o => o.value == threshold);

            for (int i = tmp; i < myValues.Count; i++)
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
