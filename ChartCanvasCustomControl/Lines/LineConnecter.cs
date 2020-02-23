using ChartCanvasNamespace.Entities;
using ChartCanvasNamespace.Thumbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using WPFHelpers.CancelActions;

namespace ChartCanvasNamespace.Lines
{
    public class LineConnecter : aToggleButtonThumbThatShowsOnMouseOver, IChartLineConnecterThumb
    {
        public LineConnecter(LineConnection line, int connecterIndex)
        {
            Width = Size;
            Height = Size;
            _Connection = line;
            ConnecterIndex = connecterIndex;
            Panel.SetZIndex(this, Properties.Settings.Default.ZIndex_LineConnecter);
            ToolTip = Properties.ToolTips.Default.ToolTips_LineConnecter;
        }

        internal LineConnection _Connection;

        public ConnecterThumbTypesEnum ConnecterType => ConnecterThumbTypesEnum.Line;
        public static int Size { get { return 10; } }
        public int ConnecterIndex { get; private set; }

        #region connect
        public bool ScriptUnchecked { get; set; }
        protected override void OnChecked(RoutedEventArgs e)
        {
            ChartCustomControl.Instance.ConnecterChecked(this);
        }
        protected override void OnUnchecked(RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            if (ScriptUnchecked)
            {
                ScriptUnchecked = false;
                return;
            }

            CancelCurrentActionDelegate = null;
            CancellableActionsHandlerClass.Instance.CancellableActionFinished(this);
        }
        public void SetNewLineCancellableAction()
        {
            CancelCurrentActionDelegate = CancelNewLine;
        }
        #endregion

        #region cancel, undo/redo
        public string CancellableId => $"ChCuCo_LineConnecter_{Guid.NewGuid().ToString()}";
        public Action CancelCurrentActionDelegate { get; private set; }
        public Func<Task> AsyncCancelCurrentActionDelegate => null;

        private void CancelNewLine()
        {
            ScriptUnchecked = true;
            this.IsChecked = false;
            CancelCurrentActionDelegate = null;
        }
        public bool Equals(IObjectWithCancellableAction other)
        {
            var obj = other as EntityConnectingThumb;
            if (obj == null)
                return false;
            return this.Equals(obj as UserControl);
        }
        #endregion
    }
}
