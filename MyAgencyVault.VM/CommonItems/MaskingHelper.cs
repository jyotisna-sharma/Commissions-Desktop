using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Globalization;

namespace MyAgencyVault.ViewModel.CommonItems
{  
    public class MaskingHelper : TextBox
    {        
        public static readonly DependencyProperty InputMaskProperty;

        private List<InputMaskChar> _maskChars;
        private int _caretIndex;

        public string Text
        {
            get 
            {
                return base.Text; 
            }
            set { base.Text = value; }
        }

        static MaskingHelper()
        {
            TextProperty.OverrideMetadata(typeof(MaskingHelper),
                new FrameworkPropertyMetadata(null, new CoerceValueCallback(Text_CoerceValue)));
            InputMaskProperty = DependencyProperty.Register("InputMask", typeof(string), typeof(MaskingHelper),
                new PropertyMetadata(string.Empty, new PropertyChangedCallback(InputMask_Changed)));
        }

        public MaskingHelper()
        {
            this._maskChars = new List<InputMaskChar>();            
            DataObject.AddPastingHandler(this, new DataObjectPastingEventHandler(MaskingHelper_Paste));
        }

        /// <summary>
        /// Get or Set the input mask.
        /// </summary>
        public string InputMask
        {
            get { return this.GetValue(InputMaskProperty) as string; }
            set { this.SetValue(InputMaskProperty, value); }
        }

        [Flags]
        protected enum InputMaskValidationFlags
        {
            None = 0,
            AllowInteger = 1,
            AllowDecimal = 2,
            AllowAlphabet = 4,
            AllowAlphanumeric = 8
        }

        /// <summary>
        /// Returns a value indicating if the current text value is valid.
        /// </summary>
        /// <returns></returns>
        public bool IsTextValid()
        {
            string value;
            return this.ValidateTextInternal(this.Text, out value);
        }

        private class InputMaskChar
        {
            private InputMaskValidationFlags _validationFlags;
            private char _literal;

            public InputMaskChar(InputMaskValidationFlags validationFlags)
            {
                this._validationFlags = validationFlags;
                this._literal = (char)0;
            }

            public InputMaskChar(char literal)
            {
                this._literal = literal;
            }

            public InputMaskValidationFlags ValidationFlags
            {
                get { return this._validationFlags; }
                set { this._validationFlags = value; }
            }

            public char Literal
            {
                get { return this._literal; }
                set { this._literal = value; }
            }

            public bool IsLiteral()
            {
                return (this._literal != (char)0);
            }

            public char GetDefaultChar()
            {
                return (this.IsLiteral()) ? this.Literal : '_';
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            //DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(TextProperty, typeof(TextBox));
            //if (dpd != null)
            //{
            //    dpd.AddValueChanged(this, delegate
            //    {
            //        this.UpdateInputMask();
            //    });
            //}

        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            this._caretIndex = this.CaretIndex;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            //no mask specified, just function as a normal textbox
            if (this._maskChars.Count == 0)
                return;

            if (e.Key == Key.Delete)
            {
                //delete key pressed: delete all text
                this.Text = this.GetDefaultText();
                this._caretIndex = this.CaretIndex = 0;
                e.Handled = true;
            }
            else
            {
                //backspace key pressed
                if (e.Key == Key.Back)
                {
                    if (this._caretIndex > 0 || this.SelectionLength > 0)
                    {
                        if (this.SelectionLength > 0)
                        {
                            //if one or more characters selected, delete them
                            this.DeleteSelectedText();
                        }
                        else
                      {
                            //if no characters selected, shift the caret back to the previous non-literal char and delete it
                            this.MoveBack();

                            char[] characters = this.Text.ToCharArray();
                            characters[this._caretIndex] = this._maskChars[this._caretIndex].GetDefaultChar();
                            this.Text = new string(characters);
                        }

                        //update the base class caret index, and swallow the event
                        this.CaretIndex = this._caretIndex;
                        e.Handled = true;
                    }
                }
                else if (e.Key == Key.Left)
                {
                    //move back to the previous non-literal character
                    this.MoveBack();
                    e.Handled = true;
                }
                else if (e.Key == Key.Right || e.Key == Key.Space)
                {
                    //move forwards to the next non-literal character
                    this.MoveForward();
                    e.Handled = true;
                }
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {

            base.OnPreviewTextInput(e);

            //no mask specified, just function as a normal textbox
            if (this._maskChars.Count == 0)
                return;

            this._caretIndex = this.CaretIndex = this.SelectionStart;

            if (this._caretIndex == this._maskChars.Count)
            {
                //at the end of the character count defined by the input mask- no more characters allowed
                e.Handled = true;
            }
            else
            {
                //validate the character against its validation scheme
                bool isValid = this.ValidateInputChar(char.Parse(e.Text),
                    this._maskChars[this._caretIndex].ValidationFlags);

                if (isValid)
                {
                    //delete any selected text
                    if (this.SelectionLength > 0)
                    {
                        this.DeleteSelectedText();
                    }

                    //insert the new character
                    char[] characters = this.Text.ToCharArray();
                    characters[this._caretIndex] = char.Parse(e.Text);
                    this.Text = new string(characters);

                    //move the caret on 
                    this.MoveForward();
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Validates the specified character against all selected validation schemes.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="validationFlags"></param>
        /// <returns></returns>
        protected virtual bool ValidateInputChar(char input, InputMaskValidationFlags validationFlags)
        {
            bool valid = (validationFlags == InputMaskValidationFlags.None);

            if (!valid)
            {
                Array values = Enum.GetValues(typeof(InputMaskValidationFlags));

                //iterate through the validation schemes
                foreach (object o in values)
                {
                    InputMaskValidationFlags instance = (InputMaskValidationFlags)(int)o;
                    if ((instance & validationFlags) != 0)
                    {
                        if (this.ValidateCharInternal(input, instance))
                        {
                            valid = true;
                            break;
                        }
                    }
                }
            }

            return valid;
        }

        /// <summary>
        /// Returns a value indicating if the current text value is valid.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ValidateTextInternal(string text, out string displayText)
        {
            if (this._maskChars.Count == 0)
            {
                displayText = text;
                return true;
            }

            StringBuilder displayTextBuilder = new StringBuilder(this.GetDefaultText());

            bool valid = (!string.IsNullOrEmpty(text) &&
                text.Length <= this._maskChars.Count);

            if (valid)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (!this._maskChars[i].IsLiteral())
                    {
                        if (this.ValidateInputChar(text[i], this._maskChars[i].ValidationFlags))
                        {
                            displayTextBuilder[i] = text[i];
                        }
                        else
                        {
                            valid = false;
                        }
                    }
                }
            }

            displayText = displayTextBuilder.ToString();

            return valid;
        }

        /// <summary>
        /// Deletes the currently selected text.
        /// </summary>
        protected virtual void DeleteSelectedText()
        {
            StringBuilder text = new StringBuilder(this.Text);
            string defaultText = this.GetDefaultText();
            int selectionStart = this.SelectionStart;
            int selectionLength = this.SelectionLength;

            text.Remove(selectionStart, selectionLength);
            text.Insert(selectionStart, defaultText.Substring(selectionStart, selectionLength));
            this.Text = text.ToString();

            //reset the caret position
            this.CaretIndex = this._caretIndex = selectionStart;
        }

        /// <summary>
        /// Returns a value indicating if the specified input mask character is a placeholder.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="validationFlags">If the character is a placeholder, returns the relevant validation scheme.</param>
        /// <returns></returns>
        protected virtual bool IsPlaceholderChar(char character, out InputMaskValidationFlags validationFlags)
        {
            validationFlags = InputMaskValidationFlags.None;

            switch (character.ToString().ToUpper())
            {
                case "I":
                    validationFlags = InputMaskValidationFlags.AllowInteger;
                    break;
                case "D":
                    validationFlags = InputMaskValidationFlags.AllowDecimal;
                    break;
                case "A":
                    validationFlags = InputMaskValidationFlags.AllowAlphabet;
                    break;
                case "W":
                    validationFlags = (InputMaskValidationFlags.AllowAlphanumeric);
                    break;
            }

            return (validationFlags != InputMaskValidationFlags.None);
        }

        /// <summary>
        /// Invoked when the coerce value callback is invoked.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private static object Text_CoerceValue(DependencyObject obj, object value)
        {
            MaskingHelper mtb = (MaskingHelper)obj;

            if (value == null || value.Equals(string.Empty))
                value = mtb.GetDefaultText();
            else if (value.ToString().Length > 0)
            {
                string displayText;
                mtb.ValidateTextInternal(value.ToString(), out displayText);
                value = displayText;
            }

            return value;
        }

        /// <summary>
        /// Invoked when the InputMask dependency property reports a change.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private static void InputMask_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as MaskingHelper).UpdateInputMask();
        }

        /// <summary>
        /// Invokes when a paste event is raised.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaskingHelper_Paste(object sender, DataObjectPastingEventArgs e)
        {
            //TODO: play nicely here?
            //

            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string value = e.DataObject.GetData(typeof(string)).ToString();
                string displayText;

                if (this.ValidateTextInternal(value, out displayText))
                {
                    this.Text = displayText;
                }
            }

            e.CancelCommand();
        }

        /// <summary>
        /// Rebuilds the InputMaskChars collection when the input mask property is updated.
        /// </summary>
        private void UpdateInputMask()
        {

            string text = this.Text;
            this._maskChars.Clear();

            this.Text = string.Empty;

            string mask = this.InputMask;

            if (string.IsNullOrEmpty(mask))
                return;

            InputMaskValidationFlags validationFlags = InputMaskValidationFlags.None;

            for (int i = 0; i < mask.Length; i++)
            {
                bool isPlaceholder = this.IsPlaceholderChar(mask[i], out validationFlags);

                if (isPlaceholder)
                {
                    this._maskChars.Add(new InputMaskChar(validationFlags));
                }
                else
                {
                    this._maskChars.Add(new InputMaskChar(mask[i]));
                }
            }

            string displayText;
            if (text.Length > 0 && this.ValidateTextInternal(text, out displayText))
            {
                this.Text = displayText;
            }
            else
            {
                this.Text = this.GetDefaultText();
            }
        }

        /// <summary>
        /// Validates the specified character against its input mask validation scheme.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="validationType"></param>
        /// <returns></returns>
        private bool ValidateCharInternal(char input, InputMaskValidationFlags validationType)
        {
            bool valid = false;

            switch (validationType)
            {
                case InputMaskValidationFlags.AllowInteger:
                case InputMaskValidationFlags.AllowDecimal:
                    int i;
                    if (validationType == InputMaskValidationFlags.AllowDecimal &&
                        input == '.' && !this.Text.Contains('.'))
                    {
                        valid = true;
                    }
                    else
                    {
                        valid = int.TryParse(input.ToString(), out i);
                    }
                    break;
                case InputMaskValidationFlags.AllowAlphabet:
                    valid = char.IsLetter(input);
                    break;
                case InputMaskValidationFlags.AllowAlphanumeric:
                    valid = (char.IsLetter(input) || char.IsNumber(input));
                    break;
            }

            return valid;
        }

        /// <summary>
        /// Builds the default display text for the control.
        /// </summary>
        /// <returns></returns>
        private string GetDefaultText()
        {
            StringBuilder text = new StringBuilder();
            foreach (InputMaskChar maskChar in this._maskChars)
            {
                text.Append(maskChar.GetDefaultChar());
            }
            return text.ToString();
        }

        /// <summary>
        /// Moves the caret forward to the next non-literal position.
        /// </summary>
        private void MoveForward()
        {
            int pos = this._caretIndex;
            while (pos < this._maskChars.Count)
            {
                if (++pos == this._maskChars.Count || !this._maskChars[pos].IsLiteral())
                {
                    this._caretIndex = this.CaretIndex = pos;
                    break;
                }
            }
        }

        /// <summary>
        /// Moves the caret backward to the previous non-literal position.
        /// </summary>
        private void MoveBack()
        {
            int pos = this._caretIndex;
            while (pos > 0)
            {
                if (--pos == 0 || !this._maskChars[pos].IsLiteral())
                {
                    this._caretIndex = this.CaretIndex = pos;
                    break;
                }
            }
        }
    }

    public class TextBoxMaskBehavior
    {
        #region MinimumValue Property

        public static double GetMinimumValue(DependencyObject obj)
        {
            return (double)obj.GetValue(MinimumValueProperty);
        }

        public static void SetMinimumValue(DependencyObject obj, double value)
        {
            obj.SetValue(MinimumValueProperty, value);
        }

        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.RegisterAttached(
                "MinimumValue",
                typeof(double),
                typeof(TextBoxMaskBehavior),
                new FrameworkPropertyMetadata(double.NaN, MinimumValueChangedCallback)
                );

        private static void MinimumValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox _this = (d as TextBox);
            ValidateTextBox(_this);
        }
        #endregion

        #region MaximumValue Property

        public static double GetMaximumValue(DependencyObject obj)
        {
            return (double)obj.GetValue(MaximumValueProperty);
        }

        public static void SetMaximumValue(DependencyObject obj, double value)
        {
            obj.SetValue(MaximumValueProperty, value);
        }

        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.RegisterAttached(
                "MaximumValue",
                typeof(double),
                typeof(TextBoxMaskBehavior),
                new FrameworkPropertyMetadata(double.NaN, MaximumValueChangedCallback)
                );

        private static void MaximumValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox _this = (d as TextBox);
            ValidateTextBox(_this);
        }
        #endregion

        #region Mask Property

        public static MaskType GetMask(DependencyObject obj)
        {
            return (MaskType)obj.GetValue(MaskProperty);
        }

        public static void SetMask(DependencyObject obj, MaskType value)
        {
            obj.SetValue(MaskProperty, value);
        }

        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.RegisterAttached(
                "Mask",
                typeof(MaskType),
                typeof(TextBoxMaskBehavior),
                new FrameworkPropertyMetadata(MaskChangedCallback)
                );

        private static void MaskChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is TextBox)
            {
                (e.OldValue as TextBox).PreviewTextInput -= TextBox_PreviewTextInput;
                DataObject.RemovePastingHandler((e.OldValue as TextBox), (DataObjectPastingEventHandler)TextBoxPastingEventHandler);
            }

            TextBox _this = (d as TextBox);
            if (_this == null)
                return;

            if ((MaskType)e.NewValue != MaskType.Any)
            {
                _this.PreviewTextInput += TextBox_PreviewTextInput;
                DataObject.AddPastingHandler(_this, (DataObjectPastingEventHandler)TextBoxPastingEventHandler);
            }

            ValidateTextBox(_this);
        }

        #endregion

        #region Private Static Methods

        private static void ValidateTextBox(TextBox _this)
        {
            if (GetMask(_this) != MaskType.Any)
            {
                _this.Text = ValidateValue(GetMask(_this), _this.Text, GetMinimumValue(_this), GetMaximumValue(_this));
            }
        }

        private static void TextBoxPastingEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            TextBox _this = (sender as TextBox);
            string clipboard = e.DataObject.GetData(typeof(string)) as string;
            clipboard = ValidateValue(GetMask(_this), clipboard, GetMinimumValue(_this), GetMaximumValue(_this));
            if (!string.IsNullOrEmpty(clipboard))
            {
                _this.Text = clipboard;
            }
            e.CancelCommand();
            e.Handled = true;
        }

        private static void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox _this = (sender as TextBox);
            bool isValid = IsSymbolValid(GetMask(_this), e.Text);
            e.Handled = !isValid;
            if (isValid)
            {
                int caret = _this.CaretIndex;
                string text = _this.Text;
                bool textInserted = false;
                int selectionLength = 0;

                if (_this.SelectionLength > 0)
                {
                    text = text.Substring(0, _this.SelectionStart) +
                            text.Substring(_this.SelectionStart + _this.SelectionLength);
                    caret = _this.SelectionStart;
                }

                if (e.Text == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                {
                    while (true)
                    {
                        int ind = text.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
                        if (ind == -1)
                            break;

                        text = text.Substring(0, ind) + text.Substring(ind + 1);
                        if (caret > ind)
                            caret--;
                    }

                    if (caret == 0)
                    {
                        text = "0" + text;
                        caret++;
                    }
                    else
                    {
                        if (caret == 1 && string.Empty + text[0] == NumberFormatInfo.CurrentInfo.NegativeSign)
                        {
                            text = NumberFormatInfo.CurrentInfo.NegativeSign + "0" + text.Substring(1);
                            caret++;
                        }
                    }

                    if (caret == text.Length)
                    {
                        selectionLength = 1;
                        textInserted = true;
                        text = text + NumberFormatInfo.CurrentInfo.NumberDecimalSeparator + "0";
                        caret++;
                    }
                }
                else if (e.Text == NumberFormatInfo.CurrentInfo.NegativeSign)
                {
                    textInserted = true;
                    if (_this.Text.Contains(NumberFormatInfo.CurrentInfo.NegativeSign))
                    {
                        text = text.Replace(NumberFormatInfo.CurrentInfo.NegativeSign, string.Empty);
                        if (caret != 0)
                            caret--;
                    }
                    else
                    {
                        text = NumberFormatInfo.CurrentInfo.NegativeSign + _this.Text;
                        caret++;
                    }
                }

                if (!textInserted)
                {
                    text = text.Substring(0, caret) + e.Text +
                        ((caret < _this.Text.Length) ? text.Substring(caret) : string.Empty);

                    caret++;
                }

                try
                {
                    double val = Convert.ToDouble(text);
                    double newVal = ValidateLimits(GetMinimumValue(_this), GetMaximumValue(_this), val);
                    if (val != newVal)
                    {
                        text = newVal.ToString();
                    }
                    else if (val == 0)
                    {
                        if (!text.Contains(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator))
                            text = "0";
                    }
                }
                catch
                {
                    text = "0";
                }

                while (text.Length > 1 && text[0] == '0' && string.Empty + text[1] != NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                {
                    text = text.Substring(1);
                    if (caret > 0)
                        caret--;
                }

                while (text.Length > 2 && string.Empty + text[0] == NumberFormatInfo.CurrentInfo.NegativeSign && text[1] == '0' && string.Empty + text[2] != NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                {
                    text = NumberFormatInfo.CurrentInfo.NegativeSign + text.Substring(2);
                    if (caret > 1)
                        caret--;
                }

                if (caret > text.Length)
                    caret = text.Length;

                _this.Text = text;
                _this.CaretIndex = caret;
                _this.SelectionStart = caret;
                _this.SelectionLength = selectionLength;
                e.Handled = true;
            }
        }

        private static string ValidateValue(MaskType mask, string value, double min, double max)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            value = value.Trim();
            switch (mask)
            {
                case MaskType.Integer:
                    try
                    {
                        Convert.ToInt64(value);
                        return value;
                    }
                    catch
                    {
                    }
                    return string.Empty;

                case MaskType.Decimal:
                    try
                    {
                        Convert.ToDouble(value);

                        return value;
                    }
                    catch
                    {
                    }
                    return string.Empty;
            }

            return value;
        }

        private static double ValidateLimits(double min, double max, double value)
        {
            if (!min.Equals(double.NaN))
            {
                if (value < min)
                    return min;
            }

            if (!max.Equals(double.NaN))
            {
                if (value > max)
                    return max;
            }

            return value;
        }

        private static bool IsSymbolValid(MaskType mask, string str)
        {
            switch (mask)
            {
                case MaskType.Any:
                    return true;

                case MaskType.Integer:
                    if (str == NumberFormatInfo.CurrentInfo.NegativeSign)
                        return true;
                    break;

                case MaskType.Decimal:
                    if (str == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator ||
                        str == NumberFormatInfo.CurrentInfo.NegativeSign)
                        return true;
                    break;
            }

            if (mask.Equals(MaskType.Integer) || mask.Equals(MaskType.Decimal))
            {
                foreach (char ch in str)
                {
                    if (!Char.IsDigit(ch))
                        return false;
                }

                return true;
            }

            return false;
        }

        #endregion
    }

    public enum MaskType
    {
        Any,
        Integer,
        Decimal
    }     
}
