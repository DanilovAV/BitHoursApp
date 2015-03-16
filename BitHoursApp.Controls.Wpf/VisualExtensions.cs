using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BitHoursApp.Controls
{
    /// <summary>
    /// Exposes extension methods for <see cref="Visual"/> and <see cref="Visual3D"/> objects
    /// </summary>
    public static class VisualExtensions
    {
        public static IEnumerable<T> Children<T>(this DependencyObject root) where T : UIElement
        {
            T t = root as T;
            if (t != null)
                yield return t;

            if (root == null) yield break;
            var count = VisualTreeHelper.GetChildrenCount(root);

            for (var idx = 0; idx < count; idx++)
            {
                foreach (var child in Children<T>(VisualTreeHelper.GetChild(root, idx)))
                {
                    yield return child;
                }
            }
        }

        public static DependencyObject GetTopLevelControl(this DependencyObject control)
        {
            DependencyObject tmp = control;
            DependencyObject parent = null;
            while ((tmp = VisualTreeHelper.GetParent(tmp)) != null)
            {
                parent = tmp;
            }
            return parent;
        }

        public static T FindAncestor<T>(this DependencyObject obj, Predicate<T> predicate = null) where T : class
        {
            var parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                T result = parent as T;
                if (result != null)
                {
                    if (predicate == null || predicate(result))
                        return result;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        public static T FindLogicalAncestor<T>(this DependencyObject obj) where T : DependencyObject
        {
            var parent = LogicalTreeHelper.GetParent(obj);
            while (parent != null && !(parent is T))
            {
                parent = LogicalTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        public static T FindTopLogicalAncestor<T>(this DependencyObject d) where T : DependencyObject
        {
            var obj = d;
            while (true)
            {
                T parentObject = obj.FindLogicalAncestor<T>();
                if (parentObject == null)
                    return obj == d ? null : obj as T;
                obj = parentObject;
            }
        }

        public static bool HasParent(this DependencyObject obj, IEnumerable<DependencyObject> parentList)
        {
            if (parentList == null) return false;
            var parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parentList.Contains(parent)) return true;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return false;
        }

        public static bool HasParent(this DependencyObject obj, DependencyObject parent)
        {
            return HasParent(obj, new[] { parent });
        }

        public static TChildItem FindVisualChild<TChildItem>(this DependencyObject parent)
            where TChildItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is TChildItem)
                    return (TChildItem)child;
                TChildItem childOfChild = FindVisualChild<TChildItem>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        public static DependencyObject GetTemplatedParent(this DependencyObject obj)
        {
            FrameworkElement fe = obj as FrameworkElement;
            if (fe != null)
                return fe.TemplatedParent;
            FrameworkContentElement fce = obj as FrameworkContentElement;
            return fce != null ? fce.TemplatedParent : null;
        }
    }
}