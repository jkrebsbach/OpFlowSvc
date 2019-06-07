using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace OpFlow.Service.DataAccess
{
    public class ImageHelper
    {
        public static byte[] CreateWebImage(byte[] tiffImage)
        {
            var tiffStream = new MemoryStream(tiffImage);
            var pngStream = new MemoryStream();

            Image.FromStream(tiffStream)
                .Save(pngStream, ImageFormat.Png);

            return pngStream.GetBuffer();
        }
    }
}