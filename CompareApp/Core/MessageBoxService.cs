namespace CompareApp.Core
{
    using System.Windows;

    public class MessageBoxService : IMessageService
    {
        public void Show(string msg, string title)
        {
            MessageBox.Show(msg, title);
        }
    }
}