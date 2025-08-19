using System;
using System.ComponentModel;
using Mathtone.MIST;

namespace TestProject
{
    [Notifier(NotificationMode.Implicit)]
    public class TestNotifier : INotifyPropertyChanged
    {
        [NotifyTarget]
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("MIST Test Project");
            
            var notifier = new TestNotifier();
            notifier.PropertyChanged += (s, e) => Console.WriteLine($"Property changed: {e.PropertyName}");
            
            notifier.FirstName = "John";
            notifier.LastName = "Doe";

            Console.WriteLine("Test completed!");
        }
    }
}