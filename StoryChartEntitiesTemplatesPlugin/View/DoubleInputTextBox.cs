using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NET471WpfUserControlsLibrary
{
    public class DoubleInputTextBox : aInputRestrictedTextBox
    {
        public DoubleInputTextBox()
        {
            _DefaultText = 0.ToString(_Format);
            Text = _DefaultText;
            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
        }

        private string _Format = "N";
        private bool _DirectInputChangedByScript;

        public int DecimalDigits
        {
            get { return (int)GetValue(DecimalDigitsProperty); }
            set { SetValue(DecimalDigitsProperty, value); }
        }
        public static readonly DependencyProperty DecimalDigitsProperty =
            DependencyProperty.Register("DecimalDigits", typeof(int), typeof(DoubleInputTextBox), 
                new PropertyMetadata(0, DecimalDigits_PropertyChanged));
        private static void DecimalDigits_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as DoubleInputTextBox).DecimalDigitsChanged((int)e.NewValue);
        }
        private void DecimalDigitsChanged(int newValue)
        {
            _Format = $"N{(newValue == 0 ? "" : newValue.ToString())}";
        }

        public double DoubleValue
        {
            get { return (double)GetValue(DoubleValueProperty); }
            set { SetValue(DoubleValueProperty, value); }
        }
        public static readonly DependencyProperty DoubleValueProperty =
            DependencyProperty.Register("DoubleValue", typeof(double), typeof(DoubleInputTextBox), new PropertyMetadata(0d, DoubleValueChanged));
        private static void DoubleValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as DoubleInputTextBox).DirectDoubleInput((double)e.NewValue);
        }
        private void DirectDoubleInput(double value)
        {
            if(_DirectInputChangedByScript)
            {
                _DirectInputChangedByScript = false;
                return;
            }
            Text = value.ToString();
        }

        protected override void aInputRestrictedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (GetIfUserChangedEvent())
                return;
            if (GetIfTextIsNullOrEmptyAndSetTextToDefault(ref e, text => string.IsNullOrEmpty(text) || text == "0"))
                return;

            double test;

            if (double.TryParse(Text, NumberStyles.Any, CultureInfo.InvariantCulture, out test))
            {
                _FormattedString = test.ToString(_Format, CultureInfo.CurrentCulture);
                _CleanString = test.ToString(CultureInfo.InvariantCulture);
                _DirectInputChangedByScript = true;
                DoubleValue = test;
            }
            else
            {
                IncorrectInput(ref e);
            }
        }
    }
}
