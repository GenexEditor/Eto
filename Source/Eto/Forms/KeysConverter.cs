﻿using System;
using System.ComponentModel;
using System.Linq;

namespace Eto.Forms
{
	class KeysConverter : TypeConverter
	{
		public string[] Separators { get; set; }

		public KeysConverter()
		{
			Separators = new [] { "+", ",", "|" };
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			var text = value as string;
			if (text != null)
			{
				var keys = text.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
				Keys result = Keys.None;
				foreach (var key in keys)
				{
					if (key == "CommonModifier")
						result |= Application.Instance.CommonModifier;
					else if (key == "AlternateModifier")
						result |= Application.Instance.AlternateModifier;
					else
						result |= (Keys)Enum.Parse(typeof(Keys), key);
				}
				return result;
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string) && value is Keys)
			{
				return ((Keys)value).ToShortcutString(Separators.First());
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}

