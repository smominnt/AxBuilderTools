using System.Windows;

namespace AxBuilder.HelperProperties
{
    public static class ScrollViewerProperties
    {
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.RegisterAttached("Scale", typeof(double), typeof(ScrollViewerProperties), new PropertyMetadata(1.0));

        public static double GetScale(DependencyObject obj)
        {
            return (double)obj.GetValue(ScaleProperty);
        }

        public static void SetScale(DependencyObject obj, double value)
        {
            obj.SetValue(ScaleProperty, value);
        }
    }
}