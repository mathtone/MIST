using Mathtone.MIST.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier
{
    [Notifier(NotificationMode.Implicit)]
    public class ImplicitSpy : TestNotifierBase
    {
        public string StringValue { get; set; }
    }
}