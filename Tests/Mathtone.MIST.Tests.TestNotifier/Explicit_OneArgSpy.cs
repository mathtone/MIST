using Mathtone.MIST.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST.TestNotifier
{
    [Notifier]
    public class Explicit_OneArgSpy : TestNotifierBase
    {
        [Notify]
        public string StringValue { get; set; }
    }
}
