using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StoryChart.Model
{
    public static class NodeIdsHandler
    {
        internal static int _LastId;
        private static Dictionary<Node, string> _ProgIds = new Dictionary<Node, string>();
        private static HashSet<string> _UserIds = new HashSet<string>();

        public static Dictionary<string, Node> _NodesByIds = new Dictionary<string, Node>();

        public static void ProgrammaticallySetId(string id, Node node)
        {
            _ProgIds.Add(node, id);
        }
        public static bool IdIsOk(string id, Node node)
        {
            if (_ProgIds.ContainsKey(node) && _ProgIds[node].Equals(id))
            {
                _ProgIds.Remove(node);
                _NodesByIds.Add(id, node);
                return true;
            }

            if (_UserIds.Contains(id))
            {
                MessageBox.Show(StoryChartEntitiesTemplatesPlugin.Properties.Settings.Default.IdEnUso);
                return false;
            }
            else
            {
                _UserIds.Add(id);
                return true;
            }
        }
        public static void ResetIds()
        {
            _LastId = 0;
            _ProgIds.Clear();
            _UserIds.Clear();
            _NodesByIds.Clear();
        }
    }
}
