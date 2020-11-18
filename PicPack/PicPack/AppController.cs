using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Linq;
using System.IO;
using Ookii.Dialogs.Wpf;

namespace PicPack
{
    public sealed class AppController
    {
        public void DoPack(
            string name,
            List<string> files,
            int maxWidth,
            int maxHeight,
            int padding,
            ImageFormat imageFormat)
        {
            try
            {
                var bitmapDic = new Dictionary<Bitmap, string>();
                files = files.Distinct().ToList();
                foreach (var f in files)
                {
                    var b = BitmapUtils.LoadBitmap(f);
                    string n = Path.GetFileNameWithoutExtension(f);
                    bitmapDic.Add(b, n);
                }
                Packer p = new Packer(maxWidth, maxHeight, padding);
                p.Pack(bitmapDic, false);
                VistaFolderBrowserDialog d = new VistaFolderBrowserDialog();
                d.ShowDialog();
                if (!string.IsNullOrEmpty(d.SelectedPath))
                {
                    string imageFile = Path.Combine(d.SelectedPath, name + "." + imageFormat.ToString());
                    p.Save(imageFile, imageFormat);
                    string jsonFile = Path.Combine(d.SelectedPath, name + ".json");
                    p.SaveJson(jsonFile, name, false);
                    Console.WriteLine("Saved " + d.SelectedPath);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(e.ToString());
            }


        }
    }
}
