using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier
{
    [Notifier(NotificationMode.ImplicitOnChange)]
    public class ImplicitOnChange_OneArgSpy
    {
        public List<string> Notifications { get; private set; }

        [NotifyTarget]
        protected void NotifyTargetMethod(string propertyName)
        {
            Notifications.Add(propertyName);
        }

        [Notify]
        public string StringValue { get; set; }

        public ImplicitOnChange_OneArgSpy()
        {
            Notifications = new List<string>();
        }
    }
}
