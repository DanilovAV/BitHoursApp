using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BitHoursApp.Controls.Wpf
{
    public class Activity : ContentControl
    {
        private Stopwatch stopwatch = new Stopwatch();
        private readonly DispatcherTimer updateDurationTimer;

        static Activity()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Activity), new FrameworkPropertyMetadata(typeof(Activity)));
        }

        public Activity()
        {
            Focusable = false;
            updateDurationTimer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher.CurrentDispatcher)
            {
                Interval = new TimeSpan(0, 0, 0, 1)
            };
        }

        private static WeakReference activeViews;
        public static IEnumerable<object> ActiveViews
        {
            get { return activeViews != null && activeViews.IsAlive ? activeViews.Target as IEnumerable<object> : null; }
            set { activeViews = value == null ? null : new WeakReference(value); }
        }

        public static DependencyProperty IsActiveProperty = DependencyProperty.Register(
            "IsActive",
            typeof(bool?),
            typeof(Activity),
            new PropertyMetadata(null, IsActiveChanged));

        public bool? IsActive
        {
            get { return (bool?)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        private static void IsActiveChanged(DependencyObject s, DependencyPropertyChangedEventArgs args)
        {
            Activity activity = s as Activity;
            if (activity != null)
                activity.IsActiveChanged(args);
        }

        private void IsActiveChanged(DependencyPropertyChangedEventArgs e)
        {
            bool? newValue = (bool?)e.NewValue;
            bool isActive = newValue.HasValue && newValue.Value;

            if (updateDurationTimer != null)
                WeakEventManager<DispatcherTimer, EventArgs>.RemoveHandler(updateDurationTimer, "Tick", TimerTick);

            if (isActive)
            {
                stopwatch = Stopwatch.StartNew();
                UpdateDuration();
                WeakEventManager<DispatcherTimer, EventArgs>.AddHandler(updateDurationTimer, "Tick", TimerTick);
                updateDurationTimer.Start();
            }
            else
            {
                stopwatch.Stop();
                updateDurationTimer.Stop();
            }

            RestoreFocus(this);
            RefreshChildren();
        }

        public static readonly DependencyPropertyKey DurationPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "Duration",
                typeof(TimeSpan),
                typeof(Activity),
                new PropertyMetadata(default(TimeSpan)));

        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationPropertyKey.DependencyProperty); }
            private set { SetValue(DurationPropertyKey, value); }
        }

        public static DependencyProperty FocusTargetProperty = DependencyProperty.Register(
          "FocusTarget",
          typeof(IInputElement),
          typeof(Activity));

        public IInputElement FocusTarget
        {
            get
            {
                return (IInputElement)GetValue(FocusTargetProperty);
            }
            set { SetValue(FocusTargetProperty, value); }
        }

        public static DependencyProperty LightActivityBrushProperty = DependencyProperty.Register(
            "LightActivityBrush",
            typeof(Brush),
            typeof(Activity),
            null);

        public Brush LightActivityBrush
        {
            get { return GetValue(LightActivityBrushProperty) as Brush; }
            set { SetValue(LightActivityBrushProperty, value); }
        }

        public static DependencyProperty DarkActivityBrushProperty = DependencyProperty.Register(
            "DarkActivityBrush",
            typeof(Brush),
            typeof(Activity),
            null);

        public Brush DarkActivityBrush
        {
            get { return GetValue(DarkActivityBrushProperty) as Brush; }
            set { SetValue(DarkActivityBrushProperty, value); }
        }

        public static DependencyProperty ActiveTextProperty = DependencyProperty.Register(
            "ActiveText",
            typeof(string),
            typeof(Activity),
            null);

        public string ActiveText
        {
            get { return GetValue(ActiveTextProperty) as string; }
            set { SetValue(ActiveTextProperty, value); }
        }

        public static readonly DependencyPropertyKey IsParentActivePropertyKey = DependencyProperty.RegisterReadOnly(
            "IsParentActive",
            typeof(bool),
            typeof(Activity),
            new UIPropertyMetadata(false, (s, args) =>
                {
                    var instance = s as Activity;
                    if (instance != null)
                        instance.RefreshChildren();
                }));

        public static readonly DependencyProperty IsParentActiveProperty = IsParentActivePropertyKey.DependencyProperty;

        public bool IsParentActive
        {
            get { return (bool)GetValue(IsParentActiveProperty); }
            private set { SetValue(IsParentActivePropertyKey, value); }
        }

        #region [ Parent ]

        private readonly List<WeakReference> childActivities = new List<WeakReference>();

        private WeakReference parentActivity;
        protected Activity ParentActivity
        {
            get { return parentActivity != null ? (Activity)parentActivity.Target : null; }
            set
            {
                var oldParent = ParentActivity;
                if (oldParent != null && !ReferenceEquals(oldParent, value))
                    oldParent.RemoveChildActivity(this);
                parentActivity = value != null ? new WeakReference(value) : null;
                if (value != null)
                    value.AddChildActivity(this);
                RefreshParentState(value);
            }
        }

        private void AddChildActivity(Activity childActivity)
        {
            if (childActivities.Any(e => ReferenceEquals(e.Target, childActivity)))
                return;
            childActivities.Add(new WeakReference(childActivity));
        }

        private void RemoveChildActivity(Activity childActivity)
        {
            for (int i = childActivities.Count - 1; i >= 0; i--)
                if (ReferenceEquals(childActivities[i].Target, childActivity))
                {
                    childActivities.RemoveAt(i);
                    return;
                }
        }

        private void RefreshChildren()
        {
            for (int i = childActivities.Count - 1; i >= 0; i--)
            {
                var child = (Activity)childActivities[i].Target;
                if (child == null)
                    childActivities.RemoveAt(i);
                else
                    child.RefreshParentState(this);
            }
        }

        private void RefreshParentState(Activity parent)
        {
            IsParentActive = parent != null && ((parent.IsActive.HasValue && parent.IsActive.Value) || parent.IsParentActive);
        }

        private void SubscribeParent()
        {
            ParentActivity = this.FindAncestor<Activity>();
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SubscribeParent();
        }

        private static void RestoreFocus(DependencyObject d)
        {
            Activity activity = d as Activity;
            if (activity == null || (activity.IsActive.HasValue && activity.IsActive.Value)) return;
            if (ActiveViews != null)
            {
                var activeViewList = ActiveViews.OfType<DependencyObject>();
                bool hasParent = activity.HasParent(activeViewList);
                if (!hasParent)
                {
                    foreach (var activeView in activeViewList)
                    {
                        hasParent = activeView.HasParent(activity);
                        if (hasParent) break;
                    }
                }
                if (!hasParent) return;
            }
            var adorney = activity.Content as AdornerDecorator;
            if (adorney == null) return;

            FrameworkElement focusTarget = GetFocusTarget(activity);
            if (focusTarget == null)
                return;

            focusTarget.Dispatcher.BeginInvoke((Action)delegate
            {

                if (!focusTarget.IsKeyboardFocusWithin)
                    focusTarget.Focus();
            },
            DispatcherPriority.Background);
        }

        private static FrameworkElement GetFocusTarget(Activity activity)
        {
            FrameworkElement focusTarget = activity.FocusTarget as FrameworkElement;
            if (focusTarget == null)
            {
                AdornerDecorator adorney = activity.Content as AdornerDecorator;
                if (adorney != null)
                {
                    DependencyObject control = adorney.Child;
                    SetFocusElement setFocus = control.Children<SetFocusElement>().FirstOrDefault();
                    if (setFocus != null)
                    {
                        focusTarget = setFocus.TargetControl as FrameworkElement;
                        if (focusTarget != null && focusTarget.Parent != null)
                        {
                            var scope = FocusManager.GetFocusScope(focusTarget.Parent);
                            var lastTargetElement = scope != null ? FocusManager.GetFocusedElement(scope) as FrameworkElement : null;
                            if (lastTargetElement != null)
                                focusTarget = lastTargetElement;
                        }
                    }
                }
            }
            return focusTarget;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            UpdateDuration();
        }

        private void UpdateDuration()
        {
            Duration = stopwatch.Elapsed;
        }
    }
}
