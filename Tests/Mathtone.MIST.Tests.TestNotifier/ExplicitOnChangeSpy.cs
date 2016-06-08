using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier
{
    [Notifier]
    public class ExplicitOnChangeSpy
    {
        [SuppressNotify]
        public int NumberOfNotifications { get; private set; }

        [NotifyTarget]
        protected void NotifyTargetMethod()
        {
            NumberOfNotifications++;
        }

        [Notify(NotifyMode.OnChange)]
        public string StringValue { get; set; }
    }
}
