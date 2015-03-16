using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BitHoursApp.Controls
{
    /// <summary> 
    /// Sets the focus to the TargetControl property as the 
    /// form or control is loaded. 
    /// </summary> 
    public class SetFocusElement : FrameworkElement
    {
        /// <summary> 
        /// Gets or sets the target UI control. 
        /// </summary> 
        public static readonly DependencyProperty TargetControlProperty =
          DependencyProperty.Register("TargetControl", typeof(IInputElement),
          typeof(SetFocusElement), new PropertyMetadata((o, e) => InnerSetFocus(o as SetFocusElement)));

        /// <summary> 
        /// Gets or sets the target UI control. 
        /// </summary> 
        [Category("Common")]
        public IInputElement TargetControl
        {
            get
            {
                return (IInputElement)GetValue(TargetControlProperty);
            }
            set { SetValue(TargetControlProperty, value); }
        }

        public SetFocusElement()
        {
        }

        public SetFocusElement(Panel panel)
        {
            TargetControl = GetDefaultFocusElement(panel);
        }

        private static void InnerSetFocus(SetFocusElement element)
        {
            if (element == null)
                return;

            var focusedElement = element.TargetControl as FrameworkElement;

            if (focusedElement == null || !focusedElement.Focusable)
                focusedElement = GetDefaultFocusElement(element.Parent);

            if (focusedElement == null || focusedElement.IsLoaded) return;

            RoutedEventHandler deferredFocusHookup = null;

            deferredFocusHookup = delegate
            {
                element.Loaded -= deferredFocusHookup;
                if (focusedElement.IsVisible == false) return;
                if (!focusedElement.IsKeyboardFocused)
                    focusedElement.Focus();
            };

            element.Loaded += deferredFocusHookup;
        }

        public static FrameworkElement GetDefaultFocusElement(object container)
        {
            var panelContainer = container as Panel;
            return panelContainer != null ? panelContainer.Children.Cast<FrameworkElement>().FirstOrDefault(e => e.Focusable) : null;
        }
    }
}
