using System;
using System.Globalization;
using System.Windows.Data;

namespace TradeHubAnalyst.Libraries
{
    public class InvisibleIfZeroValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            decimal val = (decimal)value;
            string format = "#,##0.##";
            if (val == 0)
            {
                format = "#,##.##";
            }

            return val.ToString(format, CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}