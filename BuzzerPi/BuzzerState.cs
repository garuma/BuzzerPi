using System;
namespace BuzzerPi
{
	public class BuzzerState
	{
		public string BuzzerId {
			get;
			set;
		}

		public bool NewState {
			get;
			set;
		}

		public bool OldState {
			get;
			set;
		}
	}
}

