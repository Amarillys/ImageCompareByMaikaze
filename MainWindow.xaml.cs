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
        private MaikazeImage cutImg;
        private int cur_image;
        private byte[] try_ori_px;
        private int[] dp_w, dp_h;
        private int try_unit, try_x, try_y;
        private string[] showText;
        private int cleanPnt;
        private bool[] WasLoaded,WasSaved;
        public MainWindow()
        {
            InitValue();
            InitializeComponent();
            InitUI();
        }
        private void InitValue()
        {
            dpBmp = new List<MaikazeImage>();
            showText = new string[4];
            dp_h = new int[4];
            dp_w = new int[4];
            try_x = try_y = 0;
            try_unit = 1;
            cur_image = ImageOri;
            WasLoaded = WasSaved = new bool[4];
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
            TryUnitLabel.Content = "1";
            LoadImg(System.Environment.CurrentDirectory + "\\sample\\1.png");
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
                AlphaText.Text = "255";
                AlphaSlider.Value = 255;
                if (WasLoaded[cur_image])
                    dpBmp[cur_image].SetAlpha(255);
            }
        }
        private void BitDepthList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WasLoaded[cur_image])
                if( BitDepthList.SelectedIndex == 1 && dpBmp[cur_image].bitDepth == 32)
                {
                    dpBmp[cur_image].Rgba_To_Rgb();
                    AlphaText.IsEnabled = AlphaSlider.IsEnabled = false;
                    dpBmp[cur_image].bitDepth = 24;
                }
                else if (BitDepthList.SelectedIndex == 0 && dpBmp[cur_image].bitDepth == 24)
                {
                    dpBmp[cur_image].Rgb_To_Rgba();
                    AlphaText.IsEnabled = AlphaSlider.IsEnabled = true;
                    dpBmp[cur_image].bitDepth = 32;
                }
            SwitchAlphaModule();
        }
        private void SwitchAlphaModule()
        {
            AlphaText.IsEnabled = AlphaSlider.IsEnabled
                = AlphaSlider.IsEnabled = AlphaLabel.IsEnabled
                = (WasLoaded[cur_image] && dpBmp[cur_image].bitDepth == 32);
        }
        private void SwitchCleanModule()
        {
            CleanSlider.IsEnabled = CleanLabel.IsEnabled
                = (WasLoaded[ImageDiff] && (cur_image > ImageRefer));
        }
        private void SwitchTryModule()
        {
            TryUpBtn.IsEnabled = TryDownBtn.IsEnabled
                = TryLeftBtn.IsEnabled = TryRightBtn.IsEnabled
                = TryLessBtn.IsEnabled = TryMoreBtn.IsEnabled
                = TryUnitLabel.IsEnabled = TryUnitReset.IsEnabled
                = (WasLoaded[ImageDiff] && (cur_image == ImageTry) && WasLoaded[ImageTry]);
        }
        private void CleanSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            cleanPnt = (int)CleanSlider.Value;
            CleanDiff();
        }
        private void CleanDiff()
        {
            GetDiffImage();
        }
        private void GetDiffImage()
        {
            bool isSame;
            int ori_bpp = dpBmp[ImageOri].bitDepth / 8;  // bytes per pixel
            int ref_bpp = dpBmp[ImageRefer].bitDepth / 8;
            MaikazeImage diffBmp = new MaikazeImage(dpBmp[ImageOri].w, dpBmp[ImageOri].h, 32);
            dp_w[ImageDiff] = dpBmp[ImageOri].w;
            dp_h[ImageDiff] = dpBmp[ImageOri].h;
            if (WasLoaded[ImageDiff])
                dpBmp.RemoveAt(ImageDiff);
            dpBmp.Insert(ImageDiff, diffBmp);
            for (int i = 0; i < dpBmp[ImageOri].w * dpBmp[ImageOri].h; i++)
            {
                isSame = true;
                if (!(     dpBmp[ImageOri].px[i * ori_bpp + 0] == dpBmp[ImageRefer].px[i * ref_bpp + 0]
                        && dpBmp[ImageOri].px[i * ori_bpp + 1] == dpBmp[ImageRefer].px[i * ref_bpp + 1]
                        && dpBmp[ImageOri].px[i * ori_bpp + 2] == dpBmp[ImageRefer].px[i * ref_bpp + 2]))
                    isSame = false;

                if (isSame)
                    dpBmp[ImageDiff].px[i * 4 + 3] = 0;
                else
                {
                    dpBmp[ImageDiff].px[i * 4 + 0] = (byte)(dpBmp[ImageOri].px[i * ori_bpp + 0] - dpBmp[ImageRefer].px[i * ref_bpp + 0] * (255 - cleanPnt) / 255);
                    dpBmp[ImageDiff].px[i * 4 + 1] = (byte)(dpBmp[ImageOri].px[i * ori_bpp + 1] - dpBmp[ImageRefer].px[i * ref_bpp + 1] * (255 - cleanPnt) / 255);
                    dpBmp[ImageDiff].px[i * 4 + 2] = (byte)(dpBmp[ImageOri].px[i * ori_bpp + 2] - dpBmp[ImageRefer].px[i * ref_bpp + 2] * (255 - cleanPnt) / 255);
                    dpBmp[ImageDiff].px[i * 4 + 3] = 255;
                }
            }
            dpBmp[ImageDiff].RefreshImageFromPixels();
            IsDiffImage.IsChecked = true;
            WasLoaded[ImageDiff] = true;
        }
        private void IsDiffImage_Checked(object sender, RoutedEventArgs e)
        {
            cur_image = ImageDiff;
            SyncUI();
        }
        private void IsOriImage_Checked(object sender, RoutedEventArgs e)
        {
            cur_image = ImageOri;
            SyncUI();
        }
        private void IsReferImage_Checked(object sender, RoutedEventArgs e)
        {
            cur_image = ImageRefer;
            SyncUI();
        }
        private void IsTryImage_Checked(object sender, RoutedEventArgs e)
        {
            cur_image = ImageTry;
            SyncUI();
        }
        public void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Filter = "Choose your image (*.png);|*.png";
            openfile.Title = "Choose your image";
            openfile.ShowDialog();
            if (openfile.FileName != "")
                LoadImg(openfile.FileName);
            if (WasLoaded[ImageOri] && WasLoaded[ImageRefer] && !WasLoaded[ImageDiff])
                GetDiffImage();
            if (cur_image == ImageTry && dpBmp[cur_image].SuccessLoad())
            {
                SwitchTryModule();
                try_ori_px = new byte[dpBmp[ImageTry].px.Length];
                Buffer.BlockCopy(dpBmp[ImageTry].px, 0, try_ori_px, 0, dpBmp[ImageTry].px.Length);
                MergeImage();
                WasLoaded[ImageTry] = true;
            }
            SyncUI();
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
            if(WasLoaded[cur_image])
                dpBmp.RemoveAt(cur_image);
            dpBmp.Insert(cur_image, newBmp);
            ZoomSlider.Value = 0;
            WasLoaded[cur_image] = true;
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
        public void SyncUI()
        {
            SwitchAlphaModule();
            SwitchCleanModule();
            SwitchTryModule();
            PathText.Text  = "~~~Mai de go za ru~~~";
            DpImage.Height = dp_h[cur_image];
            DpImage.Width  = dp_w[cur_image];
            if (WasLoaded[cur_image])
            {
                SaveBtn.IsEnabled = true;
                BitDepthList.IsEnabled = true;
                PathText.Text  = showText[cur_image];
                DpImage.Source = dpBmp[cur_image].bmpSrc;
                if (dpBmp[cur_image].bitDepth == 32)
                    BitDepthList.SelectedIndex = 0;
                else
                    BitDepthList.SelectedIndex = 1;
            }
            else
            {
                SaveBtn.IsEnabled = false;
                DpImage.Source = null;
                BitDepthList.IsEnabled = false;
            }
            IsDiffImage.IsEnabled = IsTryImage.IsEnabled
                = (WasLoaded[ImageDiff]);
            LoadBtn.IsEnabled = (cur_image != ImageDiff);
        }
        private void MergeImage()
        {
            Buffer.BlockCopy(try_ori_px, 0, dpBmp[ImageTry].px, 0, try_ori_px.Length);
            int cutImg_w = dpBmp[ImageDiff].w - Math.Abs(try_x);
            int cutImg_h = dpBmp[ImageDiff].h - Math.Abs(try_y);
            cutImg = new MaikazeImage(cutImg_w, cutImg_h, 32);
            CreateCutImage();
            dpBmp[ImageTry].MergeImage(cutImg, try_x, try_y);
            DpImage.Source = dpBmp[cur_image].bmpSrc;
        }
        public void CreateCutImage()
        {
            int tmp_x = try_x;
            int tmp_y = try_y;
            if(try_x > 0)                tmp_x = 0;
            if(try_y > 0)                tmp_y = 0;
            for (int i = 0; i < cutImg.h; i++)
                for (int j = 0; j < cutImg.w; j++)
                {
                    cutImg.px[(i * cutImg.w + j) * 4 + 0] = dpBmp[ImageDiff].px[((i - tmp_y) * dpBmp[ImageDiff].w + j - tmp_x) * 4 + 0];
                    cutImg.px[(i * cutImg.w + j) * 4 + 1] = dpBmp[ImageDiff].px[((i - tmp_y) * dpBmp[ImageDiff].w + j - tmp_x) * 4 + 1];
                    cutImg.px[(i * cutImg.w + j) * 4 + 2] = dpBmp[ImageDiff].px[((i - tmp_y) * dpBmp[ImageDiff].w + j - tmp_x) * 4 + 2];
                    cutImg.px[(i * cutImg.w + j) * 4 + 3] = dpBmp[ImageDiff].px[((i - tmp_y) * dpBmp[ImageDiff].w + j - tmp_x) * 4 + 3];
                }
            cutImg.RefreshImageFromPixels();
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
        public void ZoomDpImage(float proportion)
        {
            if (WasLoaded[cur_image])
            {
                dp_w[cur_image] = (int)(dpBmp[cur_image].w * proportion);
                dp_h[cur_image] = (int)(dpBmp[cur_image].h * proportion);
                DpImage.Width = dp_w[cur_image];
                DpImage.Height = dp_h[cur_image];
            }
        }
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double proportion;
            if (ZoomSlider.Value > 100)
                proportion = Math.Sqrt(ZoomSlider.Value / 100);
            else if (ZoomSlider.Value < -100)
                proportion = Math.Sqrt(1 / (-ZoomSlider.Value / 100));
            else
                proportion = 1;
            ZoomDpImage((float)proportion);
        }
        private void TryLessBtn_Click(object sender, RoutedEventArgs e)
        {
            SetTryUnit(try_unit - 2);
        }
        private void TryMoreBtn_Click(object sender, RoutedEventArgs e)
        {
            SetTryUnit(try_unit + 2);
        }
        private void TryUnitReset_Click(object sender, RoutedEventArgs e)
        {
            SetTryUnit(1);
        }
        private void SetTryUnit(int unit)
        {
            if (unit > 127)                unit = 127;
            if (unit < 2)                  unit = 1;
                try_unit = unit;
            TryUnitLabel.Content = try_unit.ToString();
        }
        private void TryLeftBtn_Click(object sender, RoutedEventArgs e)
        {
            try_x -= try_unit;
            MergeImage();
        }
        private void TryRightBtn_Click(object sender, RoutedEventArgs e)
        {
            try_x += try_unit;
            MergeImage();
        }
        private void TryUpBtn_Click(object sender, RoutedEventArgs e)
        {
            try_y -= try_unit;
            MergeImage();
        }
        private void TryDownBtn_Click(object sender, RoutedEventArgs e)
        {
            try_y += try_unit;
            MergeImage();
        }
    }
}

/*
        private System.Windows.Visibility convVisable(bool willshow)
        {
            if (willshow)
                return System.Windows.Visibility.Visible;
            else
                return System.Windows.Visibility.Hidden;
        }
*/
