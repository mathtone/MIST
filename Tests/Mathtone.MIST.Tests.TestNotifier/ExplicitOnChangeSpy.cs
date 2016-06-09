using Mathtone.MIST.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier
{
    [Notifier]
    public class ExplicitOnChangeSpy : TestNotifierBase
    {
		[Notify(NotificationStyle.OnChange)]
        public string StringValue { get; set; }
    }
}
