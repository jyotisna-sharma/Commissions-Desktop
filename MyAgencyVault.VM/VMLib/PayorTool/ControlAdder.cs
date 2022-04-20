using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using MyAgencyVault.ViewModel;
using System.Windows.Controls;
using System.Windows.Media;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Collections.ObjectModel;
using System.Windows.Data;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.VM;
namespace MyAgencyVault.ViewModel.PayorToolLib
{
    public static class ControlAdder
    {
        public static readonly DependencyProperty ControlAddPeroperty =
            DependencyProperty.RegisterAttached("ControlAdd", typeof(List<CanvasDroppedField>), typeof(ControlAdder),
                                                                                                            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnControlAddChanged)));
        public static readonly DependencyProperty LoadControlsProperty =
        DependencyProperty.RegisterAttached("LoadControls", typeof(ObservableCollection<PayorToolField>), typeof(ControlAdder),
                                                                                                        new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLoadControlsChanged)));

        public static readonly DependencyProperty ShowHideControlProperty =
       DependencyProperty.RegisterAttached("ShowHideControl", typeof(string), typeof(ControlAdder),
                                                                                                      new FrameworkPropertyMetadata(new PropertyChangedCallback(OnShowHideControlChanged)));


        public static string GetShowHideControl(DependencyObject obj)
        {
            return obj.GetValue(ShowHideControlProperty).ToString();
        }
        public static void SetShowHideControl(DependencyObject obj, int value)
        {
            obj.SetValue(ShowHideControlProperty, value);
        }
        private static void OnShowHideControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Canvas ObjCanva = d as Canvas;
            try
            {
                string strval = e.NewValue.ToString();
                if (strval == "s") //if user click on statement image
                {
                    foreach (UIElement objText in ObjCanva.Children)
                    {
                        if (objText is TextBox || objText is TextBlock)
                        {
                            objText.Visibility = Visibility.Visible;
                        }
                    }
                }
                else //if user click on cheque image
                {
                    foreach (UIElement objText in ObjCanva.Children)
                    {
                        if (objText is TextBox || objText is TextBlock)
                        {
                            objText.Visibility = Visibility.Hidden;
                        }
                    }
                }
            }
            catch
            {
            }

        }

        public static ObservableCollection<PayorToolField> GetLoadControls(DependencyObject obj)
        {
            return (ObservableCollection<PayorToolField>)obj.GetValue(LoadControlsProperty);
        }
        public static void SetLoadControls(DependencyObject obj, int value)
        {
            obj.SetValue(LoadControlsProperty, value);
        }

        private static void OnLoadControlsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Canvas ObjCanvas = d as Canvas;
            try
            {
                ObservableCollection<PayorToolField> ToolsFields = (ObservableCollection<PayorToolField>)e.NewValue;
                List<UIElement> lst = new List<UIElement>();
                foreach (UIElement objText in ObjCanvas.Children)
                {
                    if (objText is TextBox || objText is TextBlock)
                    {

                        lst.Add(objText);
                    }
                }
                lst.ForEach(p => ObjCanvas.Children.Remove(p));
                int i = 0;
                if (ToolsFields != null)
                {
                    foreach (PayorToolField Field in ToolsFields)
                    {

                        //Manual Binding for the Droped field ON DEU
                        if (ToolsFields[i].FieldValue == null)
                            ToolsFields[i].FieldValue = "";
                        Binding b = new Binding();
                        b.Source = ObjCanvas.DataContext;
                        b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        b.Path = new PropertyPath("CurrentPayorToolFields[" + i + "].FieldValue");
                        b.Mode = BindingMode.TwoWay;
                        //code updated on 5 dec 2010 provide masking feature
                        MaskingHelper txtBox = new MaskingHelper();
                        txtBox.Width = 120;
                        txtBox.Height = 21;
                        txtBox.Tag = Field.PayorFieldID;
                        txtBox.SetBinding(TextBox.TextProperty, b);
                        string MaskName = string.Empty;
                        using (ServiceClients serviceClients = new ServiceClients())
                        {
                          MaskName = serviceClients.MasterClient.GetMaskName(Field.MaskFieldTypeId);
                        }
                        MaskName = MaskName.Replace("mmm", "aaa");
                        MaskName = MaskName.Replace("mm", "ii");
                        MaskName = MaskName.Replace("yyyy", "iiii");
                        MaskName = MaskName.Replace("yy", "ii");
                        MaskName = MaskName.Replace("dd", "ii");
                        txtBox.InputMask = MaskName;

                        //Add Label for the droped control
                        TextBlock txtblk = new TextBlock();
                        txtblk.Width = 120;
                        txtblk.Height = 20;
                        txtblk.Text = Field.LabelOnField;
                        ObjCanvas.Children.Add(txtblk);
                        Canvas.SetLeft(txtblk, Field.ControlX);
                        Canvas.SetTop(txtblk, Field.ControlY - 12);

                        //blk.Background = new SolidColorBrush(Colors.Wheat);
                        //add Dropped control on UI
                        ObjCanvas.Children.Add(txtBox);
                        Canvas.SetLeft(txtBox, Field.ControlX);
                        Canvas.SetTop(txtBox, Field.ControlY + 10);
                        i++;

                    }
                }
            }
            catch (Exception)
            {
            }

        }
        public static List<CanvasDroppedField> GetControlAdd(DependencyObject obj)
        {
            return (List<CanvasDroppedField>)obj.GetValue(ControlAddPeroperty);
        }
        public static void SetControlAdd(DependencyObject obj, int value)
        {
            obj.SetValue(ControlAddPeroperty, value);
        }
        private static void OnControlAddChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Canvas _can = d as Canvas;
            List<UIElement> _lst = new List<UIElement>();
            //_can.Children.Clear();
            try
            {
                foreach (UIElement Element in _can.Children)
                {
                    if (Element is TextBlock)
                    {
                        _lst.Add(Element);
                    }
                }
                _lst.ForEach(ui => _can.Children.Remove(ui));
                List<CanvasDroppedField> _ControlList = (List<CanvasDroppedField>)e.NewValue;
                if (_ControlList == null)
                {
                    foreach (UIElement Element in _can.Children)
                    {
                        if (Element is TextBlock)
                        {
                            _lst.Add(Element);
                        }
                    }
                    _lst.ForEach(ui => _can.Children.Remove(ui));
                }
                else
                {
                    foreach (CanvasDroppedField PayorField in _ControlList)
                    {
                        TextBlock blk = new TextBlock();
                        blk.Text = PayorField.FieldName;
                        blk.Width = 120;
                        blk.Tag = PayorField.FieldId;
                        blk.Background = new SolidColorBrush(Colors.White);
                        _can.Children.Add(blk);
                        Canvas.SetLeft(blk, PayorField.ControlX);
                        Canvas.SetTop(blk, PayorField.ControlY);

                        MyAgencyVault.ViewModel.CanvasDroppedField _Field = MyAgencyVault.ViewModel.PayorToolVM.DroppedFieldList.Find(p => p.FieldId == PayorField.FieldId);
                        if (_Field == null)
                            MyAgencyVault.ViewModel.PayorToolVM.DroppedFieldList.Add(new CanvasDroppedField { ControlX = PayorField.ControlX, ControlY = PayorField.ControlY, FieldName = PayorField.FieldName, FieldId = PayorField.FieldId });
                        else
                        {
                            MyAgencyVault.ViewModel.PayorToolVM.DroppedFieldList.Remove(_Field);
                            MyAgencyVault.ViewModel.PayorToolVM.DroppedFieldList.Add(new CanvasDroppedField { ControlX = PayorField.ControlX, ControlY = PayorField.ControlY, FieldName = PayorField.FieldName, FieldId = PayorField.FieldId });
                        }

                    }
                }
            }
            catch (Exception)
            {
            }

        }

    }
}
