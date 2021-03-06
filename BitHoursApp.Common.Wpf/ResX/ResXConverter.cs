﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using BitHoursApp.Common.Resources;

namespace BitHoursApp.Common.Wpf.ResX
{   
    internal class ResXConverter : IValueConverter, IMultiValueConverter
    { 
        public IResXKeyProvider KeyProvider { get; set; }
     
        public ResXExtension ResXExtension { get; set; }

        public ResXParamList Parameters { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string key = KeyProvider.ProvideKey(new[] {value});
            var cultureInfo = value as CultureInfo;
            object localizedObject = CommonResourceManager.Instance.GetResourceObject(key, cultureInfo);
            return localizedObject ?? ResXExtension.GetDefaultValue(key);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #region IMultiValueConverter Members

        /// <summary>
        /// Получает по составному ключу локализованную строку (в общем случае - объект)
        /// Сначала через <see cref="KeyProvider"/> вычисляется составной ключ.
        /// По этому ключу достается локализованный объект.
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Check.Require(Parameters.Count + 1 == values.Length, string.Format("Vi: ResXConverter - parameters count error. Parameters.Count = {0}, values.Count = {1}", Parameters.Count, values.Length));
            // values представляют из себя смешанную в кучу коллекцию параметров для форматированной строки
            // и коллекцию частей для составного ключа. Еще и культура где-то там затесалась последней.
            // Как Золушка отделяем их по разным кучкам.
            var resXKeyParts = new List<object>();
            var resXParams = new List<object>();
            for (int i = 0; i < Parameters.Count; i++)
            {
                object value = values[i];
                BindingBase param = Parameters[i];
                if (param is ResXKeyPart)
                {
                    if (value == DependencyProperty.UnsetValue && param.FallbackValue != null)
                        value = param.FallbackValue;
                    if (value != null)
                        resXKeyParts.Add(value);
                }
                else if (param is ResXParam)
                    resXParams.Add(value);
            }
            CultureInfo cultureInfo = values[values.Length - 1] as CultureInfo;

            string key = KeyProvider.ProvideKey(resXKeyParts);
            object localizedObject = CommonResourceManager.Instance.GetResourceObject(key, cultureInfo) ??
                                     CommonResourceManager.Instance.GetResourceObject(ResXExtension.Key, cultureInfo);
            if (localizedObject == null)
            {
                localizedObject = ResXExtension.GetDefaultValue(key);
            }
            else
            {
                try
                {
                    localizedObject = string.Format(localizedObject.ToString(), resXParams.Cast<object>().ToArray());
                }
                catch
                {
                    //TODO: Переделать в нормальные проверки "строка-не строка", почему вдруг не получилось отформатировать и т.п., дабы будущим поколениям стало легче жить
                }
            }
            return localizedObject;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[0];
        }

        #endregion
    }
}