/*
Copyright(C) 2022 yly(cantry100@163.com) - All Rights Reserved
log相关
author：尧
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExpParseV
{
    public class ExpLogger
    {
        public enum LogLevel
        {
            /// <summary>
            /// 显示所有log
            /// </summary>
            All,
            Log,
            Warn,
            Error,
            Exception,
            /// <summary>
            /// 不显示log
            /// </summary>
            Cull,
        }

        public const string WARN_HEADER = "[WARN]";
        public const string ERROR_HEADER = "[ERROR]";
        public const string EXCEPTION_HEADER = "[EXCEPTION]";

        static StringBuilder SharedBuff = new StringBuilder();

        string _tag = string.Empty;
        string _tagHeader = string.Empty;
#if EXPV_DEBUG
        LogLevel _logLevel = LogLevel.All;
#else
        LogLevel _logLevel = LogLevel.Cull;
#endif

        public string tag
        {
            get
            {
                return _tag;
            }
            set
            {
                _tag = value;
                if (string.IsNullOrEmpty(_tag))
                {
                    _tagHeader = string.Empty;
                }
                else
                {
                    _tagHeader = "[" + _tag + "]";
                }
            }
        }

        public LogLevel logLevel
        {
            get
            {
                return _logLevel;
            }
            set
            {
                _logLevel = value;
            }
        }

        public ExpLogger(string tag)
        {
            this.tag = tag;
        }

        public ExpLogger(string tag, LogLevel logLevel)
        {
            this.tag = tag;
            _logLevel = logLevel;
        }

        string _AddLogHeader(LogLevel logLevel, string msg)
        {
            SharedBuff.Clear();
            bool hasHeader = false;
            if (!string.IsNullOrEmpty(_tagHeader))
            {
                SharedBuff.Append(_tagHeader);
                hasHeader = true;
            }
            
            switch(logLevel)
            {
                case LogLevel.Warn:
                    SharedBuff.Append(WARN_HEADER);
                    hasHeader = true;
                    break;
                case LogLevel.Error:
                    SharedBuff.Append(ERROR_HEADER);
                    hasHeader = true;
                    break;
                case LogLevel.Exception:
                    SharedBuff.Append(EXCEPTION_HEADER);
                    hasHeader = true;
                    break;
            }

            if(hasHeader)
            {
                if (!string.IsNullOrEmpty(msg))
                {
                    SharedBuff.Append(msg);
                }

                return SharedBuff.ToString();
            }

            return msg;
        }

        public void Log(string msg)
        {
            if(_logLevel > LogLevel.Log)
            {
                return;
            }

            msg = _AddLogHeader(LogLevel.Log, msg);
            Debug.Log(msg);
        }

        public void LogFormat(string msg, params object[] args)
        {
            if (_logLevel > LogLevel.Log)
            {
                return;
            }

            msg = _AddLogHeader(LogLevel.Log, msg);
            Debug.LogFormat(msg, args);
        }

        public void LogWarn(string msg)
        {
            if (_logLevel > LogLevel.Warn)
            {
                return;
            }

            msg = _AddLogHeader(LogLevel.Warn, msg);
            Debug.LogWarning(msg);
        }

        public void LogWarnFormat(string msg, params object[] args)
        {
            if (_logLevel > LogLevel.Warn)
            {
                return;
            }

            msg = _AddLogHeader(LogLevel.Warn, msg);
            Debug.LogWarningFormat(msg, args);
        }
        
        public void LogError(string msg)
        {
            if (_logLevel > LogLevel.Error)
            {
                return;
            }

            msg = _AddLogHeader(LogLevel.Error, msg);
            Debug.LogError(msg);
        }

        public void LogErrorFormat(string msg, params object[] args)
        {
            if (_logLevel > LogLevel.Error)
            {
                return;
            }

            msg = _AddLogHeader(LogLevel.Error, msg);
            Debug.LogErrorFormat(msg, args);
        }
        
        public void LogException(string msg)
        {
            msg = _AddLogHeader(LogLevel.Exception, msg);
            throw new Exception(msg);
        }

        public void LogExceptionFormat(string msg, params object[] args)
        {
            msg = _AddLogHeader(LogLevel.Exception, msg);
            throw new Exception(string.Format(msg, args));
        }
    }
}