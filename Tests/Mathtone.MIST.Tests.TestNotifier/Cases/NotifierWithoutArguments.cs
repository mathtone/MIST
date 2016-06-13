using Mathtone.MIST.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier.Cases
{
    [Notifier]
    public class NotifierWithoutArguments
    {
        [SuppressNotify]
        public int ChangeCount { get; private set; }

        [Notify]
        public string OnSetString { get; set; }

        [Notify(NotificationStyle.OnChange)]
        public string OnChangeString { get; set; }

        [NotifyTarget]
        protected void OnPropertyChanged()
        {
            ChangeCount++;
        }
    }
}