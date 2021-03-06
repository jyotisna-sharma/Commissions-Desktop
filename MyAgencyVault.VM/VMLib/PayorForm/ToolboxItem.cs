using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using MyAgencyVault.VM.VMLib.Adorners;

namespace MyAgencyVault.VM.VMLib.PayorForm
{
    public class ToolboxItem : ListViewItem
    {
        private Point? dragStartPoint = null;

        //static ToolboxItem()
        //{
        //    FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolboxItem), new FrameworkPropertyMetadata(typeof(ToolboxItem)));
        //}

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            this.dragStartPoint = new Point?(e.GetPosition(this));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                this.dragStartPoint = null;
            }

            if (this.dragStartPoint.HasValue)
            {
                try
                {
                    Point position = e.GetPosition(this);
                    if ((SystemParameters.MinimumHorizontalDragDistance <=
                        Math.Abs((double)(position.X - this.dragStartPoint.Value.X))) ||
                        (SystemParameters.MinimumVerticalDragDistance <=
                        Math.Abs((double)(position.Y - this.dragStartPoint.Value.Y))))
                    {
                        DataObject dataObject = new DataObject("DESIGNER_ITEM", this.Content);

                        if (dataObject != null)
                        {
                            DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
                        }
                    }

                    e.Handled = true;
                }
                catch
                { }
            }
        }
    }
}
