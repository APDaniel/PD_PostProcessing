﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace PD_ScriptTemplate.Converter
{
    public class LookBackConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Point3D(0, 0, 0) - (Point3D)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
