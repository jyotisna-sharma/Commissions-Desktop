using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MyAgencyVault.ViewModel.Behaviour
{
    public static class TextBoxAutoComplete
    {
        #region Dependency Properties

        public static Selector m_Selector;

        public static readonly DependencyProperty WordAutoCompleteSourceProperty;
        public static readonly DependencyProperty WordAutoCompleteSeparatorsProperty;
        public static readonly DependencyProperty WordAutoCompletePopupProperty;

        private static readonly DependencyProperty WordAutoCompleteWordsHostProperty;
        private static readonly DependencyPropertyKey WordAutoCompleteWordsHostPropertyKey;
        private static readonly DependencyProperty IsSelectionChangeCausedByTextInputProperty;
        private static readonly DependencyPropertyKey IsSelectionChangeCausedByTextInputPropertyKey;

        #endregion

        #region Initialization
        static TextBoxAutoComplete()
        {
            var metadata = new FrameworkPropertyMetadata(OnWordAutoCompleteSourceChanged);
            WordAutoCompleteSourceProperty = DependencyProperty.RegisterAttached("WordAutoCompleteSource", typeof(IEnumerable), typeof(TextBoxAutoComplete), metadata);

            metadata = new FrameworkPropertyMetadata(",;");
            WordAutoCompleteSeparatorsProperty = DependencyProperty.RegisterAttached("WordAutoCompleteSeparators", typeof(string), typeof(TextBoxAutoComplete), metadata);

            metadata = new FrameworkPropertyMetadata(OnWordAutoCompletePopupChanged);
            WordAutoCompletePopupProperty = DependencyProperty.RegisterAttached("WordAutoCompletePopup", typeof(Popup), typeof(TextBoxAutoComplete), metadata);

            metadata = new FrameworkPropertyMetadata();
            WordAutoCompleteWordsHostPropertyKey = DependencyProperty.RegisterAttachedReadOnly("WordAutoCompleteWordsHost", typeof(Selector), typeof(TextBoxAutoComplete), metadata);
            WordAutoCompleteWordsHostProperty = WordAutoCompleteWordsHostPropertyKey.DependencyProperty;

            metadata = new FrameworkPropertyMetadata((object)false);
            IsSelectionChangeCausedByTextInputPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsSelectionChangeCausedByTextInput", typeof(bool), typeof(TextBoxAutoComplete), metadata);
            IsSelectionChangeCausedByTextInputProperty = IsSelectionChangeCausedByTextInputPropertyKey.DependencyProperty;
        }
        #endregion

        #region Dependency Properties Getters And Setters
        public static void SetWordAutoCompleteSource(TextBox element, IEnumerable value)
        {
            element.SetValue(WordAutoCompleteSourceProperty, value);
        }

        public static IEnumerable GetWordAutoCompleteSource(TextBox element)
        {
            return (IEnumerable)element.GetValue(WordAutoCompleteSourceProperty);
        }

        public static void SetWordAutoCompleteSeparators(TextBox element, string value)
        {
            element.SetValue(WordAutoCompleteSeparatorsProperty, value);
        }

        public static string GetWordAutoCompleteSeparators(TextBox element)
        {
            return (string)element.GetValue(WordAutoCompleteSeparatorsProperty);
        }

        public static void SetWordAutoCompletePopup(TextBox element, Popup value)
        {
            element.SetValue(WordAutoCompletePopupProperty, value);
        }

        public static Popup GetWordAutoCompletePopup(TextBox element)
        {
            return (Popup)element.GetValue(WordAutoCompletePopupProperty);
        }
        #endregion

        #region Dependency Properties Callbacks
        private static void OnWordAutoCompleteSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = (TextBox)d;
            SetWordsHostSourceAndHookupEvents(textBox, (IEnumerable)e.NewValue, GetWordAutoCompletePopup(textBox));
        }

        private static void OnWordAutoCompletePopupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = (TextBox)d;
            SetWordsHostSourceAndHookupEvents(textBox, GetWordAutoCompleteSource(textBox), (Popup)e.NewValue);
        }

        private static void SetWordsHostSourceAndHookupEvents(TextBox textBox, IEnumerable source, Popup popup)
        {
            try
            {
                if (source != null && popup != null)
                {
                    //TODO: make sure we do this only this once, in case for some reason somebody re-sets one of the attached properties.
                    textBox.PreviewKeyDown += new KeyEventHandler(TextBox_PreviewKeyDown);
                    textBox.SelectionChanged += new RoutedEventHandler(TextBox_SelectionChanged);
                    textBox.TextChanged += new TextChangedEventHandler(TextBox_TextChanged);

                    //Selector wordsHost = (Selector)popup.FindName("PART_WordsHost");
                    //if (wordsHost == null)
                    //    throw new InvalidOperationException("Can't find the PART_WordsHost element in the auto-complete popup control.");
                    m_Selector.IsSynchronizedWithCurrentItem = true;
                    m_Selector.ItemsSource = source;
                    textBox.SetValue(WordAutoCompleteWordsHostPropertyKey, m_Selector);
                }
            }
            catch { }
        }
        #endregion

        #region TextBox Event Handlers
        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox textBox = (TextBox)sender;
                Popup popup = GetWordAutoCompletePopup(textBox);
                char[] separators = GetWordAutoCompleteSeparators(textBox).ToCharArray();

                int previousSeparatorOffset = -1;
                int nextSeparatorOffset = textBox.Text.Length;
                //GetPreviousAndNextSeparatorOffsets(textBox.Text, separators, textBox.CaretIndex, out previousSeparatorOffset, out nextSeparatorOffset);

                string currentWord = textBox.Text.Substring(previousSeparatorOffset + 1, nextSeparatorOffset - (previousSeparatorOffset + 1));
                if (currentWord.Length > 0)
                {
                    // Filter all the auto-complete suggestions with what the user is currently typing.
                    Selector wordsHost = (Selector)textBox.GetValue(WordAutoCompleteWordsHostProperty);
                    wordsHost.Items.Filter = o => GetTextSearchText(wordsHost, o).StartsWith(currentWord, StringComparison.CurrentCultureIgnoreCase);
                    if (wordsHost.Items.IsEmpty)
                    {
                        // Nothing matched... hide the popup.
                        popup.IsOpen = false;
                    }
                    else
                    {
                        // Some matches have been found... show the popup, and select the first
                        // item if the previously selected item is now excluded.
                        if (popup.IsOpen)
                        {
                            //if (wordsHost.Items.IsCurrentAfterLast || wordsHost.Items.IsCurrentBeforeFirst)
                            //{
                            //    wordsHost.Items.MoveCurrentToFirst();
                            //}
                        }
                        else
                        {
                            wordsHost.Items.MoveCurrentToFirst();
                            popup.IsOpen = true;
                        }
                    }
                }
                else
                {
                    popup.IsOpen = false;
                }

                textBox.SetValue(IsSelectionChangeCausedByTextInputPropertyKey, true);
            }
            catch { }
        }

        private static void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox textBox = (TextBox)sender;
                if (!(bool)textBox.GetValue(IsSelectionChangeCausedByTextInputProperty))
                {
                    Popup popup = GetWordAutoCompletePopup(textBox);
                    popup.IsOpen = false;
                }

                // Reset to default (false).
                textBox.SetValue(IsSelectionChangeCausedByTextInputPropertyKey, false);
            }
            catch
            { }
        }

        private static void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox textBox = (TextBox)sender;
                Popup popup = GetWordAutoCompletePopup(textBox);
                if (popup.IsOpen)
                {
                    if (e.Key == Key.Escape)
                    {
                        // Escape closes the popup.
                        popup.IsOpen = false;
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Home || e.Key == Key.End)
                    {
                        // Jumping to the beginning or the end of the text closes the popup,
                        // but we don't set the event as handled because we want to still let
                        // the text box move the caret.
                        popup.IsOpen = false;
                    }
                    else
                    {
                        // Navigating or accepting auto-complete suggestions.
                        Selector wordsHost = (Selector)textBox.GetValue(WordAutoCompleteWordsHostProperty);
                        if (e.Key == Key.Up)
                        {
                            // Up or wrap around to the last suggestion.
                            wordsHost.Items.MoveCurrentToPrevious();
                            if (wordsHost.Items.IsCurrentBeforeFirst)
                                wordsHost.Items.MoveCurrentToLast();
                            e.Handled = true;
                        }
                        else if (e.Key == Key.Down)
                        {
                            // Down or wrap around to the first suggestion.
                            wordsHost.Items.MoveCurrentToNext();
                            if (wordsHost.Items.IsCurrentAfterLast)
                                wordsHost.Items.MoveCurrentToFirst();
                            e.Handled = true;
                        }
                        else if (e.Key == Key.Tab || e.Key == Key.Enter || e.Key == Key.Return)
                        {
                            // Accept the current suggestion... rebuild the text with the completed entry.
                            if (wordsHost.Items.CurrentItem != null)
                            {
                                string text = textBox.Text;
                                string selection = wordsHost.Items.CurrentItem.ToString();

                                textBox.Text = selection;
                                textBox.CaretIndex = textBox.Text.Length;

                                e.Handled = true;
                            }
                            popup.IsOpen = false;
                        }
                    }
                }
            }
            catch { }

        }

        private static void GetPreviousAndNextSeparatorOffsets(string text, char[] separators, int caretIndex, out int previousSeparatorOffset, out int nextSeparatorOffset)
        {
            previousSeparatorOffset = -1;
            nextSeparatorOffset = text.Length;

            try
            {
               
                if (caretIndex > 0)
                    previousSeparatorOffset = text.LastIndexOfAny(separators, caretIndex - 1, caretIndex);

                
                if (caretIndex < text.Length)
                {
                    nextSeparatorOffset = text.IndexOfAny(separators, caretIndex);
                    if (nextSeparatorOffset < 0)
                        nextSeparatorOffset = text.Length;
                }
            }
            catch { }
        }

        private static string GetTextSearchText(Selector wordsHost, object item)
        {
            //TODO: ideally we want something better than just using the ToString() representation of items.
            try
            {
                if (item != null)
                    return item.ToString();
                else
                    return string.Empty;
            }
            catch {  return string.Empty;}
        }
        #endregion
    }
}
