using System.Windows;
using System.Windows.Controls;

namespace MyAgencyVault.VM.VMLib.Adorners
{
    public class ResizeChrome : Control
    {
        static ResizeChrome()
        {
            try
            {
                FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeChrome), new FrameworkPropertyMetadata(typeof(ResizeChrome)));
            }
            catch { }
        }
    }
}
