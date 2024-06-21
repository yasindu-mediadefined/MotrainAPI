using System;
using System.Globalization;
using log4net;
using log4net.Config;
using System.Reflection;

namespace NetExamMotrainFileGeneration
{
    /// <summary>
    /// Summary description for Logger.
    /// </summary>
    internal sealed class Logger
    {

        private static ILog log = log4net.LogManager.GetLogger("LogApp001.Logging");

        //private constants 
        private static readonly string NULL = "null";
        private static readonly string NOT_SUPPLIED = "NOT_SUPPLIED";
        private static readonly string EXCEPTION_IN_CREATING_LOGGING_MESSAGE = "EXCEPTION_IN_CREATING_LOGGING_MESSAGE";
        private static readonly string METHOD_ELEMENT_NAME = "method";
        private static readonly string PARAMETER_ELEMENT_NAME = "parameter";

        //Public Propoties
        public static bool IsDebugEnabled
        {
            get
            {
                return Logger.log.IsDebugEnabled;
            }
        }

        public static bool IsErrorEnabled
        {
            get
            {
                return Logger.log.IsErrorEnabled;
            }
        }

        public static bool IsFatalEnabled
        {
            get
            {
                return Logger.log.IsFatalEnabled;
            }
        }

        public static bool IsInfoEnabled
        {
            get
            {
                return Logger.log.IsInfoEnabled;
            }
        }

        public static bool IsWarnEnabled
        {
            get
            {
                return Logger.log.IsWarnEnabled;
            }
        }


        //Error Methods

        public static void Error(Exception exception, MethodBase methodBase, params object[] values)
        {
            if (!Logger.IsErrorEnabled) return;
            log.Error(Logger.CreateMessage(methodBase, exception));
        }

        public static void Error(object message, Exception exception)
        {
            if (!Logger.IsErrorEnabled) return;
            log.Error(message, exception);
        }

        public static void Error(object message)
        {
            if (!Logger.IsErrorEnabled) return;
            log.Error(message);
        }

        //Info Methods

        public static void Info(object message, Exception exception)
        {
            if (!Logger.IsInfoEnabled) return;
            log.Info(message, exception);
        }

        public static void Info(object message)
        {
            if (!Logger.IsInfoEnabled) return;
            log.Info(message);
        }

        public static void Info(MethodBase methodBase, params object[] values)
        {
            if (!Logger.IsInfoEnabled) return;
            log.Error(Logger.CreateMessage(methodBase, values));
        }


        //Warn Methods

        public static void Warn(object message, Exception exception)
        {
            if (!Logger.IsWarnEnabled) return;
            log.Warn(message, exception);
        }

        public static void Warn(object message)
        {
            if (!Logger.IsWarnEnabled) return;
            log.Warn(message);
        }

        //Debug Methods

        public static void Debug(MethodBase methodBase, params object[] values)
        {
            if (!Logger.IsDebugEnabled) return;
            log.Debug(Logger.CreateMessage(methodBase, values));
        }

        public static void Debug(object message, Exception exception)
        {
            if (!Logger.IsDebugEnabled) return;
            log.Debug(message, exception);
        }

        public static void Debug(string methodName, params object[] values)
        {
            if (!Logger.IsDebugEnabled) return;

            string msg = string.Empty;
            if (methodName != null) msg += methodName;

            if (values != null)
            {
                foreach (object val in values)
                    msg += " " + Logger.ToString(val);
            }

            log.Debug(msg);
        }

        public static void Debug(object message)
        {
            if (!Logger.IsDebugEnabled) return;
            log.Debug(message);
        }

        //Fatal Methods

        public static void Fatal(object message, Exception exception)
        {
            if (!Logger.IsFatalEnabled) return;
            log.Fatal(message, exception);
        }

        public static void Fatal(object message)
        {
            if (!Logger.IsFatalEnabled) return;
            log.Fatal(message);
        }

        //Private Helper Methods

        /// <summary>
        /// Get a String representation of the object if possible
        /// </summary>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        private static string ToString(object fieldValue)
        {
            string returnValue = string.Empty;

            if (fieldValue.GetType().IsArray)
            {
                foreach (object obj in (object[])fieldValue)
                {
                    returnValue += Logger.ToString(obj);
                }
            }
            else if (fieldValue != null)
            {
                returnValue = fieldValue.ToString();
            }
            else
            {
                returnValue = Logger.NULL;
            }

            return returnValue;
        }

        /// <summary>
        /// Helper Mehtod to create a Log Message
        /// </summary>
        /// <param name="methodBase"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private static string CreateMessage(MethodBase methodBase, params object[] values)
        {
            if (methodBase == null) return string.Empty;

            string message = string.Empty;

            try
            {
                message += "<" + Logger.METHOD_ELEMENT_NAME + ">";
                message += "<" + Logger.PARAMETER_ELEMENT_NAME + ">"
                    + methodBase.Name
                    + "</" + Logger.PARAMETER_ELEMENT_NAME + ">";

                ParameterInfo[] parametersInfoArray = methodBase.GetParameters();

                if (values != null)
                {
                    for (int i = 0; i < parametersInfoArray.Length; i++)
                    {
                        message += "<" + parametersInfoArray[i].Name + ">";

                        if (i > values.Length - 1)
                            message += Logger.NOT_SUPPLIED;
                        else
                        {
                            if (values[i] != null)
                                message += values[i].ToString();
                            else
                                message += Logger.NULL;
                        }

                        message += "</" + parametersInfoArray[i].Name + ">";
                    }
                }
                message += "</" + Logger.METHOD_ELEMENT_NAME + ">";
            }
            catch (Exception)
            {
                //	an exception occured while trying to gernerate a logging message
                //	so we'll just log the original exception 
                //	the message cannot be trusted so we'll change it
                message = Logger.EXCEPTION_IN_CREATING_LOGGING_MESSAGE;
            }

            return message;
        }

        internal static void Error()
        {
            throw new NotImplementedException();
        }
    }
}
