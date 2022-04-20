using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MyAgencyVault.VM.VMLib.Adorners;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System;

namespace MyAgencyVault.VM.VMLib.PayorForm
{
    public class DesignerItem : ContentControl
    {
        #region VM Delegate Instance

        private PayorToolVMDelegate m_PayorToolVMDelegate;


        #endregion

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
          DependencyProperty.Register("IsSelected", typeof(bool),
                                      typeof(DesignerItem),
                                      new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty MoveThumbTemplateProperty =
            DependencyProperty.RegisterAttached("MoveThumbTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetMoveThumbTemplate(UIElement element)
        {
            return (ControlTemplate)element.GetValue(MoveThumbTemplateProperty);
        }

        public static void SetMoveThumbTemplate(UIElement element, ControlTemplate value)
        {
            element.SetValue(MoveThumbTemplateProperty, value);
        }

        static DesignerItem()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerItem), new FrameworkPropertyMetadata(typeof(DesignerItem)));
        }

        public DesignerItem()
        {
            this.Loaded += new RoutedEventHandler(this.DesignerItem_Loaded);
            m_PayorToolVMDelegate = new PayorToolVMDelegate();

        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            try
            {
                base.OnPreviewMouseDown(e);
                DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

                if (designer != null)
                {
                    if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                    {
                        this.IsSelected = !this.IsSelected;
                    }
                    else
                    {
                        if (!this.IsSelected)
                        {
                            designer.DeselectAll();
                            this.IsSelected = true;
                            m_PayorToolVMDelegate.OnFieldSelectionChanged((this.Content as TextBoxDI).Field);

                            if (this.Content is TextBox)
                            {
                                (this.Content as TextBox).Focus();
                            }
                        }
                    }
                }

                e.Handled = false;
            }
            catch { }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            try
            {
                base.OnKeyDown(e);
                DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

                if (e.Key == Key.Delete)
                {
                    designer.DeleteSelected();
                }
            }
            catch { }
        }

        private void DesignerItem_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
            if (this.Template != null)
            {
                ContentPresenter contentPresenter =
                    this.Template.FindName("PART_ContentPresenter", this) as ContentPresenter;

                MoveThumb thumb =
                    this.Template.FindName("PART_MoveThumb", this) as MoveThumb;

                if (contentPresenter != null && thumb != null)
                {
                    UIElement contentVisual =
                        VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;

                    if (contentVisual != null)
                    {
                        ControlTemplate template =
                            DesignerItem.GetMoveThumbTemplate(contentVisual) as ControlTemplate;

                        if (template != null)
                        {
                            thumb.Template = template;
                        }
                    }
                }
            }
        }
            catch{}
        }
    }

    public class TextBoxDI : TextBox
    {
        public PayorToolAvailablelFieldType Field { get; set; }
        public Guid PayorId { get; set; }
    }
}
