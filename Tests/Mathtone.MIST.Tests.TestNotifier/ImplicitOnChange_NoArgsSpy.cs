using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier
{
    [Notifier(NotificationMode.ImplicitOnChange)]
    public class ImplicitOnChange_NoArgsSpy
    {
        public int NumberOfNotifications { get; private set; }

        [NotifyTarget]
        protected void NotifyTargetMethod()
        {
            NumberOfNotifications++;
        }

        [Notify]
        public string StringValue { get; set; }
    }
}
