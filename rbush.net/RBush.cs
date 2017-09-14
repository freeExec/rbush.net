using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rbush.net
{
    public interface IBBox
    {
        int MinX { get; set; }
        int MaxX { get; set; }
        int MinY { get; set; }
        int MaxY { get; set; }
    }

    public class BBox : IBBox
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }

        public BBox()
        { }

        public BBox(int[] data)
        {
            if (data.Length == 2)
            {
                MinX = data[0];
                MinY = data[1];
                MaxX = data[0];
                MaxY = data[1];
            } else if (data.Length == 4)
            {
                MinX = data[0];
                MinY = data[1];
                MaxX = data[2];
                MaxY = data[3];
            }
            else throw new ArgumentException("Data length is 2 or 4.");
        }

        public BBox(int x, int y)
        {
            MinX = MaxX = x;
            MinY = MaxY = y;
        }

        public BBox(int minX, int minY, int maxX, int maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public int Area
        {
            get { return (MaxX - MinX) * (MaxY - MinY); }
        }

        public static int EnlargedArea(IBBox a, IBBox b)
        {
            return (Math.Max(b.MaxX, a.MaxX) - Math.Min(b.MinX, a.MinX)) *
                   (Math.Max(b.MaxY, a.MaxY) - Math.Min(b.MinY, a.MinY));
        }

        public int EnlargedArea(IBBox b)
        {
            return (Math.Max(b.MaxX, MaxX) - Math.Min(b.MinX, MinX)) *
                   (Math.Max(b.MaxY, MaxY) - Math.Min(b.MinY, MinY));
        }

        public void Extend(IBBox b)
        {
            MinX = Math.Min(MinX, b.MinX);
            MinY = Math.Min(MinY, b.MinY);
            MaxX = Math.Max(MaxX, b.MaxX);
            MaxY = Math.Max(MaxY, b.MaxY);
        }

        public static BBox Extend(IBBox a, IBBox b)
        {
            int minX = Math.Min(a.MinX, b.MinX);
            int minY = Math.Min(a.MinY, b.MinY);
            int maxX = Math.Max(a.MaxX, b.MaxX);
            int maxY = Math.Max(a.MaxY, b.MaxY);
            return new BBox(minX, minY, maxX, maxY);
        }

        public int Margin
        {
            get { return (MaxX - MinX) + (MaxY - MinY); }
        }

        public void Reset()
        {
            MinX = int.MaxValue;
            MinY = int.MaxValue;
            MaxX = int.MinValue;
            MaxY = int.MinValue;
        }

        public int IntersectionArea(IBBox b)
        {
            int minX = Math.Max(MinX, b.MinX);
            int minY = Math.Max(MinY, b.MinY);
            int maxX = Math.Min(MaxX, b.MaxX);
            int maxY = Math.Min(MaxY, b.MaxY);

            return Math.Max(0, maxX - minX) *
                   Math.Max(0, maxY - minY);
        }

        public virtual bool Intersects(IBBox b)
        {
            return b.MinX <= MaxX &&
                   b.MinY <= MaxY &&
                   b.MaxX >= MinX &&
                   b.MaxY >= MinY;
        }

        public static bool Intersects(IBBox a, IBBox b)
        {
            return b.MinX <= a.MaxX &&
                   b.MinY <= a.MaxY &&
                   b.MaxX >= a.MinX &&
                   b.MaxY >= a.MinY;
        }

        public virtual bool Contains(IBBox a)
        {
            return MinX <= a.MinX &&
                   MinY <= a.MinY &&
                   a.MaxX <= MaxX &&
                   a.MaxY <= MaxY;
        }

        public static bool Contains(IBBox a, IBBox b)
        {
            return a.MinX <= b.MinX &&
                   a.MinY <= b.MinY &&
                   b.MaxX <= a.MaxX &&
                   b.MaxY <= a.MaxY;
        }

        public static int CompareMinX(IBBox a, IBBox b)
        {
            int d = a.MinX - b.MinX;
            if (d < 0) return -1;
            else if (d > 0) return 1;
            else return 0;
        }

        public static int CompareMinY(IBBox a, IBBox b)
        {
            int d = a.MinY - b.MinY;
            if (d < 0) return -1;
            else if (d > 0) return 1;
            else return 0;
        }
    }

    public class RBush
    {
        public class Node : BBox
        {
            public bool Leaf;
            public int Height;
            public List<Node> Children;

            public readonly IBBox ExternalObject;

            public Node(int[] data)
                : base(data)
            { }

            public Node(int minX, int minY, int maxX, int maxY)
                : base(minX, minY, maxX, maxY)
            { }

            public Node(IBBox externalObject)
                : base(externalObject.MinX, externalObject.MinY, externalObject.MaxX, externalObject.MaxY)
            {
                ExternalObject = externalObject;
            }

            public static Node CreateNode(List<Node> children)
            {
                return new Node(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue) { Leaf = true, Height = 1, Children = children };
            }

            public void AddChild(Node child)
            {
                if (Children == null) Children = new List<Node>();
                Children.Add(child);
            }

            // sorts node children by the best axis for split
            internal static void ChooseSplitAxis(Node node, int minEntries, int nodeChildsCount)
            {
                //var compareMinX = node.leaf ? this.compareMinX : compareNodeMinX,
                //    compareMinY = node.leaf ? this.compareMinY : compareNodeMinY,
                int xMargin = AllDistMargin(node, minEntries, nodeChildsCount, CompareMinX);
                int yMargin = AllDistMargin(node, minEntries, nodeChildsCount, CompareMinY);

                // if total distributions margin value is minimal for x, sort by minX,
                // otherwise it's already sorted by minY
                if (xMargin < yMargin)
                    node.Children.Sort(CompareMinX);
                // compareMinX);
            }

            // total margin of all possible split distributions where each node is at least m full
            internal static int AllDistMargin(Node node, int minEntries, int nodeChildsCount, Comparison<Node> comparison)
            {
                node.Children.Sort(comparison);

                var leftBBox = node.DistBBox(0, minEntries);
                var rightBBox = node.DistBBox(nodeChildsCount - minEntries, nodeChildsCount);
                int margin = leftBBox.Margin + rightBBox.Margin;

                for (int i = minEntries; i < nodeChildsCount - minEntries; i++)
                {
                    Node child = node.Children[i];
                    leftBBox.Extend(child);
                    margin += leftBBox.Margin;
                }

                for (int i = nodeChildsCount - minEntries - 1; i >= minEntries; i--)
                {
                    Node child = node.Children[i];
                    rightBBox.Extend(child);
                    margin += rightBBox.Margin;
                }

                return margin;
            }

            // min bounding rectangle of node children from k to p-1
            public BBox DistBBox(int k, int p)
            {
                var destNode = new BBox();
                destNode.Reset();

                for (int i = k; i < p; i++)
                {
                    var child = Children[i];
                    destNode.Extend(child); //node.Leaf ? toBBox(child) : child);
                }

                return destNode;
            }

            // calculate node's bbox from bboxes of its children
            public void UpdateBBox()
            {
                var bbox = DistBBox(0, Children.Count);
                MinX = bbox.MinX;
                MinY = bbox.MinY;
                MaxX = bbox.MaxX;
                MaxY = bbox.MaxY;
            }

            public static int ChooseSplitIndex(Node node, int minEntries, int nodeChildsCount)
            {

                int index = 0;

                int minOverlap = int.MaxValue;
                int minArea = int.MaxValue;

                for (int i = minEntries; i <= nodeChildsCount - minEntries; i++)
                {
                    BBox bbox1 = node.DistBBox(0, i);
                    BBox bbox2 = node.DistBBox(i, nodeChildsCount);

                    int overlap = bbox1.IntersectionArea(bbox2);
                    int area = bbox1.Area + bbox2.Area;

                    // choose distribution with minimum overlap
                    if (overlap < minOverlap)
                    {
                        minOverlap = overlap;
                        index = i;

                        minArea = area < minArea ? area : minArea;

                    }
                    else if (overlap == minOverlap)
                    {
                        // otherwise choose distribution with minimum area
                        if (area < minArea)
                        {
                            minArea = area;
                            index = i;
                        }
                    }
                }

                return index;
            }
        }

        private readonly int _maxEntries;
        private readonly int _minEntries;

        private Node _data;

        public Node All { get { return _data; } }

        public RBush()
            : this(9)        // max entries in a node is 9 by default
        {
        }

        public RBush(int maxEntries)
        {
            _maxEntries = Math.Max(4, maxEntries);
            // min node fill is 40% for best performance
            _minEntries = Math.Max(2, (int)Math.Ceiling(maxEntries * 0.4f));

            Clear();
        }

        public void Clear()
        {
            _data = Node.CreateNode(new List<Node>());
        }

        public void Insert(IBBox item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            Insert(item, _data.Height - 1, false);
        }

        private void Insert(IBBox item, int level, bool isNode)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            var bbox = item; //isNode ? item : new Node(item);

            var insertPath = new List<Node>();

            // find the best node for accommodating the item, saving all nodes along the path too
            var node = ChooseSubtree(bbox, _data, level, ref insertPath);
            
            // put the item into the node
            node.Children.Add(isNode ? (Node)item : new Node(item));
            node.Extend(bbox);

            // split on node overflow; propagate upwards if necessary
            while (level >= 0)
            {
                if (insertPath[level].Children.Count > _maxEntries)
                {
                    Split(ref insertPath, level);
                    level--;
                }
                else break;
            }

            // adjust bboxes along the insertion path
            AdjustParentBBoxes(bbox, insertPath, level);
        }

        // split overflowed node into two
        private void Split(ref List<Node> insertPath, int level)
        {
            var node = insertPath[level];
            int nodeChildsCount = node.Children.Count;

            Node.ChooseSplitAxis(node, _minEntries, nodeChildsCount);

            var splitIndex = Node.ChooseSplitIndex(node, _minEntries, nodeChildsCount);

            var splice = node.Children.GetRange(splitIndex, node.Children.Count - splitIndex);
            node.Children.RemoveRange(splitIndex, node.Children.Count - splitIndex);

            var newNode = Node.CreateNode(splice);
            newNode.Height = node.Height;
            newNode.Leaf = node.Leaf;

            node.UpdateBBox();
            newNode.UpdateBBox();

            if (level != 0) insertPath[level - 1].Children.Add(newNode);
            else SplitRoot(node, newNode);
        }

        private void SplitRoot(Node node, Node newNode)
        {
            // split root node
            _data = Node.CreateNode(new List<Node>() { node, newNode });
            _data.Height = node.Height + 1;
            _data.Leaf = false;
            _data.UpdateBBox();
        }

        private void AdjustParentBBoxes(IBBox bbox, List<Node> path, int level)
        {
            // adjust bboxes along the given tree path
            for (var i = level; i >= 0; i--)
            {
                path[i].Extend(bbox);
            }
        }

        private Node ChooseSubtree(IBBox bbox, Node node, int level, ref List<Node> path)
        {
            Node child;
            Node targetNode = null;
            int minArea, minEnlargement;

            while (true)
            {
                path.Add(node);

                if (node.Leaf || path.Count - 1 == level) break;

                minArea = minEnlargement = int.MaxValue;

                for (int i = 0, len = node.Children.Count; i < len; i++)
                {
                    child = node.Children[i];
                    int area = child.Area;
                    int enlargement = child.EnlargedArea(bbox) - area;

                    // choose entry with the least area enlargement
                    if (enlargement < minEnlargement)
                    {
                        minEnlargement = enlargement;
                        minArea = area < minArea ? area : minArea;
                        targetNode = child;

                    }
                    else if (enlargement == minEnlargement)
                    {
                        // otherwise choose one with the smallest area
                        if (area < minArea)
                        {
                            minArea = area;
                            targetNode = child;
                        }
                    }
                }

                node = targetNode ?? node.Children[0];
            }

            return node;
        }

        public List<IBBox> Search(IBBox bbox)
        {
            var node = _data;
            var result = new List<IBBox>();

            if (!node.Intersects(bbox)) return result;
            
            var nodesToSearch = new Stack<Node>();

            while (node != null)
            {
                for (int i = 0, len = node.Children.Count; i < len; i++)
                {
                    Node child = node.Children[i];
                    BBox childBBox = child;

                    if (childBBox.Intersects(bbox))
                    {
                        if (node.Leaf) result.Add(child.ExternalObject);
                        else if (BBox.Contains(bbox, childBBox)) AllCombine(child, ref result);
                        else nodesToSearch.Push(child);
                    }
                }
                node = nodesToSearch.Count > 0 ? nodesToSearch.Pop() : null;
            }

            return result;
        }

        private void AllCombine(Node node, ref List<IBBox> result)
        {
            var nodesToSearch = new Stack<Node>();
            while (node != null)
            {
                if (node.Leaf)
                {
                    //result.AddRange(node.Children);
                    //result.Add(node.ExternalObject);
                    result.AddRange(node.Children.Select(c => c.ExternalObject));
                }
                else
                {
                    foreach (var c in node.Children)
                        nodesToSearch.Push(c);
                }

                node = nodesToSearch.Count > 0 ? nodesToSearch.Pop() : null;
            }
            //return result;
        }

        public bool Collides(IBBox bbox)
        {
            var node = _data;

            if (!node.Intersects(bbox)) return false;

            var nodesToSearch = new Stack<Node>();

            while (node != null)
            {
                for (int i = 0, len = node.Children.Count; i < len; i++)
                {
                    Node child = node.Children[i];
                    BBox childBBox = child;

                    if (childBBox.Intersects(bbox))
                    {
                        if (node.Leaf || BBox.Contains(bbox, childBBox)) return true;
                        nodesToSearch.Push(child);
                    }
                }
                node = nodesToSearch.Count > 0 ? nodesToSearch.Pop() : null;
            }

            return false;
        }

        public void AddRange(IEnumerable<IBBox> data)
        {
            // if (!(data && data.length)) return this;

            if (data.Count() < _minEntries)
            {
                foreach(IBBox d in data)
                {
                    Insert(d);
                }
                return;
            }

            var copy = data.ToList();

            // recursively build the tree with the given data from scratch using OMT algorithm
            var node = Build(ref copy /*.slice()*/, 0, copy.Count - 1, 0);

            if (_data.Children.Count == 0)
            {
                // save as is if tree is empty
                _data = node;

            }
            else if (_data.Height == node.Height)
            {
                // split root if trees have the same height
                SplitRoot(_data, node);
            }
            else
            {
                if (_data.Height < node.Height)
                {
                    // swap trees if inserted one is bigger
                    var tmpNode = _data;
                    _data = node;
                    node = tmpNode;
                }

                // insert the small tree into the large tree at appropriate level
                Insert(node, _data.Height - node.Height - 1, true);
            }
        }

        private Node Build(ref List<IBBox> items, int left, int right, int height)
        {
            Node node;
            var N = right - left + 1;
            var M = _maxEntries;

            if (N <= M)
            {
                // reached leaf level; return leaf
                var slice = items.GetRange(left, N); //right + 1);

                var sliceChild = slice.Select(sl => new Node(sl)).ToList();

                node = Node.CreateNode(sliceChild/*items.slice(left, right + 1)*/);
                node.UpdateBBox();
                return node;
            }

            if (height == 0)
            {
                // target height of the bulk-loaded tree
                height = (int)Math.Ceiling(Math.Log(N) / Math.Log(M));

                // target number of root entries to maximize storage utilization
                M = (int)Math.Ceiling(N / Math.Pow(M, height - 1));
            }

            node = Node.CreateNode(new List<Node>());
            node.Leaf = false;
            node.Height = height;

            // split the items into M mostly square tiles

            int N2 = (int)Math.Ceiling((double)N / M);
            int N1 = (int)(N2 * Math.Ceiling(Math.Sqrt(M)));

            MultiSelect(ref items, left, right, N1, BBox.CompareMinX);

            for (int i = left; i <= right; i += N1)
            {
                int right2 = Math.Min(i + N1 - 1, right);

                MultiSelect(ref items, i, right2, N2, BBox.CompareMinY);

                for (int j = i; j <= right2; j += N2)
                {
                    int right3 = Math.Min(j + N2 - 1, right2);

                    // pack each entry recursively
                    node.Children.Add(Build(ref items, j, right3, height - 1));
                }
            }

            node.UpdateBBox();

            return node;
        }

        // sort an array so that items come in groups of n unsorted items, with groups sorted between each other;
        // combines selection algorithm with binary divide & conquer approach
        private void MultiSelect(ref List<IBBox> arr, int left, int right, int n, Comparison<IBBox> comparer)
        {
            var stack = new Stack<int>(new int[] { left, right });
            int mid;

            while (stack.Count != 0)
            {
                right = stack.Pop();
                left = stack.Pop();

                if (right - left <= n) continue;

                mid = left + (int)Math.Ceiling((right - left) / n / (double)2) * n;

                var ilist = (IList<IBBox>)arr;
                QuickSelect<IBBox>.Select(ref ilist, mid, left, right, comparer);

                //stack.Push(left, mid, mid, right);
                stack.Push(left);
                stack.Push(mid);
                stack.Push(mid);
                stack.Push(right);
            }
        }
    }
}
