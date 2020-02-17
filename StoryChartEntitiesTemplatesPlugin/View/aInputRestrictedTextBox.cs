using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NET471WpfUserControlsLibrary
{
    public abstract class aInputRestrictedTextBox : TextBox
    {
        public aInputRestrictedTextBox()
        {
            this.GotFocus += aInputRestrictedTextBox_GotFocus;
            this.LostFocus += aInputRestrictedTextBox_LostFocus;
            this.TextChanged += aInputRestrictedTextBox_TextChanged;
            this.KeyDown += aInputRestrictedTextBox_KeyDown;
        }

        protected string _DefaultText = "";
        protected string _CleanString = "";
        protected string _FormattedString = "";
        protected bool _DoChangedEvent = false;

        #region helpers
        protected virtual bool GetIfUserChangedEvent()
        {
            if (!_DoChangedEvent)
            {
                _DoChangedEvent = true;
                return true;
            }

            return false;
        }
        protected virtual bool GetIfTextIsNullOrEmptyAndSetTextToDefault(ref TextChangedEventArgs e, Predicate<string> replaceConditionTextIsNullOrEmpty = null)
        {
            if (replaceConditionTextIsNullOrEmpty == null && string.IsNullOrEmpty(Text))
            {
                _FormattedString = "";
                _CleanString = "";
                e.Handled = true;
                return true;
            }

            if (replaceConditionTextIsNullOrEmpty != null && replaceConditionTextIsNullOrEmpty(Text))
            {
                _FormattedString = "";
                _CleanString = "";
                e.Handled = true;
                return true;
            }

            return false;
        }
        protected virtual void IncorrectInput(ref TextChangedEventArgs e)
        {
            _DoChangedEvent = false;
            Text = _CleanString;
            SelectionStart = _CleanString.Length;
            e.Handled = true;
        }
        #endregion

        protected abstract void aInputRestrictedTextBox_TextChanged(object sender, TextChangedEventArgs e);
        protected virtual void aInputRestrictedTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _DoChangedEvent = false;
                Text = _FormattedString;
                FocusManager.SetFocusedElement(Application.Current.MainWindow, Application.Current.MainWindow as IInputElement);
            }
        }
        protected virtual void aInputRestrictedTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _DoChangedEvent = false;
            if (!string.IsNullOrEmpty(_FormattedString))
                Text = _FormattedString;
            else
                Text = _DefaultText;
        }
        protected virtual void aInputRestrictedTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _DoChangedEvent = false;
            Text = _CleanString;
        }
    }
}
