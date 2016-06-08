using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier
{
    [Notifier(NotificationMode.Implicit)]
    public class ImplicitSpy
    {
        [SuppressNotify]
        public int NumberOfNotifications { get; private set; }

        [NotifyTarget]
        protected void NotifyTargetMethod()
        {
            NumberOfNotifications++;
        }

        public string StringValue { get; set; }
    }
}
