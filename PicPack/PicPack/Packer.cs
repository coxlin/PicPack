#region PORT LICENSE
/*

 C# Port of Chevy Ray Johnston's Packer

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
#endregion

#region ORIGINAL CODE LICENSE
/* 
Original Packer.hpp/cpp

 MIT License

 Copyright (c) 2017 Chevy Ray Johnston
 
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
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using BitmapJSONLib;
using Newtonsoft.Json;

namespace PicPack
{
    public sealed class Packer
    {
        private struct Point
        {
            public int X;
            public int Y;
            public bool IsRot;
        };

        private readonly List<Point> _points = new List<Point>();
        private readonly List<Bitmap> _bitmaps = new List<Bitmap>();
        
        private Dictionary<Bitmap, string> _bitmapDic;
        
        private int _width, _height, _pad;
        
        public Packer(int w , int h, int pad)
        {
            _width = w;
            _height = h;
            _pad = pad;
        }

        public void Pack(
            Dictionary<Bitmap, string> bitmapDic, 
            bool allowRotations)
        {
            _bitmapDic = bitmapDic;
            var bitmaps = bitmapDic.Keys.ToList();

            MaxRectsBinPack packer = new MaxRectsBinPack(_width, _height, allowRotations);
            
            int ww = 0;
            int hh = 0;

            while (bitmaps.Count > 0)
            {
                var bitmap = bitmaps.Last();
                Console.WriteLine("Processing " + bitmaps.Count);

                var rect =
                    packer.Insert(
                        bitmap.Width,
                        bitmap.Height,
                        MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestShortSideFit);

                if (rect.width == 0 || rect.height == 0)
                {
                    break;
                }

                Point p = new Point();
                p.X = rect.x;
                p.Y = rect.y;
                p.IsRot = allowRotations && bitmap.Width != (rect.width - _pad);
                _points.Add(p);
                _bitmaps.Add(bitmap);
                bitmaps.Remove(bitmap);

                ww = Math.Max(rect.x + rect.width, ww);
                hh = Math.Max(rect.y + rect.height, hh);
            }

            while (_width / 2 >= ww)
                _width /= 2;
            while (_height / 2 >= hh)
                _height /= 2;
        }

        public void Save(string file, ImageFormat imageFormat)
        {
            Bitmap bitmap = new Bitmap(_width, _height);
            int j = _bitmaps.Count;
            for (int i = 0; i < j; ++i)
            {
                if (_points[i].IsRot)
                {
                    BitmapUtils.CopyPixelsRot(bitmap, _bitmaps[i], _points[i].X, _points[i].Y);
                }
                else
                {
                    BitmapUtils.CopyPixels(bitmap, _bitmaps[i], _points[i].X, _points[i].Y);
                }
            }
            bitmap.Save(file, imageFormat);
        }

        public void SaveJson(string jsonFile, string name, bool rotate)
        {
            var json = new BitmapJSON();
            var jsonElements = new List<BitmapJSONElement>();
            json.Name = name;
            for (int i = 0; i < _bitmaps.Count; ++i)
            {
                jsonElements.Add(new BitmapJSONElement()
                {
                    Name = _bitmapDic[_bitmaps[i]],
                    X = _points[i].X,
                    Y = _points[i].Y,
                    Width = _bitmaps[i].Width,
                    Height = _bitmaps[i].Height,
                    Rotate = _points[i].IsRot
                });
            }

            json.Images = jsonElements;
            string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
            File.WriteAllText(jsonFile, jsonString);
        }
    }
}
