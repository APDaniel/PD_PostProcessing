﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PD_ScriptTemplate.Converter
{
    public class OperationStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringValue = value as string;

            // Check if the string value is "ring"
            if (stringValue == "ring")
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
