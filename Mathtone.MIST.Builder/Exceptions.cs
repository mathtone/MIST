using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathtone.MIST {
	[Serializable]
	public class CannotLocateNotifyTargetException : Exception {
		public CannotLocateNotifyTargetException(string typeName) :
			base($"{Resources.CannotLocateNotifyTarget}: {typeName}") {
		}
	}

	[Serializable]
	public class InvalidNotifyTargetException : Exception {
		public InvalidNotifyTargetException(string methodName) :
			base(String.Format(Resources.InvalidNotifyTarget, methodName)) {
		}
	}

	[Serializable]
	public class InvalidNotifierException : Exception {
		public InvalidNotifierException() :
			base(Resources.NotifyAttributeCannotBeSet) {
		}
	}

	[Serializable]
	public class BuildTaskErrorException : Exception {
		public BuildTaskErrorException(string typeName, Exception innerException = null) :
			base(String.Format(Resources.BuildTaskError, typeName), innerException) {
		}
	}
}