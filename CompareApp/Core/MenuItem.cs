namespace CompareApp.Core
{
    using System.Windows.Input;
    public class MenuItem : BindableBase
    {
        public string Header { get; }

        public ICommand Command { get; }

        public MenuItem(string header, ICommand command)
        {
            this.Header = header;
            this.Command = command;
        }
    }
}