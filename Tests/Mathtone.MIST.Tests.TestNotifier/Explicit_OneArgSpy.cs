using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier
{
    [Notifier]
    public class Explicit_OneArgSpy
    {
        public List<string> Notifications { get; private set; }

        [NotifyTarget]
        protected void NotifyTargetMethod(string propertyName)
        {
            Notifications.Add(propertyName);
        }

        [Notify]
        public string StringValue { get; set; }

        public Explicit_OneArgSpy()
        {
            Notifications = new List<string>();
        }
    }
}
