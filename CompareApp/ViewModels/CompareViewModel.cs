
namespace CompareApp.ViewModels
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using System.Diagnostics;
    using System.Windows.Input;

    using Microsoft.Win32;

    using CompareApp.Core;
    /// <summary>
    ///     Viewmodel for xml/xsd comparison.
    /// </summary>
    public class CompareViewModel : BindableBase
    {
        /// <summary>
        ///     File extension for xml's.
        /// </summary>
        public const string FilterXml = "xml";

        /// <summary>
        ///     File extension for xsd's.
        /// </summary>
        public const string FilterXsd = "xsd";

        private string _pathXsd = "Please select file";

        private string _pathXml = "Please select file";

        /// <summary>
        ///     Path to selected xsd file.
        /// </summary>
        public string PathXsd
        {
            get { return this._pathXsd; }

            set
            {
                this._pathXsd = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Path to selected xml file.
        /// </summary>
        public string PathXml
        {
            get { return this._pathXml; }

            set
            {
                this._pathXml = value;
                this.RaisePropertyChanged();
            }
        }

        private XmlNodeViewModel[] _xml;

        /// <summary>
        ///     Viewmodel for xml data.
        /// </summary>
        public XmlNodeViewModel[] Xml
        {
            get { return this._xml; }

            set
            {
                this._xml = value;
                this.RaisePropertyChanged();
            }
        }

        private XmlNodeViewModel[] _xsd;

        /// <summary>
        ///     Viewmodel for xsd data.
        /// </summary>
        public XmlNodeViewModel[] Xsd
        {
            get { return this._xsd; }

            set
            {
                this._xsd = value;
                this.RaisePropertyChanged();
            }
        }

        private readonly IMessageService _messageService;

        public CompareViewModel(IMessageService messageService)
        {
            this._messageService = messageService;
        }

        /// <summary>
        ///     <see cref="ICommand"/> for comparing <see cref="Xml"/> and <see cref="Xsd"/>.
        /// </summary>
        public ICommand CompareCommand => new DelegateCommand(this.Compare, o => this._xml != null && this._xsd != null);

        /// <summary>
        ///     Compares <see cref="Xsd"/> to <see cref="Xml"/> and marks specific nodes that are missing.
        /// </summary>
        /// <param name="obj"></param>
        private void Compare(object obj)
        {
            this.Xsd.Single().CompareTo(this.Xml.Single());
        }

        /// <summary>
        ///     <see cref="ICommand"/> for loading an xml for comparison.
        /// </summary>
        public ICommand LoadXmlCommand => new DelegateCommand(this.LoadXml);

        /// <summary>
        ///     Loads the xml file into <see cref="Xml"/>.
        /// </summary>
        /// <param name="obj">Not used.</param>
        private void LoadXml(object obj)
        {
            this.Load(path => this.PathXml = path, data => this.Xml = data, FilterXml);
        }

        /// <summary>
        ///     Loads a file as <see cref="XElement"/> into a specified property.
        /// </summary>
        /// <param name="setterPath">The property setter for the file path.</param>
        /// <param name="setterData">The property setter for the <see cref="XElement"/> data.</param>
        /// <param name="filter">The filter string.</param>
        private void Load(Action<string> setterPath, Action<XmlNodeViewModel[]> setterData, string filter)
        {
            var dialog = new OpenFileDialog
            {
                Filter = $"Comparable files (*.{filter})|*.{filter}",
                FilterIndex = 0,
                InitialDirectory = Path.Combine(Environment.CurrentDirectory, "Datasets")
            };

            var showDialog = dialog.ShowDialog();
            if (showDialog != null && (bool)showDialog)
            {
                try
                {
                    var data = File.ReadAllText(dialog.FileName);
                    var xElement = XElement.Parse(data);

                    setterPath(dialog.FileName);
                    setterData(new[] { (filter == FilterXsd ? XmlNodeViewModel.FromXsd(xElement, this.AddToXml) : XmlNodeViewModel.FromXml(xElement, this.AddToXml)) });
                }
                catch (Exception exception)
                {
                    this._messageService.Show(exception.Message, exception.GetType().Name);
                }
            }
        }

        /// <summary>
        ///     Adds one node of a tree to another.
        /// </summary>
        /// <param name="node">The node to add.</param>
        public void AddToXml(XmlNodeViewModel node)
        {
            var nodes = this.Xml;
            XmlNodeViewModel parent = null;
            foreach (var pathNode in node.GetPath())
            {
                var found = nodes.FirstOrDefault(x => x.Equals(pathNode));

                if (found == null)
                {
                    found = pathNode.Clone();
                    parent?.AddChild(found);
                }

                parent = found;
                nodes = parent.Children.ToArray();
            }

            this.Compare(null);
        }

        /// <summary>
        ///     <see cref="ICommand"/> for loading an xsd for comparison.
        /// </summary>
        public ICommand LoadXsdCommand => new DelegateCommand(this.LoadXsd);

        /// <summary>
        ///     Loads the xsd file into <see cref="Xsd"/>.
        /// </summary>
        /// <param name="obj">Not used.</param>
        private void LoadXsd(object obj)
        {
            this.Load(path => this.PathXsd = path, data => this.Xsd = data, FilterXsd);
        }

        /// <summary>
        ///     <see cref="ICommand"/> for saving the current <see cref="Xml"/>.
        /// </summary>
        public ICommand SaveCommand => new DelegateCommand(this.SaveXml, o => this._xml != null && this._xsd != null);

        /// <summary>
        ///     Saves the <see cref="Xml"/> data into a file.
        /// </summary>
        /// <param name="obj">Not used.</param>
        private void SaveXml(object obj)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "Xml files (*.xml)|*.xml",
                FilterIndex = 0,
                InitialDirectory = Path.Combine(Environment.CurrentDirectory, "Datasets"),
                AddExtension = true
            };

            var showDialog = dialog.ShowDialog();
            if (showDialog != null && (bool)showDialog)
            {
                try
                {
                    this.Xml.Single().ToXElement().Save(dialog.FileName);

                    Process.Start(dialog.FileName);
                }
                catch (Exception exception)
                {
                    this._messageService.Show(exception.Message, exception.GetType().Name);
                }
            }
        }
    }
}