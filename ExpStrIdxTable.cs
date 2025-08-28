/*
Copyright(C) 2022 yly(cantry100@163.com) - All Rights Reserved
字符串索引表，注册/获取字符串索引
author：尧
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ExpParseV.ExpByteCodeVM;
using static ExpParseV.ExpLogger;

namespace ExpParseV
{
    public class ExpStrIdxTable
    {
        public const int ID_MAX = TValue.STR_PID_MAX;
        public const int STR_IDX_MAX = TValue.STR_IDX_MAX;

        static int sId = TValue.STR_PID_NONE;

        static int NextId
        {
            get
            {
                if (sId == ID_MAX)
                {
                    throw new Exception(string.Format("NextId Fail! sId==ID_MAX={0}", ID_MAX));
                }
                sId++;
                return sId;
            }
        }

        int _id = TValue.STR_PID_NONE;
        Dictionary<string, int> _regStrIdxDict = new Dictionary<string, int>();
        List<string> _regStrs = new List<string>();

        public int id
        {
            get
            {
                return _id;
            }
        }

        public ExpStrIdxTable()
        {
            _id = NextId;
        }
        
        /// <summary>
        /// 注册字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="getter"></param>
        public int RegStr(string str, bool isUnReg)
        {
            int index = -1;
            if (_regStrIdxDict.TryGetValue(str, out index))
            {
                if (isUnReg)
                {
                    _regStrs[index] = null;
                }
                else if(_regStrs[index] == null)
                {
                    _regStrs[index] = str;
                }
            }
            else if (!string.IsNullOrEmpty(str))
            {
                index = _regStrs.Count;
                if(index < STR_IDX_MAX)
                {
                    _regStrIdxDict.Add(str, index);
                    _regStrs.Add(str);
                }
                else
                {
                    throw new Exception(string.Format("RegStr {0} Fail! index={1} >= indexMax={2}", str, index, STR_IDX_MAX));
                }
            }

            return index;
        }
        
        /// <summary>
        /// 根据字符串获取其索引
        /// </summary>
        /// <param name="str"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool TryGetStrIdx(string str, out int index)
        {
            if (!_regStrIdxDict.TryGetValue(str, out index))
            {
                index = -1;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 根据字符串索引获取字符串
        /// </summary>
        /// <param name="index"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool TryGetStr(int index, out string str)
        {
            if (index >= 0 && index < _regStrs.Count)
            {
                str = _regStrs[index];
                return true;
            }
            str = string.Empty;
            return false;
        }
    }
}