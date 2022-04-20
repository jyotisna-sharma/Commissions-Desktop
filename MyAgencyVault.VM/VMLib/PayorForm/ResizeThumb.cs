using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MyAgencyVault.VM.VMLib.Adorners;

namespace MyAgencyVault.VM.VMLib.PayorForm
{
    public class ResizeThumb : Thumb
    {
        private DesignerItem designerItem;
        private DesignerCanvas designerCanvas;

        public ResizeThumb()
        {
            DragStarted += new DragStartedEventHandler(this.ResizeThumb_DragStarted);
            DragDelta += new DragDeltaEventHandler(this.ResizeThumb_DragDelta);
        }

        private void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.designerItem = DataContext as DesignerItem;

            if (this.designerItem != null)
            {
                try
                {
                    this.designerCanvas = VisualTreeHelper.GetParent(this.designerItem) as DesignerCanvas;
                }
                catch { }
            }
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            try
            {
                if (this.designerItem != null && this.designerCanvas != null && this.designerItem.IsSelected)
                {
                    double minLeft = double.MaxValue;
                    double minTop = double.MaxValue;
                    double minDeltaHorizontal = double.MaxValue;
                    double minDeltaVertical = double.MaxValue;
                    double dragDeltaVertical, dragDeltaHorizontal;

                    foreach (DesignerItem item in this.designerCanvas.SelectedItems)
                    {
                        minLeft = Math.Min(Canvas.GetLeft(item), minLeft);
                        minTop = Math.Min(Canvas.GetTop(item), minTop);

                        minDeltaVertical = Math.Min(minDeltaVertical, item.ActualHeight - item.MinHeight);
                        minDeltaHorizontal = Math.Min(minDeltaHorizontal, item.ActualWidth - item.MinWidth);
                    }

                    foreach (DesignerItem item in this.designerCanvas.SelectedItems)
                    {
                        TextBox itemContent = item.Content as TextBox;
                        //switch (VerticalAlignment)
                        //{
                        //    case VerticalAlignment.Bottom:
                        //        dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                        //        item.Height = item.ActualHeight - dragDeltaVertical;
                        //        break;
                        //    case VerticalAlignment.Top:
                        //        dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                        //        Canvas.SetTop(item, Canvas.GetTop(item) + dragDeltaVertical);
                        //        item.Height = item.ActualHeight - dragDeltaVertical;
                        //        break;
                        //}
                        //itemContent.Height = item.Height;

                        switch (HorizontalAlignment)
                        {
                            case HorizontalAlignment.Left:
                                dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                                Canvas.SetLeft(item, Canvas.GetLeft(item) + dragDeltaHorizontal);
                                item.Width = item.ActualWidth - dragDeltaHorizontal;
                                break;
                            case HorizontalAlignment.Right:
                                dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                                item.Width = item.ActualWidth - dragDeltaHorizontal;
                                break;
                        }
                        itemContent.Width = item.Width;
                    }

                    e.Handled = true;
                }
            }
            catch { }
        }
    }
}
