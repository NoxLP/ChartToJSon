using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StoryChart
{
    public static class ExceptionExtensions
    {
        public static void ShowException(this Exception e, string additionalMessagePrefix = "")
        {
            var msg = $@"{additionalMessagePrefix}
                Error: 
{e.Message} ;

                Trace: 
{e.StackTrace} ;";

            Exception innerEx = e.InnerException;

            while (innerEx != null)
            {
                msg = msg + $@"
                
                InnerException: 
{(e.InnerException != null ? innerEx.Message : "")} ; 
                
                InnerException Trace:
{(e.InnerException != null ? innerEx.StackTrace : "")} ;";

                innerEx = innerEx.InnerException;
            }

            System.Windows.MessageBox.Show(msg);
        }
    }
    public static class CanvasExtensions
    {
        public static void AddChildInCoordinates(this Canvas source, UIElement control, double fromLeft, double fromTop)
        {
            source.Children.Add(control);
            Canvas.SetLeft(control, fromLeft);
            Canvas.SetTop(control, fromTop);
        }
        public static void MoveChildToCoordinates(this Canvas source, UIElement control, double fromLeft, double fromTop)
        {
            Canvas.SetLeft(control, fromLeft);
            Canvas.SetTop(control, fromTop);
        }
    }
    public static class FrameworkElementExtensions
    {
        public static Rect GetBoundingBox(this FrameworkElement element, Window containerWindow)
        {
            GeneralTransform transform = element.TransformToAncestor(containerWindow);
            Point topLeft = transform.Transform(new Point(0, 0));
            Point bottomRight = transform.Transform(new Point(element.ActualWidth, element.ActualHeight));
            return new Rect(topLeft, bottomRight);
        }
    }
    public static class DictionaryExtensions
    {
        public static void AddListOrItemToDictionary<TKey,RListType>(this Dictionary<TKey, List<RListType>> source, TKey key, RListType value)
        {
            if(!source.ContainsKey(key))
            {
                source.Add(key, new List<RListType>() { value });
            }
            else
            {
                source[key].Add(value);
            }
        }
        public static void RemoveListOrItemFromdictionary<TKey, RListType>(this Dictionary<TKey, List<RListType>> source, TKey key, RListType value)
        {
            if (!source.ContainsKey(key))
                return;

            if (!source[key].Contains(value))
                return;

            if (source[key].Count < 2)
                source.Remove(key);
            else
                source[key].Remove(value);
        }
    }
}
