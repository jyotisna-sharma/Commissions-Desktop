using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using MyAgencyVault.VM.VMLib.Adorners;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.ViewModel;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.ViewModel.Converters;
using System.Collections.ObjectModel;

namespace MyAgencyVault.VM.VMLib.PayorForm
{
    public class DesignerCanvas : Canvas
    {
        #region VM Delegate Instance

        private PayorToolVMDelegate m_PayorToolVMDelegate;
        public double CanvasWidth;
        public double CanvasHeigth;
        public List<DesignerItem> DesignerItems = new List<DesignerItem>();

        public DesignerCanvas()
        {
            m_PayorToolVMDelegate = new PayorToolVMDelegate();
            CanvasWidth = this.ActualWidth;
            CanvasHeigth = this.ActualHeight;
            m_PayorToolVMDelegate.SetCanvas(this);
        }

        #endregion

        private System.Windows.Point? dragStartPoint = null;

        public IEnumerable<DesignerItem> SelectedItems
        {
            get
            {
                var selectedItems = from item in this.Children.OfType<DesignerItem>()
                                    where item.IsSelected == true
                                    select item;

                return selectedItems;
            }
        }

        public void DeleteSelected()
        {
            DesignerItem item = null;
            Guid payorId = Guid.Empty;

            try
            {
                while (this.SelectedItems.Count() != 0)
                {
                    item = this.SelectedItems.FirstOrDefault();
                    int field = 0;

                    m_PayorToolVMDelegate.OnFieldDeleted((item.Content as TextBoxDI).Field);
                    payorId = (item.Content as TextBoxDI).PayorId;
                    field = (item.Content as TextBoxDI).Field.FieldID;

                    DesignerItems.Remove(DesignerItems.FirstOrDefault(s => (s.Content as TextBoxDI).PayorId == payorId && (s.Content as TextBoxDI).Field.FieldID == field));
                    this.Children.Remove(item);
                }

                if (this.Children.Count > 2)
                {
                    this.DeselectAll();
                    DesignerItem temp = DesignerItems.Find(s => (((s.Content as TextBoxDI).PayorId) == payorId));
                    if (temp != null)
                    {
                        temp.IsSelected = true;
                        m_PayorToolVMDelegate.OnFieldSelectionChanged((temp.Content as TextBoxDI).Field);
                    }
                    else
                    {
                        m_PayorToolVMDelegate.OnFieldSelectionChanged(null);
                    }
                }
                else
                {
                    m_PayorToolVMDelegate.OnFieldSelectionChanged(null);
                }
            }
            catch { }

        }

        public void SelectField(int fieldId,Guid PayorId)
        {
            if (fieldId != 0)
            {  
                //DesignerItem temp = DesignerItems.Find(s => (((s.Content as TextBoxDI).Field.FieldID) == fieldId) && (((s.Content as TextBoxDI).PayorId) == PayorId));
                //With check null
                DesignerItem temp = DesignerItems.Find(s => (((s.Content as TextBoxDI).Field != null) && (((s.Content as TextBoxDI).Field.FieldID) == fieldId) && (((s.Content as TextBoxDI).PayorId) == PayorId)));
                if (temp != null)
                    temp.IsSelected = true;
            }
        }

        public void DeselectAll()
        {
            foreach (DesignerItem item in this.SelectedItems)
            {
                item.IsSelected = false;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            //if (e.Source == this.Children[1])
            //{
            //    this.dragStartPoint = new System.Windows.Point?(e.GetPosition(this));
            //    this.DeselectAll();
            //    e.Handled = true;
            //}

            //Keep focus on canvas required to fire previewkeydown event on canvas.
            (this.Children[0] as TextBox).Focus();
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
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    RubberbandAdorner adorner = new RubberbandAdorner(this, this.dragStartPoint);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }

                e.Handled = true;
            }

            //Keep focus on canvas required to fire previewkeydown event on canvas.
            (this.Children[0] as TextBox).Focus();
        }

        public void CopyFieldsFromPayor(Guid payorId, ObservableCollection<PayorToolField> payorToolFields)
        {
            try
            {
                List<DesignerItem> SourceItems = DesignerItems.FindAll(s => ((s.Content as TextBoxDI).PayorId) == payorId);
               
                Guid targetPayerid = m_PayorToolVMDelegate.getPayorId();
                List<DesignerItem> tempDestinationItems= DesignerItems.FindAll(s => ((s.Content as TextBoxDI).PayorId) == payorId);
                if(tempDestinationItems!=null)
                    if(tempDestinationItems.Count>0)
                        DeleteAllFieldsForPayor(targetPayerid);
                foreach (DesignerItem Item in SourceItems)
                {
                    DesignerItem newItem = null;

                    TextBoxDI sourceTextBox = Item.Content as TextBoxDI;
                    TextBoxDI targetTextBox = new TextBoxDI();

                    targetTextBox.Width = sourceTextBox.Width;
                    targetTextBox.Height = sourceTextBox.Height;
                    targetTextBox.Text = sourceTextBox.Field.FieldName;
                    targetTextBox.Field = sourceTextBox.Field;
                    targetTextBox.PayorId = m_PayorToolVMDelegate.getPayorId();
                    targetTextBox.IsHitTestVisible = false;
                    targetTextBox.IsReadOnly = true;
                    double initialWidth = targetTextBox.Width;
                    double initialHeight = targetTextBox.Height;

                    if (targetTextBox != null)
                    {
                        newItem = new DesignerItem();
                        newItem.Content = targetTextBox;

                        DesignerCanvas.SetLeft(newItem, DesignerCanvas.GetLeft(Item));
                        DesignerCanvas.SetTop(newItem, DesignerCanvas.GetTop(Item));
                        DesignerCanvas.SetZIndex(newItem, 2);

                        this.Children.Add(newItem);
                    }

                    DesignerItems.Add(newItem);

                    System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
                    binding.Source = payorToolFields.FirstOrDefault(s => s.AvailableFieldName == sourceTextBox.Field.FieldName);
                    binding.Path = new PropertyPath("IsNotVisible");
                    binding.Converter = new VisibilityConverter();
                    newItem.SetBinding(TextBox.VisibilityProperty, binding);
                   
                }
            }
            catch { }
        }

        public void DropFieldOnCanvas(PayorToolField payorToolField,PayorToolAvailablelFieldType availableFieldType,double X,double Y,double Width,double Height)
        {
            try
            {
                if (payorToolField != null)
                {
                    DesignerItem newItem = null;

                    TextBoxDI textBox = new TextBoxDI();
                    textBox.Width = Width;
                    textBox.Height = Height;
                    textBox.Text = payorToolField.AvailableFieldName;
                    textBox.Field = availableFieldType;
                    textBox.PayorId = m_PayorToolVMDelegate.getPayorId();
                    textBox.IsHitTestVisible = false;
                    textBox.IsReadOnly = true;
                    textBox.FontSize = 9;

                    double initialWidth = textBox.Width;
                    double initialHeight = textBox.Height;

                    if (textBox != null)
                    {
                        newItem = new DesignerItem();
                        newItem.Content = textBox;

                        DesignerCanvas.SetLeft(newItem, X);
                        DesignerCanvas.SetTop(newItem, Y);
                        DesignerCanvas.SetZIndex(newItem, 2);

                        this.Children.Add(newItem);
                        //this.DeselectAll();

                        //newItem.IsSelected = true;
                        //m_PayorToolVMDelegate.OnFieldSelectionChanged((newItem.Content as TextBoxDI).Field);

                        textBox.Focus();
                    }

                    DesignerItems.Add(newItem);
                    //PayorToolField tempPayorToolField = m_PayorToolVMDelegate.OnDropCompleted(payorToolField, DesignerCanvas.GetLeft(newItem), DesignerCanvas.GetTop(newItem), newItem.Width, newItem.Height);

                    //bind textbox with tempPayorToolField.IsNotVisible field
                    System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
                    binding.Source = payorToolField;
                    binding.Path = new PropertyPath("IsNotVisible");
                    binding.Converter = new VisibilityConverter();
                    newItem.SetBinding(TextBox.VisibilityProperty, binding);
                }

                //Keep focus on canvas required to fire previewkeydown event on canvas.
                (this.Children[0] as TextBox).Focus();
            }
            catch { }
        }

        public void ShowPayorFields(Guid PayorId)
        {
            try
            {
                if (DesignerItems != null)
                {
                    foreach (DesignerItem Item in DesignerItems)
                    {
                        TextBoxDI field = Item.Content as TextBoxDI;
                        if (field.PayorId == PayorId)
                            Item.Visibility = System.Windows.Visibility.Visible;
                        else
                            Item.Visibility = System.Windows.Visibility.Hidden;
                    }
                }
            }
            catch { }
        }

        public void DeleteAllFieldsForPayor(Guid payorId)
        {
            try
            {
                List<DesignerItem> Items = DesignerItems.FindAll(s => ((s.Content as TextBoxDI).PayorId) == payorId);
                foreach (DesignerItem Item in Items)
                {
                    this.Children.Remove(Item);
                    DesignerItems.Remove(Item);
                }
            }
            catch { }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            try
            {
                base.OnDrop(e);

                PayorToolAvailablelFieldType payorToolField = e.Data.GetData("DESIGNER_ITEM") as PayorToolAvailablelFieldType;
                bool isValid = m_PayorToolVMDelegate.OnDropValidation(payorToolField);

                if (!isValid)
                    return;

                if (payorToolField != null)
                {
                    DesignerItem newItem = null;

                    TextBoxDI textBox = new TextBoxDI();
                    textBox.Width = 120;
                    textBox.Height = 23;
                    textBox.Text = payorToolField.FieldName;
                    textBox.Field = payorToolField;
                    textBox.PayorId = m_PayorToolVMDelegate.getPayorId();
                    textBox.IsHitTestVisible = false;
                    textBox.IsReadOnly = true;
                    textBox.FontSize = 9;

                    double initialWidth = textBox.Width;
                    double initialHeight = textBox.Height;

                    if (textBox != null)
                    {
                        newItem = new DesignerItem();
                        newItem.Content = textBox;

                        System.Windows.Point position = e.GetPosition(this);
                        if (textBox.MinHeight != 0 && textBox.MinWidth != 0)
                        {
                            newItem.Width = textBox.MinWidth * 2;
                            newItem.Height = textBox.MinHeight * 2;

                            TextBoxDI txtBox = newItem.Content as TextBoxDI;
                            txtBox.Width = textBox.MinWidth * 2;
                            txtBox.Height = textBox.MinHeight * 2;
                        }
                        else
                        {
                            newItem.Width = initialWidth;
                            newItem.Height = initialHeight;
                        }
                        DesignerCanvas.SetLeft(newItem, Math.Max(0, position.X - newItem.Width / 2));
                        DesignerCanvas.SetTop(newItem, Math.Max(0, position.Y - newItem.Height / 2));
                        DesignerCanvas.SetZIndex(newItem, 2);

                        this.Children.Add(newItem);

                        this.DeselectAll();
                        newItem.IsSelected = true;
                        textBox.Focus();
                    }

                    DesignerItems.Add(newItem);
                    PayorToolField tempPayorToolField = m_PayorToolVMDelegate.OnDropCompleted(payorToolField, DesignerCanvas.GetLeft(newItem), DesignerCanvas.GetTop(newItem), newItem.Width, newItem.Height);
                    m_PayorToolVMDelegate.OnFieldSelectionChanged((newItem.Content as TextBoxDI).Field);

                    //bind textbox with tempPayorToolField.IsNotVisible field
                    System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
                    binding.Source = tempPayorToolField;
                    binding.Path = new PropertyPath("IsNotVisible");
                    binding.Converter = new VisibilityConverter();
                    newItem.SetBinding(TextBox.VisibilityProperty, binding);

                    e.Handled = true;
                }

                //Keep focus on canvas required to fire previewkeydown event on canvas.
                (this.Children[0] as TextBox).Focus();
            }
            catch { }
        }

        protected override Size MeasureOverride(Size constraint)
        { 
            Size size = new Size();
            try
            {
               
                foreach (UIElement element in Children)
                {
                    double left = Canvas.GetLeft(element);
                    double top = Canvas.GetTop(element);
                    left = double.IsNaN(left) ? 0 : left;
                    top = double.IsNaN(top) ? 0 : top;

                    element.Measure(constraint);

                    Size desiredSize = element.DesiredSize;
                    if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
                    {
                        size.Width = Math.Max(size.Width, left + desiredSize.Width);
                        size.Height = Math.Max(size.Height, top + desiredSize.Height);
                    }
                }

                // add some extra margin
                size.Width += 10;
                size.Height += 10;
                return size;
            }
            catch { return size; }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Delete)
            {
                DeleteSelected();
            }
            e.Handled = true;
        }
    }
}
