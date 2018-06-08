using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebInteraction
{
    public sealed class HttpError : Dictionary<string, object>
    {
        /// <summary>Gets or sets the high-level, user-visible message explaining the cause of the error. Information carried in this field should be considered public in that it will go over the wire regardless of the <see cref="T:System.Web.Http.IncludeErrorDetailPolicy" />. As a result care should be taken not to disclose sensitive information about the server or the application.</summary>
        /// <returns>The high-level, user-visible message explaining the cause of the error. Information carried in this field should be considered public in that it will go over the wire regardless of the <see cref="T:System.Web.Http.IncludeErrorDetailPolicy" />. As a result care should be taken not to disclose sensitive information about the server or the application.</returns>
        public string Message
        {
            get
            {
                return this.GetPropertyValue<string>(HttpErrorKeys.MessageKey);
            }
            set
            {
                base[HttpErrorKeys.MessageKey] = value;
            }
        }
        /// <summary>Gets the <see cref="P:System.Web.Http.HttpError.ModelState" /> containing information about the errors that occurred during model binding.</summary>
        /// <returns>The <see cref="P:System.Web.Http.HttpError.ModelState" /> containing information about the errors that occurred during model binding.</returns>
        public HttpError ModelState
        {
            get
            {
                return this.GetPropertyValue<HttpError>(HttpErrorKeys.ModelStateKey);
            }
        }
        /// <summary>Gets or sets a detailed description of the error intended for the developer to understand exactly what failed.</summary>
        /// <returns>A detailed description of the error intended for the developer to understand exactly what failed.</returns>
        public string MessageDetail
        {
            get
            {
                return this.GetPropertyValue<string>(HttpErrorKeys.MessageDetailKey);
            }
            set
            {
                base[HttpErrorKeys.MessageDetailKey] = value;
            }
        }
        /// <summary>Gets or sets the message of the <see cref="T:System.Exception" /> if available.</summary>
        /// <returns>The message of the <see cref="T:System.Exception" /> if available.</returns>
        public string ExceptionMessage
        {
            get
            {
                return this.GetPropertyValue<string>(HttpErrorKeys.ExceptionMessageKey);
            }
            set
            {
                base[HttpErrorKeys.ExceptionMessageKey] = value;
            }
        }
        /// <summary>Gets or sets the type of the <see cref="T:System.Exception" /> if available.</summary>
        /// <returns>The type of the <see cref="T:System.Exception" /> if available.</returns>
        public string ExceptionType
        {
            get
            {
                return this.GetPropertyValue<string>(HttpErrorKeys.ExceptionTypeKey);
            }
            set
            {
                base[HttpErrorKeys.ExceptionTypeKey] = value;
            }
        }
        /// <summary>Gets or sets the stack trace information associated with this instance if available.</summary>
        /// <returns>The stack trace information associated with this instance if available.</returns>
        public string StackTrace
        {
            get
            {
                return this.GetPropertyValue<string>(HttpErrorKeys.StackTraceKey);
            }
            set
            {
                base[HttpErrorKeys.StackTraceKey] = value;
            }
        }

        public HttpError InnerException
        {
            get
            {
                return this.GetPropertyValue<HttpError>(HttpErrorKeys.InnerExceptionKey);
            }
        }

        public HttpError() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public TValue GetPropertyValue<TValue>(string key)
        {
            object result;
            if (this.TryGetValue(key, out result))
            {
                return (TValue)result;
            }
            return default(TValue);
        }


        public static void CheckResponse(HttpResponseMessage rsp)
        {
            if (rsp.IsSuccessStatusCode)
            {
                return;
            }

            var conentType = rsp.Content.Headers.ContentType;
            if ((conentType == null) || (conentType.MediaType != "application/json"))       //不可识别的异常信息，直接抛出标准http异常
            {
                rsp.EnsureSuccessStatusCode();
                return;
            }

            var err = Task.Run(() => rsp.Content.ReadAsAsync<HttpError>()).WaitForResult();

            var errorMsg = err.Message;
#if DEBUG
            errorMsg = string.Join("\n", errorMsg, err.ExceptionMessage, err.StackTrace);
#endif
            if (string.IsNullOrEmpty(errorMsg))
            {
                rsp.EnsureSuccessStatusCode();
            }
            else
            {
                throw new InvalidOperationException(errorMsg);
            }
        }
    }
    public static class HttpErrorKeys
    {
        /// <summary> Provides a key for the Message. </summary>
        public static readonly string MessageKey = "Message";
        /// <summary> Provides a key for the MessageDetail. </summary>
        public static readonly string MessageDetailKey = "MessageDetail";
        /// <summary> Provides a key for the ModelState. </summary>
        public static readonly string ModelStateKey = "ModelState";
        /// <summary> Provides a key for the ExceptionMessage. </summary>
        public static readonly string ExceptionMessageKey = "ExceptionMessage";
        /// <summary> Provides a key for the ExceptionType. </summary>
        public static readonly string ExceptionTypeKey = "ExceptionType";
        /// <summary> Provides a key for the StackTrace. </summary>
        public static readonly string StackTraceKey = "StackTrace";
        /// <summary> Provides a key for the InnerException. </summary>
        public static readonly string InnerExceptionKey = "InnerException";
        /// <summary> Provides a key for the MessageLanguage. </summary>
        public static readonly string MessageLanguageKey = "MessageLanguage";
        /// <summary> Provides a key for the ErrorCode. </summary>
        public static readonly string ErrorCodeKey = "ErrorCode";
    }
}
