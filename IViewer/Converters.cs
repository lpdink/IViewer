﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace IViewer {
  public class MatchingIntToBooleanConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return value != null && parameter as string == ((long)value).ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      if (!(value is bool b)) {
        return -1;
      }

      var i = System.Convert.ToInt64((parameter ?? "0") as string);

      return b ? System.Convert.ChangeType(i, targetType) : 0;
    }
  }

  public class StringLongConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return $"{(long)(value ?? 0)}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return value is string s && long.TryParse(s, out var l) ? l : 0;
    }
  }

  public class StringDoubleConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return $"{(double)(value ?? 0)}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return value is string s && double.TryParse(s, out var l) ? l : 0;
    }
  }

  public class StringColorConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return ColorConverter.ConvertFromString((value ?? "#00000000") as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      var r = value is Color s ? s.ToString() : "#00000000";
      return r;
    }
  }

  public class LongDescriptionPair {
    public long Value { get; set; }
    public string Description { get; set; }
  }

  public static class EnumHelper {
    public static string ResourceDescription(this Enum value) {
      string fallback = null;
      var attributes = value.GetType().GetField(value.ToString())
        .GetCustomAttributes(typeof(DescriptionAttribute), false);
      if (attributes.Any()) {
        fallback = (attributes.First() as DescriptionAttribute)?.Description;
        var result = Settings.Resource(fallback);
        if (!string.IsNullOrWhiteSpace(result)) {
          return result;
        }
      }

      if (string.IsNullOrWhiteSpace(fallback)) {
        fallback = value.ToString();
      }

      TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
      return ti.ToTitleCase(ti.ToLower(fallback.Replace("_", " ")));
    }

    public static IEnumerable<LongDescriptionPair> GetAllValuesAndDescriptions(Type t) {
      if (!t.IsEnum) {
        throw new ArgumentException($"{nameof(t)} must be an enum type");
      }

      return Enum.GetValues(t)
        .Cast<Enum>()
        .Select(e => new LongDescriptionPair {Value = Convert.ToInt64(e), Description = e.ResourceDescription()})
        .ToList();
    }
  }

  [ValueConversion(typeof(Type), typeof(IEnumerable<LongDescriptionPair>))]
  public class EnumToCollectionConverter : MarkupExtension {

    public Type EnumType { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return EnumHelper.GetAllValuesAndDescriptions(EnumType);
    }
  }
}