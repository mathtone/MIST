using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathtone.MIST {

	public class CannotLocateNotifyTargetException : Exception {
		public CannotLocateNotifyTargetException(string typeName) :
			base($"{Resources.CannotLocateNotifyTarget}: {typeName}") {
		}
	}

	public class InvalidNotifyTargetException : Exception {
		public InvalidNotifyTargetException(string methodName) :
			base(String.Format(Resources.InvalidNotifyTarget, methodName)) {
		}
	}

	public class InvalidNotifierException : Exception {
		public InvalidNotifierException() :
			base(Resources.NotifyAttributeCannotBeSet) {
		}
	}

	public class BuildTaskErrorException : Exception {
		public BuildTaskErrorException(string typeName, Exception innerException = null) :
			base(String.Format(Resources.BuildTaskError, typeName), innerException) {
		}
	}
}