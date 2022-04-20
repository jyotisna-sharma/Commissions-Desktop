using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using MyAgencyVault.VM.VMLib.Adorners;

namespace MyAgencyVault.VM.VMLib.PayorForm
{
    public class ResizeDecorator : Control
    {
        private Adorner adorner;

        public bool ShowDecorator
        {
            get { return (bool)GetValue(ShowDecoratorProperty); }
            set { SetValue(ShowDecoratorProperty, value); }
        }

        public static readonly DependencyProperty ShowDecoratorProperty =
            DependencyProperty.Register("ShowDecorator", typeof(bool), typeof(ResizeDecorator),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(ShowDecoratorProperty_Changed)));

        public ResizeDecorator()
        {
            Unloaded += new RoutedEventHandler(this.ResizeDecorator_Unloaded);
        }

        private void HideAdorner()
        {
            if (this.adorner != null)
            {
                this.adorner.Visibility = Visibility.Hidden;
            }
        }

        private void ShowAdorner()
        {
            try
            {
                if (this.adorner == null)
                {
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);

                    if (adornerLayer != null)
                    {
                        ContentControl designerItem = this.DataContext as ContentControl;
                        Canvas canvas = VisualTreeHelper.GetParent(designerItem) as Canvas;
                        this.adorner = new ResizeAdorner(designerItem);
                        adornerLayer.Add(this.adorner);

                        if (this.ShowDecorator)
                        {
                            this.adorner.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.adorner.Visibility = Visibility.Hidden;
                        }
                    }
                }
                else
                {
                    this.adorner.Visibility = Visibility.Visible;
                }
            }
            catch
            { }
        }

        private void ResizeDecorator_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.adorner != null)
                {
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                    if (adornerLayer != null)
                    {
                        adornerLayer.Remove(this.adorner);
                    }

                    this.adorner = null;
                }
            }
            catch{ }
        }

        private static void ShowDecoratorProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                ResizeDecorator decorator = (ResizeDecorator)d;
                bool showDecorator = (bool)e.NewValue;

                if (showDecorator)
                {
                    decorator.ShowAdorner();
                }
                else
                {
                    decorator.HideAdorner();
                }
            }
            catch { }
        }
    }
}
