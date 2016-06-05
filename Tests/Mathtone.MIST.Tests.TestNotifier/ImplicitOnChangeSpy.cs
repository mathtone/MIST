using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier
{
    [Notifier(NotificationMode.ImplicitOnChange)]
    public class ImplicitOnChangeSpy
    {
        [SuppressNotify]
        public int NumberOfNotifications { get; private set; }

        [NotifyTarget]
        protected void NotifyTargetMethod()
        {
            NumberOfNotifications++;
        }

        public string StringValue { get; set; }

        [Notify(NotifyMode.OnSet)]
        public string ExplicitOnSetString { get; set; }
    }
}
