using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=334336

namespace ComicShampoo.Control
{
    public class RangeBaseValueChangedEventArgs
    {
        public object OriginalSource { get; private set; }
        public double NewValue { get; private set; }
        public double OldValue { get; private set; }

        public RangeBaseValueChangedEventArgs(object source, double oldv, double newv)
        {
            OriginalSource = source;
            OldValue = oldv;
            NewValue = newv;
        }
    }

    public sealed partial class BrightnessSlider : UserControl, INotifyPropertyChanged
    {
        private double minimum = 0;
        public double Minimum
        {
            get { return minimum; }
            set
            {
                if (value == minimum)
                {
                    return;
                }

                minimum = value;
                if (minimum > maximum)
                {
                    maximum = minimum;
                }
                if (Value < minimum)
                {
                    Value = minimum;
                }

                NotifyPropertyChanged();
            }
        }

        private double maximum = 0;
        public double Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                if (value == maximum)
                {
                    return;
                }

                maximum = value;
                if (maximum < minimum)
                {
                    minimum = maximum;
                }
                if (Value > maximum)
                {
                    Value = maximum;
                }

                NotifyPropertyChanged();
            }
        }

        private double changing = 0;
        public double Changing
        {
            get
            {
                return changing;
            }
            private set
            {
                if (value == changing)
                {
                    return;
                }

                double old = changing;
                changing = value;
                changing = Correction(changing);

                NotifyPropertyChanged();
                InvokeValueChanging(old, changing);
            }
        }

        private double _value = 0;
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value == value)
                {
                    return;
                }

                double oldValue = _value;
                if (value < minimum)
                {
                    _value = minimum;
                }
                else if (value > maximum)
                {
                    _value = maximum;
                }
                else
                {
                    _value = value;
                }
                _value = Correction(_value);
                changing = _value;

                InvokeValueChanged(oldValue, _value);
                NotifyPropertyChanged();
            }
        }

        private double smallChange = 1;
        public double SmallChange
        {
            get
            {
                return smallChange;
            }
            set
            {
                if (value == smallChange)
                {
                    return;
                }

                smallChange = value;

                NotifyPropertyChanged();
            }
        }

        private double ValueRange
        {
            get
            {
                return Maximum - Minimum;
            }
        }

        private double Ratio
        {
            get
            {
                if (maximum == minimum)
                {
                    return 1;
                }

                double range = maximum - minimum;
                double subrange = _value - minimum;
                return subrange / range;
            }
        }

        private bool isDirectionReversed = false;
        public bool IsDirectionReversed
        {
            get
            {
                return isDirectionReversed;
            }
            set
            {
                if (value == isDirectionReversed)
                {
                    return;
                }

                isDirectionReversed = value;

                NotifyPropertyChanged();
            }
        }

        private EventHandler<RangeBaseValueChangedEventArgs> valueChanged;
        public event EventHandler<RangeBaseValueChangedEventArgs> ValueChanged
        {
            add
            {
                valueChanged += value;
            }
            remove
            {
                valueChanged -= value;
            }
        }

        private EventHandler<RangeBaseValueChangedEventArgs> valueChanging;
        public event EventHandler<RangeBaseValueChangedEventArgs> ValueChanging
        {
            add
            {
                valueChanging += value;
            }
            remove
            {
                valueChanging -= value;
            }
        }

        public BrightnessSlider()
        {
            this.InitializeComponent();

            SizeChanged += CanvasSlider_SizeChanged;
            Range.PointerPressed += Range_PointerPressed;
            Range.PointerMoved += Range_PointerMoved;
            Range.PointerReleased += Range_PointerReleased;
            PropertyChanged += CanvasSlider_PropertyChanged;
        }

        private void CanvasSlider_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InvalidateRange();
        }

        private double Correction(double v)
        {
            if (0 == SmallChange || true == double.IsNaN(SmallChange))
            {
                return v;
            }

            double c = v / SmallChange;
            double quotient = SmallChange * (int)c;
            double remain = v - quotient;
            if (remain > SmallChange * 0.5)
            {
                quotient += SmallChange;
            }

            return quotient;
        }

        private double GetPointerRatio(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint(sender as UIElement);
            double ratio = pp.Position.X / Range.ActualWidth;
            if (true == IsDirectionReversed)
            {
                ratio = 1 - ratio;
            }

            if (ratio < 0) ratio = 0;
            else if (ratio > 1) ratio = 1;

            return ratio;
        }

        public void InvalidateRange()
        {
            if (null != Range && null != SubRange)
            {
                SubRange.Width = Range.ActualWidth * ((Changing - Minimum) / ValueRange);

                double margin = SubRange.Width - 0.5;
                double invMargin = Range.ActualWidth - SubRange.Width - 0.5;

                if (false == IsDirectionReversed)
                {
                    Marker.Margin = new Thickness(margin, 3, invMargin, 3);
                }
                else
                {
                    Marker.Margin = new Thickness(invMargin, 3, margin, 3);
                }
            }
        }

        bool isPressed = false;
        private void Range_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            isPressed = true;
            RangeValue.Visibility = Visibility.Visible;
            Range.CapturePointer(e.Pointer);

            double ratio = GetPointerRatio(sender, e);

            Changing = Minimum + ValueRange * ratio;
            RangeValue.Text = Changing.ToString();
        }

        private void Range_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (false == isPressed)
            {
                return;
            }

            double ratio = GetPointerRatio(sender, e);

            Changing = Minimum + ValueRange * ratio;
            RangeValue.Text = Changing.ToString();
        }

        private void Range_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            isPressed = false;
            RangeValue.Visibility = Visibility.Collapsed;

            double ratio = GetPointerRatio(sender, e);

            Value = Minimum + ratio * ValueRange;
            RangeValue.Text = Value.ToString();
        }

        private void CanvasSlider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Value":
                    InvalidateRange();
                    break;

                case "Changing":
                    InvalidateRange();
                    break;

                case "IsDirectionReversed":
                    if (false == isDirectionReversed)
                    {
                        SubRange.HorizontalAlignment = HorizontalAlignment.Left;
                    }
                    else
                    {
                        SubRange.HorizontalAlignment = HorizontalAlignment.Right;
                    }

                    InvalidateRange();
                    break;
            }
        }

        private void InvokeValueChanged(double oldValue, double newValue)
        {
            valueChanged?.Invoke(this, new RangeBaseValueChangedEventArgs(this.DataContext, oldValue, newValue));
        }

        private void InvokeValueChanging(double oldValue, double newValue)
        {
            valueChanging?.Invoke(this, new RangeBaseValueChangedEventArgs(this.DataContext, oldValue, newValue));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
