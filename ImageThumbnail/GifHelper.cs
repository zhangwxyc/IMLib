using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageThumbnail
{
    internal class GifHelper
    {
        /// <summary>
        /// 获取图片中的各帧
        /// </summary>
        /// <param name="pPath">图片路径</param>
        /// <param name="pSavePath">保存路径</param>
        public void GetFrames(string pPath, string pSavedPath)
        {
            Image gif = Image.FromFile(pPath);
            FrameDimension fd = new FrameDimension(gif.FrameDimensionsList[0]);

            //获取帧数(gif图片可能包含多帧，其它格式图片一般仅一帧)
            int count = gif.GetFrameCount(fd);

            //以Jpeg格式保存各帧
            for (int i = 0; i < count; i++)
            {
                gif.SelectActiveFrame(fd, i);
                gif.Save(pSavedPath + "\\frame_" + i + ".jpg", ImageFormat.Jpeg);
            }
        }

        /// <summary> 

        /// 设置GIF大小 
        /// </summary> 
        /// <param name="path">图片路径</param> 
        /// <param name="width">宽</param> 
        /// <param name="height">高</param> 
        public static void MakeGifBySize(string path,string desPath, int width, int height)
        {
            Image gif = new Bitmap(width, height);
            Image frame = new Bitmap(width, height);
            Image res = Image.FromFile(path);
            Graphics g = Graphics.FromImage(gif);
            Rectangle rg = new Rectangle(0, 0, width, height);
            Graphics gFrame = Graphics.FromImage(frame);

            foreach (Guid gd in res.FrameDimensionsList)
            {
                FrameDimension fd = new FrameDimension(gd);

                //因为是缩小GIF文件所以这里要设置为Time，如果是TIFF这里要设置为PAGE，因为GIF以时间分割，TIFF为页分割 
                FrameDimension f = FrameDimension.Time;
                int count = res.GetFrameCount(fd);
                ImageCodecInfo codecInfo = GetEncoder(ImageFormat.Gif);
                System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.SaveFlag;
                EncoderParameters eps = null;

                for (int i = 0; i < count; i++)
                {
                    res.SelectActiveFrame(f, i);
                    if (0 == i)
                    {

                        g.DrawImage(res, rg);

                        eps = new EncoderParameters(1);

                        //第一帧需要设置为MultiFrame 

                        eps.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.MultiFrame);
                        BindProperty(res, gif);
                        gif.Save(desPath, codecInfo, eps);
                    }
                    else
                    {

                        gFrame.DrawImage(res, rg);

                        eps = new EncoderParameters(1);

                        //如果是GIF这里设置为FrameDimensionTime，如果为TIFF则设置为FrameDimensionPage 

                        eps.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.FrameDimensionTime);

                        BindProperty(res, frame);
                        gif.SaveAdd(frame, eps);
                    }
                }

                eps = new EncoderParameters(1);
                eps.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.Flush);
                gif.SaveAdd(eps);
            }
        }

        /// <summary> 
        /// 将源图片文件里每一帧的属性设置到新的图片对象里 
        /// </summary> 
        /// <param name="a">源图片帧</param> 
        /// <param name="b">新的图片帧</param> 
        private static void BindProperty(Image a, Image b)
        {

            //这个东西就是每一帧所拥有的属性，可以用GetPropertyItem方法取得这里用为完全复制原有属性所以直接赋值了 

            //顺便说一下这个属性里包含每帧间隔的秒数和透明背景调色板等设置，这里具体那个值对应那个属性大家自己在msdn搜索GetPropertyItem方法说明就有了 

            for (int i = 0; i < a.PropertyItems.Length; i++)
            {
                b.SetPropertyItem(a.PropertyItems[i]);
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
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
    }
}
