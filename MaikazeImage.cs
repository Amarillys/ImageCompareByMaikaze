//BitmapSource
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace ImageCompare_Maikaze_
{
    class MaikazeImage
    {
        /// <summary>
        /// Interface:
        /// 
        /// ---> ChangeAlpha(int dst_alpha): to change the alpha of the image while its bitDepth is 32.
        /// 
        /// </summary>
        public int w,h;
        public int bitDepth;

        public BitmapSource bmpSrc;
        public byte[] px;
        public string fileName;
        private int m_alpha;
        private bool loadOkay;
        public MaikazeImage()
        {
            bitDepth = 24;
            m_alpha = 255;
            loadOkay = false;
        }
        public MaikazeImage(int dst_w, int dst_h,int bit)
        {
            w = dst_w;
            h = dst_h;
            bitDepth = bit;
            px = new byte[w * h * bitDepth / 8 + 4];
            if(bit == 32)
            {
                bmpSrc = BitmapSource.Create(w, h, 0, 0, PixelFormats.Pbgra32, null, px, bitDepth * w / 8);
                m_alpha = 255;
            }
            else
                bmpSrc = BitmapSource.Create(w, h, 0, 0, PixelFormats.Bgr24, null, px, bitDepth * w / 8);
        }
        public void ChangeAlpha(int dst_alpha)
        {
            SetAlpha(dst_alpha);
            for (int i = 0; i < px.Length / 4 - 1; i++)
                if (px[i * 4 + 3] > 0 )
                    px[i * 4 + 3] = (byte)m_alpha;
            RefreshImageFromPixels();
        }
        public bool EqualsTo(MaikazeImage img)
        {
            if (this.w == img.w && this.h == img.h)
                return true;
            return false;
        }
        public int GetAlpha()
        {
            return m_alpha;
        }
        public void GetBitDepth()
        {
            //get the bit depth of the image by reading its header
            FileStream imgFile = new FileStream(fileName, FileMode.Open);
            imgFile.Seek(0, SeekOrigin.Begin);
            if (imgFile.ReadByte() == 0x89)
                if (imgFile.ReadByte() == 0x50)
                    if (imgFile.ReadByte() == 0x4e)
                        loadOkay = true;
            if (loadOkay)
            {
                imgFile.Seek(25, SeekOrigin.Begin);
                int src_bit = imgFile.ReadByte();
                if (src_bit == 0x06)
                    bitDepth = 32;
                else
                    bitDepth = 24;
            }
            imgFile.Close();
        }
        public bool SuccessLoad()
        {
            return loadOkay;
        }
        public void LoadImage(string dst_fileName)
        {
            fileName = dst_fileName;
            GetBitDepth();
            Uri fileUri = new Uri(dst_fileName);
            BitmapDecoder decoder = BitmapDecoder.Create(fileUri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            bmpSrc = decoder.Frames[0];
            w = (int)bmpSrc.PixelWidth;
            h = (int)bmpSrc.PixelHeight;
            px = new byte[w * h * bitDepth / 8];
            bmpSrc.CopyPixels(px, w * bitDepth / 8, 0);
            loadOkay = true;
        }
        public void RefreshImageFromPixels()
        {
            bmpSrc = BitmapSource.Create(w, h, 0, 0, PixelFormats.Pbgra32, null, px, w * bitDepth / 8);
        }
        public void Rgb_To_Rgba()
        {
            byte[] tmpPx = new byte[w * h * bitDepth / 8];
            Buffer.BlockCopy(px, 0, tmpPx, 0, px.Length);
            bitDepth = 32;
            px = new byte[w * h * bitDepth / 8];
            for(int i = 0;i < px.Length / 4 - 1;i++)
            {
                px[i * 4 + 0] = tmpPx[i * 3 + 0];
                px[i * 4 + 1] = tmpPx[i * 3 + 1];
                px[i * 4 + 2] = tmpPx[i * 3 + 2];
                px[i * 4 + 3] = 255;
            }
            RefreshImageFromPixels();
        }
        public void Rgba_To_Rgb()
        {
            byte[] tmpPx = new byte[w * h * bitDepth / 8];
            Buffer.BlockCopy(px, 0, tmpPx, 0, px.Length);
            bitDepth = 24;
            px = new byte[w * h * bitDepth / 8];
            for(int i = 0;i < px.Length / 4 - 1;i++)
            {
                px[i * 3 + 0] = px[i * 4 + 0];
                px[i * 3 + 1] = px[i * 4 + 1];
                px[i * 3 + 2] = px[i * 4 + 2];
            }
            RefreshImageFromPixels();
        }
        public void SaveImage(string dst_fileName)
        {
            FileStream saveimg = new FileStream(dst_fileName, FileMode.Create);
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Interlace = PngInterlaceOption.On;
            pngEncoder.Frames.Add(BitmapFrame.Create(bmpSrc));
            pngEncoder.Save(saveimg);
            saveimg.Close();
        }
        public void SetAlpha(int dst_alpha)
        {
            if (dst_alpha >= 0 && dst_alpha < 256)
                m_alpha = dst_alpha;
        }
    }
}
