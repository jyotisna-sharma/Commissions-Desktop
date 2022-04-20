using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MyAgencyVault.VM.VMLib.Adorners;

namespace MyAgencyVault.VM.VMLib.PayorForm
{
    public class MoveThumb : Thumb
    {
        private DesignerItem designerItem;
        private DesignerCanvas designerCanvas;

        public MoveThumb()
        {
            DragStarted += new DragStartedEventHandler(this.MoveThumb_DragStarted);
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
        }

        private void MoveThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            try
            {
                this.designerItem = DataContext as DesignerItem;

                if (this.designerItem != null)
                {
                    this.designerCanvas = VisualTreeHelper.GetParent(this.designerItem) as DesignerCanvas;

                }
            }
            catch { }
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            try
            {
                if (this.designerItem != null && this.designerCanvas != null && this.designerItem.IsSelected)
                {
                    double minLeft = double.MaxValue;
                    double minTop = double.MaxValue;
                    double minRight = double.MaxValue;
                    double minBottom = double.MaxValue;

                    //change by vinod
                    double dbCanvasWidth = designerCanvas.ActualWidth;
                    double dbCanvasHeight = designerCanvas.ActualHeight;


                    foreach (DesignerItem item in this.designerCanvas.SelectedItems)
                    {
                        minLeft = Math.Min(Canvas.GetLeft(item), minLeft);
                        minTop = Math.Min(Canvas.GetTop(item), minTop);
                        //minRight = minLeft + item.Width;
                        //minBottom = minTop + item.Height;

                        //change by vinod
                        //get actual height and width of control
                        minRight = minLeft + item.ActualWidth;
                        minBottom = minTop + item.ActualHeight;

                    }



                    //if ((designerCanvas.CanvasWidth <= minRight) || (designerCanvas.CanvasHeigth <= minBottom))
                    //    return;

                    double deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
                    double deltaVertical = Math.Max(-minTop, e.VerticalChange);

                    #region "Restrict to move out side canvas"
                    //change by vinod
                    double maxHeight = minBottom + e.VerticalChange;
                    double maxwidth = minRight + e.HorizontalChange;

                    maxHeight = maxHeight + 10;
                    maxwidth = maxwidth + 10;

                    if ((maxwidth > designerCanvas.ActualWidth) || (maxHeight > designerCanvas.ActualHeight))
                        return;

                    #endregion
                    //if (designerCanvas.Width < minRight)
                    //    minRight = designerCanvas.Width;

                    //if (designerCanvas.Height < minBottom)
                    //    minBottom = designerCanvas.Width;





                    foreach (DesignerItem item in this.designerCanvas.SelectedItems)
                    {
                        Canvas.SetLeft(item, Canvas.GetLeft(item) + deltaHorizontal);
                        Canvas.SetTop(item, Canvas.GetTop(item) + deltaVertical);
                        //Canvas.SetRight(item, minRight);
                        //Canvas.SetBottom(item, minBottom);
                    }

                    this.designerCanvas.InvalidateMeasure();
                    e.Handled = true;
                }
            }
            catch { }
        }
    }
}
