using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace OpFlow.Service.DataAccess
{
    public class ImageHelper
    {
        public static List<byte[]> CreateWebImage(byte[] tiffBytes)
        {
            if (tiffBytes == null) return null;

            var result = new List<byte[]>();

            var tiffStream = new MemoryStream(tiffBytes);
            
            var tiffImage = Image.FromStream(tiffStream);
            
            for (var index = 0; index < tiffImage.GetFrameCount(FrameDimension.Page); index++)
            {
                tiffImage.SelectActiveFrame(FrameDimension.Page, index);

                var pngStream = new MemoryStream();
                tiffImage.Save(pngStream, ImageFormat.Png);
                result.Add(pngStream.GetBuffer());
            }

            return result;
        }
    }
}