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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Windows;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace PicPack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Regex _paddingRegex = new Regex("[^0-9]+");
        private static readonly AppController _appController = new AppController();

        public MainWindow()
        {
            InitializeComponent();
            SetupSizeBox();
            SetupFileTypeBox();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("PICPACK Created by Lindsay Cox");
            Console.WriteLine("Copyright (c) 2020");
            Console.WriteLine();
            Console.WriteLine("Inspired by Crunch - https://github.com/ChevyRay/crunch");
            Console.WriteLine();
            Console.WriteLine("Uses MaxRectBinPack");
            Console.WriteLine("Original C# Port by Sven Magnus - https://wiki.unity3d.com/index.php/MaxRectsBinPack");
            Console.WriteLine("Original C++ code by Jukka Jylänki - https://github.com/juj/RectangleBinPack");
            Console.WriteLine();
        }

        private void SetupSizeBox()
        {
            var list = new List<string>()
            {
                "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192"
            };

            foreach (var i in list)
            {
                MaxWidthBox.Items.Add(i);
                MaxHeightBox.Items.Add(i);
            }
        }

        private void SetupFileTypeBox()
        {
            TypeBox.Items.Add(ImageFormat.Png.ToString());
            TypeBox.Items.Add(ImageFormat.Jpeg.ToString());
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FileList.Items.Add(openFileDialog.FileName);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var items = FileList.SelectedItems;
            foreach (var i in items)
            {
                FileList.Items.Remove(i);
            }
        }

        private void PackButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileList.Items.Count == 0 ||
                string.IsNullOrEmpty(NameBox.Text)) 
            {
                return;
            }

            ImageFormat imageFormat = ImageFormat.Png;
            if (TypeBox.Text == ImageFormat.Jpeg.ToString())
            {
                imageFormat = ImageFormat.Jpeg;
            }

            _appController.DoPack(
                NameBox.Text,
                FileList.Items.Cast<string>().ToList(),
                Convert.ToInt32(MaxWidthBox.Text),
                Convert.ToInt32(MaxHeightBox.Text),
                Convert.ToInt32(PaddingBox.Text),
                imageFormat);
        }

        private void PaddingBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = _paddingRegex.IsMatch(e.Text);
        }

        private void PaddingBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!_paddingRegex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            FileList.Items.Clear();
        }
    }
}
