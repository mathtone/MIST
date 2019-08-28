using Mathtone.MIST.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathtone.MIST.TestNotifier.Cases {
    [Notifier(NotificationMode.Implicit)]
    public class ImplicitSpy : TestNotifierBase
    {
        public string StringValue { get; set; }
    }
}