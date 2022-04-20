using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
namespace MyAgencyVault.ViewModel.CommonItems
{
    public class ImageUploader
    {
        public static UIElement GetFileName(DependencyObject obj)
        {
            return (UIElement)obj.GetValue(FileNameProperty);
        }
        static DependencyObject _Objtext;
        public static void SetFileName(DependencyObject obj, int value)
        {
            obj.SetValue(FileNameProperty, value);
        }

        // Using a DependencyProperty as the backing store for MaxLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.RegisterAttached("FileName", typeof(UIElement), typeof(ImageUploader), new PropertyMetadata(OnFileNameChanged));

        private static void OnFileNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Hyperlink Btn = obj as Hyperlink;
           // DependencyObject _Obj1 = Btn.Parent;
            //Grid gr = _Obj1 as Grid;
            //_Objtext = gr.FindChild(typeof(Image), "imgPayor");
            _Objtext = args.NewValue as Image;
            Btn.Click += new RoutedEventHandler(Btn_Click);

        }
        private static void Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog();
                //Open the Pop-Up Window to select the file  
                if (dlg.ShowDialog() == true)
                {
                    new FileInfo(dlg.FileName);
                    using (Stream s = dlg.OpenFile())
                    {
                        TextReader reader = new StreamReader(s);
                        string st = reader.ReadToEnd();
                        Image img = _Objtext as Image;
                        //if (img.Source != null)
                        //{
                            img.SourceUpdated += new EventHandler<System.Windows.Data.DataTransferEventArgs>(img_SourceUpdated);
                            img.Source = new BitmapImage(new Uri(dlg.FileName));
                            PayorToolVM _obj = img.DataContext as PayorToolVM;
                            //code updated by sunil on 5 dec to upload only image file

                            //  BitmapImage _img = img.Source as BitmapImage;
                            _obj.ImagePath = dlg.FileName;
                            _obj.OnPropertyChanged("ImagePath");

                      //  }
                    }
                }
            }
            catch 
            {
            }

        }

        static void img_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            
        }

    }
}
