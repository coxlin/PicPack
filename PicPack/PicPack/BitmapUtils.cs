/*
 
 MIT License
 
 Copyright (c) 2020 Lindsay Cox
 
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
 
 */

using System.Drawing;
using System.IO;

namespace PicPack
{
    public static class BitmapUtils
    {
        public static Bitmap LoadBitmap(string file)
        {
            Image i = Image.FromFile(file);
            return new Bitmap(i);
        }

        public static byte[] ImageToByte(Bitmap img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public static void CopyRegionIntoImage(
            Bitmap src,
            ref Bitmap dest,
            int x,
            int y)
        {
            using (Graphics g = Graphics.FromImage(dest))
            {
                g.DrawImage(src, x, y);
            }
        }

        public static void CopyPixels(Bitmap target, Bitmap src, int tx, int ty)
        {
            CopyRegionIntoImage(src, ref target, tx, ty);
        }

        
    }
}
