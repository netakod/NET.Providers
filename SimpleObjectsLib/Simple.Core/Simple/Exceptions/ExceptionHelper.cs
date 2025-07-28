using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public class ExceptionHelper
    {
        public static string GetFullErrorMessage(Exception ex)
        {
            string errorMessage;

            errorMessage = ex.Message;
            while (ex.InnerException != null)
            {
                errorMessage += Environment.NewLine + ex.InnerException.Message;
                ex = ex.InnerException;
            }

            return errorMessage;
        }
    }
}
