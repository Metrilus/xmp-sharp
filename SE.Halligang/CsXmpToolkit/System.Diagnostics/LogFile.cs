using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace System.Diagnostics
{
	/// <summary>
	/// 
	/// </summary>
    public class LogFile
    {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="fullPath"></param>
		/// <param name="overwrite"></param>
		/// <param name="maxFileSize"></param>
		public LogFile(string key, string fullPath, bool overwrite, long maxFileSize)
		{
			if (logFiles.ContainsKey(key))
			{
				throw new ArgumentException("Log instance for '" + key + "' already exists.");
			}

			this.fullPath = fullPath;
			this.maxFileSize = maxFileSize;
			this.timer = new Timer(this.TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
			this.validInstance = true;

			string directory = Path.GetDirectoryName(fullPath);
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			if (overwrite && File.Exists(fullPath))
			{
				File.Delete(fullPath);
			}

			logFiles.Add(key, this);
		}

		private LogFile()
		{
			this.fullPath = null;
			this.maxFileSize = -1;
			this.timer = null;
			this.validInstance = false;
		}

		/// <summary>
		/// 
		/// </summary>
		~LogFile()
        {
            CloseLog();
        }

        private string fullPath;
        /// <summary>
        /// 
        /// </summary>
        public string FullPath
        {
            get { return fullPath; }
        }

		private long maxFileSize;
		/// <summary>
		/// 
		/// </summary>
		public long MaxFileSize
		{
			get { return maxFileSize; }
		}

        private static bool logEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["LogEnabled"]);
		private static Dictionary<string, LogFile> logFiles = new Dictionary<string, LogFile>();

		private bool validInstance;
        private FileStream logFile = null;
        private DateTime lastAppend = DateTime.MinValue;
        private Timer timer;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static LogFile GetInstance(string key)
        {
			if (logFiles.ContainsKey(key))
			{
				return logFiles[key];
			}
			else
			{
				return new LogFile();
			}
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="level"></param>
		/// <param name="className"></param>
		/// <returns></returns>
		public bool Enabled(TraceLevel level, string className)
		{
			string moduleName = Assembly.GetCallingAssembly().GetName().Name;
			return Enabled(level, moduleName, className);
		}

		private bool Enabled(TraceLevel level, string moduleName, string className)
        {
			if (validInstance && logEnabled)
            {
                string logLevelString = null;
				if (className != null)
				{
					logLevelString = ConfigurationManager.AppSettings["LogLevel." + moduleName + "." + className];
                }
                if (logLevelString == null)
                {
                    logLevelString = ConfigurationManager.AppSettings["LogLevel." + moduleName];
                }
                if (logLevelString == null)
                {
                    logLevelString = ConfigurationManager.AppSettings["LogLevel"];
                }
                if (logLevelString == null)
                {
                    return false;
                }

				// Get loglevel
				TraceLevel logLevel;
                switch (logLevelString)
                {
                    default:
                    case "Off":
						logLevel = TraceLevel.Off;
						break;
                    case "Error":
						logLevel = TraceLevel.Error;
						break;
                    case "Warning":
						logLevel = TraceLevel.Warning;
						break;
					case "Info":
						logLevel = TraceLevel.Info;
						break;
					case "Verbose":
						logLevel = TraceLevel.Verbose;
						break;
                }

				bool enabled = (int)level <= (int)logLevel;
				return enabled;
            }
            else
            {
                return false;
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="level"></param>
		/// <param name="method"></param>
		/// <param name="logString"></param>
		[Conditional("TRACE")]
		public void AppendString(TraceLevel level, MethodBase method, string logString)
        {
			string moduleName = Assembly.GetCallingAssembly().GetName().Name;
			if (Enabled(level, moduleName, method.DeclaringType.Name))
			{
				AppendStrings(level, moduleName, method.DeclaringType.Name, method.Name, new string[] { logString }, false);
			}
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="level"></param>
		/// <param name="method"></param>
		/// <param name="logStrings"></param>
		[Conditional("TRACE")]
		public void AppendStrings(TraceLevel level, MethodBase method, string[] logStrings)
		{
			string moduleName = Assembly.GetCallingAssembly().GetName().Name;
			if (Enabled(level, moduleName, method.DeclaringType.Name))
			{
				AppendStrings(level, moduleName, method.DeclaringType.Name, method.Name, logStrings, false);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="level"></param>
		/// <param name="method"></param>
		/// <param name="logException"></param>
		[Conditional("TRACE")]
		public void AppendException(TraceLevel level, MethodBase method, Exception logException)
		{
			if (!Enabled(level, method.DeclaringType.Name))
			{
				return;
			}

			char[] trimChars = new char[] { '\r', '\n' };

			List<string> logStrings = new List<string>();
			logStrings.Add(logException.GetType().Name + " in thread '" + Thread.CurrentThread.Name + "'");
			logStrings.Add(logException.Message);

			Regex stackTrace = new Regex(@"^.*$", RegexOptions.Multiline);
			MatchCollection stackFrames = stackTrace.Matches(logException.StackTrace);
			foreach (Match stackFrame in stackFrames)
			{
				string value = stackFrame.Value.TrimEnd(trimChars);
				if (value.Length > 0)
				{
					logStrings.Add(value);
				}
			}

			string moduleName = Assembly.GetCallingAssembly().GetName().Name;
			AppendStrings(level, moduleName, method.DeclaringType.Name, method.Name, logStrings.ToArray(), true);
		}

		private void AppendStrings(TraceLevel level, string moduleName, string className, string methodName, string[] logStrings, bool isException)
		{
			lock (this)
			{
				try
				{
					AssertLogSize();
					OpenLog();
					byte[] buffer;
					TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, Environment.TickCount);
					string dateTime = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.") +
						timeSpan.Milliseconds.ToString("000") + "] ";

					string traceLevel;
					switch (level)
					{
						case TraceLevel.Error:
							traceLevel = "Error  : ";
							break;
						case TraceLevel.Warning:
							traceLevel = "Warning: ";
							break;
						case TraceLevel.Info:
							traceLevel = "Info   : ";
							break;
						case TraceLevel.Verbose:
							traceLevel = "Verbose: ";
							break;
						default:
							traceLevel = "???    : ";
							break;
					}

					string classMethod;
					if (className != null && methodName != null)
					{
						classMethod = "[" + className + "." + methodName + "] ";
					}
					else if (className != null)
					{
						classMethod = "[" + className + "] ";
					}
					else if (methodName != null)
					{
						classMethod = "[" + methodName + "] ";
					}
					else
					{
						classMethod = string.Empty;
					}

					buffer = Encoding.UTF8.GetBytes(dateTime + traceLevel + classMethod + logStrings[0] + "\r\n");
					logFile.Write(buffer, 0, buffer.Length);
					logFile.Flush();

					string padding = "".PadRight(dateTime.Length + traceLevel.Length + classMethod.Length);
					for (int n = 1; n < logStrings.Length; n++)
					{
						buffer = Encoding.UTF8.GetBytes(padding + logStrings[n] + "\r\n");
						logFile.Write(buffer, 0, buffer.Length);
						logFile.Flush();
					}
				}
				catch (ObjectDisposedException)
				{
					// Some class attempted to log during finalization. Not allowed. Ignore.
				}
				catch
				{
					throw new ApplicationException("Failed when writing to log file.");
				}
			}
		}

		/// <summary>
		/// WARNING! Should only be called from a locked state!
		/// </summary>
		private void AssertLogSize()
		{
			bool logLimitReached = false;
			if (logFile == null)
			{
				// Log file not opened
				FileInfo fileInfo = new FileInfo(fullPath);
				if (fileInfo.Exists && fileInfo.Length > maxFileSize)
				{
					logLimitReached = true;
				}
			}
			else
			{
				if (logFile.Position > maxFileSize)
				{
					CloseLog();
					logLimitReached = true;
				}
			}

			if (logLimitReached)
			{
				string oldFullPath = fullPath + ".old";
				if (File.Exists(oldFullPath))
				{
					File.Delete(oldFullPath);
				}
				File.Move(fullPath, oldFullPath);
			}
		}

		/// <summary>
		/// WARNING! Should only be called from a locked state!
		/// </summary>
        private void OpenLog()
        {
            if (logFile == null)
            {
                logFile = File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                logFile.Seek(0, SeekOrigin.End);
            }

            timer.Change(10000, Timeout.Infinite);
        }

		/// <summary>
		/// WARNING! Should only be called from a locked state!
		/// </summary>
		private void CloseLog()
        {
            if (logFile != null)
            {
                logFile.Close();
                logFile = null;
            }
        }

        private void TimerCallback(object state)
        {
            lock (this)
            {
                CloseLog();
            }
        }
    }
}