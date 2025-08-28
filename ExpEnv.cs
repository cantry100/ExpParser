/*
Copyright(C) 2022 yly(cantry100@163.com) - All Rights Reserved
表达式环境，提供对外接口
author：尧
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExpParseV
{
    public class ExpEnv
    {
        public const string TAG = "ExpEnv";
        
        public static ExpLogger sLogger = new ExpLogger(TAG);

        ExpParser _expParser = new ExpParser();
        ExpByteCodeVM _expByteCodeVM = new ExpByteCodeVM();

        /// <summary>
        /// 获取语法分析器
        /// </summary>
        public ExpParser expParser
        {
            get
            {
                return _expParser;
            }
        }

        /// <summary>
        /// 获取byteCode虚拟机
        /// </summary>
        public ExpByteCodeVM expByteCodeVM
        {
            get
            {
                return _expByteCodeVM;
            }
        }
        
        public ExpEnv()
        {
            
        }

        public void ShowAllLog(bool isShow)
        {
            ExpLogger.LogLevel logLevel = ExpLogger.LogLevel.All;
            if (!isShow)
            {
                logLevel = ExpLogger.LogLevel.Cull;
            }
            
            ExpLexer.sLogger.logLevel = logLevel;
            ExpParser.sLogger.logLevel = logLevel;
            ExpByteCodeVM.sLogger.logLevel = logLevel;
            ExpIdentifierProvider.sLogger.logLevel = logLevel;
        }

        /// <summary>
        /// 解析表达式源码
        /// 返回：表达式byteCode字节流
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        public byte[] ParseExp(string sourceCode)
        {
            return _expParser.ParseExp(sourceCode);
        }

        /// <summary>
        /// 解析表达式源码(支持多个表达式之间用逗号,分隔)
        /// 返回：一个或多个表达式byteCode字节流
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        public byte[][] ParseExps(string sourceCode)
        {
            return _expParser.ParseExps(sourceCode);
        }

        /// <summary>
        /// 解析并运行表达式源码
        /// （如果需要高频调用该接口，建议先调用ParseExp缓存返回的表达式byteCode字节流，然后通过调用RunExp运行缓存的表达式byteCode字节流，可提高性能和避免不必要的gc）
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <param name="identProvider">标识符提供者</param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public TValue RunExp(string sourceCode, ExpIdentifierProvider identProvider = null, System.Object userData = null)
        {
            TValue resultTv = _expByteCodeVM.RunByteCode(_expParser.ParseExp(sourceCode), identProvider, userData);
            return resultTv;
        }

        /// <summary>
        /// 运行表达式byteCode字节流
        /// </summary>
        /// <param name="byteCode"></param>
        /// <param name="identProvider">标识符提供者</param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public TValue RunExp(byte[] byteCode, ExpIdentifierProvider identProvider = null, System.Object userData = null)
        {
            TValue resultTv = _expByteCodeVM.RunByteCode(byteCode, identProvider, userData);
            return resultTv;
        }

        /// <summary>
        /// 计算表达式源码的布尔值
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <param name="identProvider"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public bool EvalBool(string sourceCode, ExpIdentifierProvider identProvider = null, System.Object userData = null)
        {
            TValue resultTv = RunExp(sourceCode, identProvider, userData);
            return resultTv.ToBool();
        }

        /// <summary>
        /// 计算表达式byteCode字节流的布尔值
        /// </summary>
        /// <param name="byteCode"></param>
        /// <param name="identProvider"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public bool EvalBool(byte[] byteCode, ExpIdentifierProvider identProvider = null, System.Object userData = null)
        {
            TValue resultTv = _expByteCodeVM.RunByteCode(byteCode, identProvider, userData);
            return resultTv.ToBool();
        }

        /// <summary>
        /// 计算表达式源码的整数值
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <param name="identProvider"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public int EvalInt(string sourceCode, ExpIdentifierProvider identProvider = null, System.Object userData = null)
        {
            TValue resultTv = RunExp(sourceCode, identProvider, userData);
            return resultTv.ToInt();
        }

        /// <summary>
        /// 计算表达式byteCode字节流的整数值
        /// </summary>
        /// <param name="byteCode"></param>
        /// <param name="identProvider"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public int EvalInt(byte[] byteCode, ExpIdentifierProvider identProvider = null, System.Object userData = null)
        {
            TValue resultTv = _expByteCodeVM.RunByteCode(byteCode, identProvider, userData);
            return resultTv.ToInt();
        }

        /// <summary>
        /// 计算表达式源码的单精度浮点数值
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <param name="identProvider"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public float EvalFloat(string sourceCode, ExpIdentifierProvider identProvider = null, System.Object userData = null)
        {
            TValue resultTv = RunExp(sourceCode, identProvider, userData);
            return resultTv.ToFloat();
        }

        /// <summary>
        /// 计算表达式byteCode字节流的单精度浮点数值
        /// </summary>
        /// <param name="byteCode"></param>
        /// <param name="identProvider"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public float EvalFloat(byte[] byteCode, ExpIdentifierProvider identProvider = null, System.Object userData = null)
        {
            TValue resultTv = _expByteCodeVM.RunByteCode(byteCode, identProvider, userData);
            return resultTv.ToFloat();
        }

        /// <summary>
        /// 计算表达式源码的双精度浮点数值
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <param name="identProvider"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public double EvalDouble(string sourceCode, ExpIdentifierProvider identProvider = null, System.Object userData = null)
        {
            TValue resultTv = RunExp(sourceCode, identProvider, userData);
            return resultTv.value;
        }

        /// <summary>
        /// 计算表达式byteCode字节流的双精度浮点数值
        /// </summary>
        /// <param name="byteCode"></param>
        /// <param name="identProvider"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public double EvalDouble(byte[] byteCode, ExpIdentifierProvider identProvider = null, System.Object userData = null)
        {
            TValue resultTv = _expByteCodeVM.RunByteCode(byteCode, identProvider, userData);
            return resultTv.value;
        }

        /// <summary>
        /// 计算表达式源码的字符串值
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <param name="identProvider"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public string EvalString(string sourceCode, ExpIdentifierProvider identProvider = null, System.Object userData = null)
        {
            TValue resultTv = RunExp(sourceCode, identProvider, userData);
            if(resultTv.type == ValueType.VT_STR)
            {
                return resultTv.valStr;
            }
            return resultTv.value.ToString();
        }

        /// <summary>
        /// 计算表达式byteCode字节流的字符串值
        /// </summary>
        /// <param name="byteCode"></param>
        /// <param name="identProvider"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public string EvalString(byte[] byteCode, ExpIdentifierProvider identProvider = null, System.Object userData = null)
        {
            TValue resultTv = _expByteCodeVM.RunByteCode(byteCode, identProvider, userData);
            if (resultTv.type == ValueType.VT_STR)
            {
                return resultTv.valStr;
            }
            return resultTv.value.ToString();
        }
    }
}