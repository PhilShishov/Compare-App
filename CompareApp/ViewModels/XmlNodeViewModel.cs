namespace CompareApp.ViewModels
{
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using CompareApp.Core;

    public class XmlNodeViewModel : BindableBase
    {

        public ObservableCollection<XmlNodeViewModel> Children { get; private set; }

        private bool _isExpanded;
        
        public bool IsExpanded
        {
            get { return this._isExpanded; }
            set
            {
                this._isExpanded = value;
                this.RaisePropertyChanged();
            }
        }       
        public string Header
        {
            get
            {
                if (string.IsNullOrEmpty(this.Value))
                {
                    return this.Name;
                }
                else
                {
                    return $"{this.Name} - {this.Value}";
                }
            }
        }       
        public MenuItem[] Menu
        {
            get
            {
                var items = new List<MenuItem>
                {
                    new MenuItem("Add all missing",
                        new DelegateCommand(
                            o =>
                                this.GetPath()
                                    .First()
                                    .GetAllDescending()
                                    .Where(x => x.IsMissing)
                                    .ToList()
                                    .ForEach(this.AddAction)))
                };


                if (this.IsMissing)
                {
                    items.Add(new MenuItem("Add node to xml", new DelegateCommand(o => this.AddAction(this))));
                    items.Add(new MenuItem("Add node and children to xml",
                            new DelegateCommand(
                                o => this.GetAllDescending().Where(x => x.IsMissing).ToList().ForEach(this.AddAction),
                                o => this.GetAllDescending().Any(x => x.IsMissing))));
                }

                return items.ToArray();
            }
        }

        /// <summary>
        ///     Gets all descending children.
        /// </summary>
        /// <returns>All descending children for the node.</returns>
        public IEnumerable<XmlNodeViewModel> GetAllDescending()
        {
            foreach (var child in this.Children)
            {
                yield return child;

                foreach (var grandChild in child.GetAllDescending())
                {
                    yield return grandChild;
                }
            }
        }

        /// <summary>
        ///     The name of the node.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     The value of the node.
        /// </summary>
        public string Value { get; private set; }

        private bool _isMissing;

        /// <summary>
        ///     <c>true</c> if the node is missing in the
        ///     other tree, <c>false</c> otherwise.
        /// </summary>
        public bool IsMissing
        {
            get { return this._isMissing; }
            set
            {
                if (this.IsMissing != value)
                {
                    this._isMissing = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(this.Menu));
                }
            }
        }

        /// <summary>
        ///     Action that adds the node to another tree.
        /// </summary>
        public Action<XmlNodeViewModel> AddAction { get; private set; }

        private XmlNodeViewModel()
        {

        }

        /// <summary>
        ///     Creates a xml representation of the <see cref="XElement"/>.
        /// </summary>
        /// <param name="root">The root of the tree.</param>
        /// <param name="addAction">The action to add nodes to another tree.</param>
        /// <returns>The root of the representation.</returns>
        public static XmlNodeViewModel FromXml(XElement root, Action<XmlNodeViewModel> addAction)
        {
            var node = new XmlNodeViewModel
            {
                Name = root.Name.LocalName,
                Value = root.HasElements ? string.Empty : root.Value,
                AddAction = addAction
            };

            node.Children =
                new ObservableCollection<XmlNodeViewModel>(
                    root.Elements().Select(x => FromXml(x, addAction).WithParent(node)));

            return node;
        }

        /// <summary>
        ///     Sets the parent of this node.
        /// </summary>
        /// <param name="parent">The new parent.</param>
        /// <returns>This node.</returns>
        public XmlNodeViewModel WithParent(XmlNodeViewModel parent)
        {
            this.Parent = parent;

            return this;
        }

        /// <summary>
        ///     Creates a xsd representation of the <see cref="XElement"/>.
        /// </summary>
        /// <param name="root">The root of the tree.</param>
        /// <param name="addAction">The action to add nodes to another tree.</param>
        /// <returns>The root of the representation.</returns>
        public static XmlNodeViewModel FromXsd(XElement root, Action<XmlNodeViewModel> addAction)
        {
            return CreateXsd(root, null, addAction).Single();
        }

        /// <summary>
        ///    Creates a xsd representation of the <see cref="XElement"/>. 
        /// </summary>
        /// <param name="root">The root of the tree.</param>
        /// <param name="parent">The parent for the created nodes.</param>
        /// <param name="addAction">The action to add nodes to another tree.</param>
        /// <returns>Created nodes of this depth.</returns>
        private static IEnumerable<XmlNodeViewModel> CreateXsd(XElement root, XmlNodeViewModel parent, Action<XmlNodeViewModel> addAction)
        {
            if (root.Attribute("name") == null)
            {
                foreach (var child in root.Elements().SelectMany(x => CreateXsd(x, parent, addAction)))
                {
                    yield return child;
                }
            }
            else
            {
                var node = new XmlNodeViewModel
                {
                    Name = root.Attribute("name")?.Value ?? string.Empty,
                    Value = root.Attribute("default")?.Value ?? string.Empty,
                    AddAction = addAction,
                    Parent = parent
                };
                node.Children = new ObservableCollection<XmlNodeViewModel>(root.Elements().SelectMany(x => CreateXsd(x, node, addAction)).Where(x => x != null));

                yield return node;
            }

        }

        /// <summary>
        ///     Checks if this and descending nodes exist 
        ///     in another tree and marks missing nodes.
        /// </summary>
        /// <param name="root">The root of the tree that is checked.</param>
        public void CompareTo(XmlNodeViewModel root)
        {
            this.IsMissing = !root.Contains(this);
            if (this.IsMissing)
            {
                this.ExpandPath();
            }

            foreach (var child in this.Children)
            {
                child.CompareTo(root);
            }
        }

        /// <summary>
        ///     Adds a child to this node.
        /// </summary>
        /// <param name="child">The child to add.</param>
        public void AddChild(XmlNodeViewModel child)
        {
            this.Children.Add(child.WithParent(this));

            child.ExpandPath();
        }

        /// <summary>
        ///     Expands the path to this node.
        /// </summary>
        public void ExpandPath()
        {
            foreach (var pathNode in this.GetPath())
            {
                pathNode.IsExpanded = true;
            }
        }

        /// <summary>
        ///     Checks if this sub tree contains the specified node.
        /// </summary>
        /// <param name="node">The node to check for.</param>
        /// <returns><c>true</c> if the sub tree contains the <param name="node">node</param>, <c>false</c> otherwise.</returns>
        private bool Contains(XmlNodeViewModel node)
        {
            var nodes = new[] { this };
            foreach (var pathNode in node.GetPath())
            {
                var found = nodes.FirstOrDefault(x => x.Equals(pathNode));

                if (found == null)
                {
                    return false;
                }

                nodes = found.Children.ToArray();
            }

            return true;
        }

        /// <summary>
        ///     Checks for equality to another object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns><c>true</c> if node equals to <param name="obj">node</param>, <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as XmlNodeViewModel;
            if (other != null)
            {
                return this.Name == other.Name && this.Value == other.Value;
            }

            return false;
        }

        /// <summary>
        ///     The parent of this node.
        /// </summary>
        public XmlNodeViewModel Parent { get; private set; }

        /// <summary>
        ///     Creates a list of nodes that
        ///     lie on the path to this node.
        /// </summary>
        /// <returns>Nodes to reach this node (including).</returns>
        public XmlNodeViewModel[] GetPath()
        {
            var path = new List<XmlNodeViewModel>();

            var node = this;
            while (node != null)
            {
                path.Add(node);

                node = node.Parent;
            }

            path.Reverse();

            return path.ToArray();
        }

        public override string ToString()
        {
            return this.Header;
        }

        /// <summary>
        ///     Clones this node.
        /// </summary>
        /// <returns>A clone of this node, excluding the parent.</returns>
        public XmlNodeViewModel Clone()
        {
            return new XmlNodeViewModel
            {
                Name = this.Name,
                Value = this.Value,
                Children = new ObservableCollection<XmlNodeViewModel>()
            };
        }

        /// <summary>
        ///     Creates a <see cref="XElement"/> from this viewmodel.
        /// </summary>
        /// <returns>The <see cref="XElement"/> representation of this viewmodel.</returns>
        public XElement ToXElement()
        {
            var x = new XElement(this.Name.Trim(), string.IsNullOrEmpty(this.Value) ? null : this.Value.Trim());

            foreach (var child in this.Children)
            {
                x.Add(child.ToXElement());
            }

            return x;
        }
    }
}