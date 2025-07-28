using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public class ValidationResult
    {
		public static ValidationResult DefaultSuccessResult = new ValidationResult(true);

		public ValidationResult(bool passed)
			: this(passed, passed ? "Passed" : "Failed")
		{
		}

		public ValidationResult(bool passed, string message)
        {
            this.Passed = passed;
            this.Message = message;
        }

        public bool Passed { get; private set; }
        public string Message { get; private set; }
	}
}
