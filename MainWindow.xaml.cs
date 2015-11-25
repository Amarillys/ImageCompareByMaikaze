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
using System.Windows.Forms;

namespace ImageCompare_Maikaze_
{
    /// <summary>
    /// ---> cur_image: the index of the image that is displayed currently in the DpImage.
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int ImageOri = 0;
        public const int ImageRefer = 1;
        public const int ImageDiff = 2;
        public const int ImageTry = 3;

        private List<MaikazeImage> dpBmp;
        private int cur_image;
        private int[] dp_w, dp_h;
        private string[] showText;
        private int bitDepth, cleanPnt;
        private bool noCompare;
        private bool[] WasLoaded,WasSaved;
        public MainWindow()
        {
            InitValue();
            InitializeComponent();
            InitUI();
        }

        public void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Filter = "Choose your image (*.png);|*.png";
            openfile.Title = "Choose your image";
            openfile.ShowDialog();
            if (openfile.FileName != "")
                LoadImg(openfile.FileName);
            if (WasLoaded[ImageOri] && WasLoaded[ImageRefer])
            {
                WasLoaded[ImageDiff] = true;
                GetDiffImage();
            }
        }
        private void InitValue()
        {
            dpBmp = new List<MaikazeImage>();
            showText = new string[4];
            dp_h = new int[4];
            dp_w = new int[4];
            cur_image = ImageOri;
            WasLoaded = WasSaved = new bool[4];
            noCompare = true;
            bitDepth = 24;
            cleanPnt = 0;
            for(int i = 0;i < 4;i++)
                WasLoaded[i] = WasSaved[i] = false;
        }
        private void InitUI()
        {
            BitDepthList.Items.Add("32Bit");
            BitDepthList.Items.Add("24Bit");
            IsOriImage.IsChecked = true;
            BitDepthList.SelectedIndex = 1;
            ZoomSlider.Value = 0;
            AlphaSlider.Value = 255;
            LoadImg(System.Environment.CurrentDirectory + "\\bg.png");
        }
        public void LoadImg(string dst_filename)
        {
            showText[cur_image] = "Image Was Loaded From：" + dst_filename;
            MaikazeImage newBmp = new MaikazeImage();
            newBmp.LoadImage(dst_filename);
            if (!newBmp.SuccessLoad())
            {
                PathText.Text = "failed to load file :" + dst_filename;
                return;
            }

            dp_w[cur_image] = newBmp.w;
            dp_h[cur_image] = newBmp.h;

            if (WasLoaded[ImageOri] && WasLoaded[ImageRefer]
                    && dpBmp[ImageOri].EqualsTo(dpBmp[ImageRefer]))
            {
                GetDiffImage();
                WasLoaded[ImageDiff] = true;
            }
            else
                noCompare = true;
            dpBmp.Insert(cur_image, newBmp);
            ZoomSlider.Value = 0;
            WasLoaded[cur_image] = true;
            SyncUI();
        }
        private void IsOriImage_Checked(object sender, RoutedEventArgs e)
        {
            cur_image = ImageOri;
            if (WasLoaded[ImageOri])
            {
                if(dpBmp[ImageOri].bitDepth == 32)
                    AlphaText.IsEnabled = AlphaSlider.IsEnabled = AlphaSlider.IsEnabled = true;
                else
                    AlphaText.IsEnabled = AlphaSlider.IsEnabled = AlphaSlider.IsEnabled = false;
                SaveBtn.IsEnabled = true;
            }
            else
            {
                SaveBtn.IsEnabled = false;
                AlphaText.IsEnabled = AlphaSlider.IsEnabled = AlphaSlider.IsEnabled = false;
            }            
            SyncUI();
        }
        private void IsReferImage_Checked(object sender, RoutedEventArgs e)
        {
            cur_image = ImageRefer;
            if (WasLoaded[ImageRefer])
            {
                if (dpBmp[ImageRefer].bitDepth == 32)
                    AlphaText.IsEnabled = AlphaSlider.IsEnabled = AlphaSlider.IsEnabled = true;
                else
                    AlphaText.IsEnabled = AlphaSlider.IsEnabled = AlphaSlider.IsEnabled = false;
                SaveBtn.IsEnabled = true;
            }
            else
            {
                SaveBtn.IsEnabled = false;
                AlphaText.IsEnabled = AlphaSlider.IsEnabled = AlphaSlider.IsEnabled = false;
            }
            SyncUI();
        }

        private void IsDiffImage_Checked(object sender, RoutedEventArgs e)
        {
            cur_image = ImageDiff;
            if (WasLoaded[ImageDiff])
            {
                if (dpBmp[ImageDiff].bitDepth == 32)
                    AlphaText.IsEnabled = AlphaSlider.IsEnabled = AlphaSlider.IsEnabled = true;
                else
                    AlphaText.IsEnabled = AlphaSlider.IsEnabled = AlphaSlider.IsEnabled = false;
                SaveBtn.IsEnabled = true;
            }
            else
            {
                SaveBtn.IsEnabled = false;
                AlphaText.IsEnabled = AlphaSlider.IsEnabled = AlphaSlider.IsEnabled = false;
            }
            SyncUI();
        }
        private void IsTryImage_Checked(object sender, RoutedEventArgs e)
        {
            cur_image = ImageTry;
            if (WasLoaded[ImageTry])
            {
                if (dpBmp[ImageTry].bitDepth == 32)
                    AlphaText.IsEnabled = AlphaSlider.IsEnabled = AlphaSlider.IsEnabled = true;
                else
                    AlphaText.IsEnabled = AlphaSlider.IsEnabled = AlphaSlider.IsEnabled = false;
                SaveBtn.IsEnabled = true;
            }
            else
            {
                SaveBtn.IsEnabled = false;
                AlphaText.IsEnabled = AlphaSlider.IsEnabled = AlphaSlider.IsEnabled = false;
            }
            SyncUI();
        }
        public void ZoomDpImage(float proportion)
        {
            if (WasLoaded[cur_image])
            {
                dp_w[cur_image] = (int)(dpBmp[cur_image].w * proportion);
                dp_h[cur_image] = (int)(dpBmp[cur_image].h * proportion);
                DpImage.Width   = dp_w[cur_image];
                DpImage.Height  = dp_h[cur_image];
            }
        }
        public void SyncUI()
        {
            PathText.Text  = "~~~Mai de go za ru~~~";
            DpImage.Height = dp_h[cur_image];
            DpImage.Width  = dp_w[cur_image];
            if (WasLoaded[cur_image])
            {
                PathText.Text  = showText[cur_image];
                DpImage.Source = dpBmp[cur_image].bmpSrc;
            }
            else
                DpImage.Source = null;
        }

        private void GetDiffImage()
        {
            bool isSame;
            MaikazeImage diffBmp = new MaikazeImage(dpBmp[ImageOri].w, dpBmp[ImageOri].h, 32);
            dp_w[ImageDiff] = dpBmp[ImageOri].w;
            dp_h[ImageDiff] = dpBmp[ImageOri].h;
            dpBmp.Insert(ImageDiff, diffBmp);            
            for (int i = 0; i < dpBmp[ImageOri].px.Length / 3 - 1; i++)
            {
                isSame = true;
                if (!(dpBmp[ImageOri].px[i * dpBmp[ImageOri].bitDepth / 8] == dpBmp[ImageRefer].px[i * 3]
                        && dpBmp[ImageOri].px[i * dpBmp[ImageOri].bitDepth / 8 + 1] == dpBmp[ImageRefer].px[i * bitDepth / 8 + 1]
                        && dpBmp[ImageOri].px[i * dpBmp[ImageOri].bitDepth / 8 + 2] == dpBmp[ImageRefer].px[i * bitDepth / 8 + 2]))
                    isSame = false;

                if (isSame)
                    dpBmp[ImageDiff].px[i * 4 + 3] = 0;
                else
                {
                    dpBmp[ImageDiff].px[i * 4 + 0] = (byte)(dpBmp[ImageOri].px[i * 3 + 0] - dpBmp[ImageRefer].px[i * 3 + 0] * (255 - cleanPnt) / 255);
                    dpBmp[ImageDiff].px[i * 4 + 1] = (byte)(dpBmp[ImageOri].px[i * 3 + 1] - dpBmp[ImageRefer].px[i * 3 + 1] * (255 - cleanPnt) / 255);
                    dpBmp[ImageDiff].px[i * 4 + 2] = (byte)(dpBmp[ImageOri].px[i * 3 + 2] - dpBmp[ImageRefer].px[i * 3 + 2] * (255 - cleanPnt) / 255);
                    dpBmp[ImageDiff].px[i * 4 + 3] = 255;
                }
            }
            dpBmp[ImageDiff].RefreshImageFromPixels();
            IsDiffImage.IsChecked = true;
        }

        private void BitDepthList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BitDepthList.SelectedIndex == 1)
            {
                AlphaText.IsEnabled = AlphaSlider.IsEnabled = false;
                bitDepth = 24;
            }
            else
            {
                AlphaText.IsEnabled = AlphaSlider.IsEnabled = true;
                bitDepth = 32;
            }
        }

        private void AlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WasLoaded[cur_image])
            {
                AlphaText.Text = ((int)(AlphaSlider.Value)).ToString();
                dpBmp[cur_image].ChangeAlpha((int)AlphaSlider.Value);
                DpImage.Source = dpBmp[cur_image].bmpSrc;
            }
        }

        private void AlphaText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsNumber(AlphaText.Text) && Int16.Parse(AlphaText.Text) < 255 && WasLoaded[cur_image])
            {
                AlphaSlider.Value = int.Parse(AlphaText.Text);
                dpBmp[cur_image].SetAlpha(int.Parse(AlphaText.Text));
            }
            else
            {
                AlphaText.Text    = "255";
                AlphaSlider.Value =  255 ;
                if (WasLoaded[cur_image])
                    dpBmp[cur_image].SetAlpha(255);
            }
        }
        public static bool IsNumber(string txt)
        {
            if (string.IsNullOrEmpty(txt))
                return false;
            foreach (char c in txt)
                if (!char.IsDigit(c))
                    return false;
            return true;
        }

        private void CleanSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            cleanPnt = (int)CleanSlider.Value;
            CleanDiff();
        }
        private void CleanDiff()
        {

        }
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double proportion;
            if (ZoomSlider.Value >= 100)
                proportion = Math.Sqrt(ZoomSlider.Value / 100);
            else
                proportion = Math.Sqrt(1 / (-((ZoomSlider.Value - 100) / 100)));
            ZoomDpImage((float)proportion);
        }
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.Filter = "Choose your image (*.png);|*.png";
            savefile.Title = "Choose the location that the image save ";
            savefile.ShowDialog();
            dpBmp[cur_image].SaveImage(savefile.FileName);
            PathText.Text = "Save successfully at " + savefile.FileName;
        }
    }
}
