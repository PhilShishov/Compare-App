namespace CompareApp.Views
{
    using System.Linq;
    using System.Xml.Linq;
    using System.Windows.Controls;

    using CompareApp.Core;
    using CompareApp.ViewModels;

    /// <summary>
    /// Interaction logic for CompareView.xaml
    /// </summary>
    public partial class CompareView : UserControl
    {
        public CompareView()
        {
            this.InitializeComponent();

            this.DataContext = new CompareViewModel(new MessageBoxService());
        }

        private void BuildTree(ItemCollection itemCollection, XElement xElement, bool isSchema)
        {
            string header = string.Empty;
            if (xElement.Elements().Count() != 0)
            {
                if (isSchema)
                {
                    var attr = xElement.Attribute("name");
                    if (attr != null)
                    {
                        header = xElement.Attribute("name").Value;
                    }
                    else
                    {
                        foreach (var xElem in xElement.Elements())
                        {
                            BuildTree(itemCollection, xElem, isSchema);
                        }
                        return;
                    }
                }
                else
                {
                    header = xElement.Name.LocalName;
                }
            }
            else
            {
                if (isSchema)
                {
                    header = xElement.Attribute("name").Value + " - " + xElement.Attribute("default").Value;
                }
                else
                {
                    header = xElement.Name.LocalName + " - " + xElement.Value;
                }
            }
            var item = new TreeViewItem() { Header = header, Tag = xElement };
            itemCollection.Add(item);
            foreach (var xElem in xElement.Elements())
            {
                BuildTree(item.Items, xElem, isSchema);
            }
        }
    }
}
