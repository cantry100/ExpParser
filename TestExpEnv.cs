/*
Copyright(C) 2022 yly(cantry100@163.com) - All Rights Reserved
示例
author：尧
*/
using ExpParseV;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ExpParseV.ExpIdentifierProvider;

public class TestExpEnv : MonoBehaviour
{
    [Multiline]
    public string sourceCode = "";

    ExpEnv _expEnv = new ExpEnv();

    // Start is called before the first frame update
    void Start()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        _expEnv.ShowAllLog(true);

        ExpIdentifierProvider identifierProvider1 = new ExpIdentifierProvider();
        ExpIdentifierProvider identifierProvider2 = new ExpIdentifierProvider();

        identifierProvider1.RegVarGetter("ctrl", (System.Object userData, ref TValue returnValue) => {
#if EXPV_DEBUG
                sLogger.LogFormat("==============ctrl00");
#endif
            returnValue.SetFloat(1);
        });

        identifierProvider2.RegVarGetter("ctrl", (System.Object userData, ref TValue returnValue) => {
#if EXPV_DEBUG
                sLogger.LogFormat("==============ctrl11");
#endif
            returnValue.SetFloat(100);
        });
        
        TValue resultTv = _expEnv.RunExp(sourceCode, identifierProvider1);
        sw.Stop();
        Debug.LogFormat("=============expr result tv type={0} value={1}", resultTv.type, resultTv.value);
        sw.Reset();
        sw.Start();

        return;

        byte[][] byteCodes = _expEnv.ParseExps(sourceCode);

        sw.Stop();
        Debug.LogFormat("=============expr parseExp usedtime={0} count={1}", sw.ElapsedMilliseconds / 1000f, byteCodes.Length);
        sw.Reset();
        sw.Start();
        
        for (int i = 0; i < byteCodes.Length; i++)
        {
            resultTv = _expEnv.RunExp(byteCodes[i], identifierProvider1);
            sw.Stop();
            Debug.LogFormat("=============expr{0} result tv type={1} value={2} usedtime={3}", i, resultTv.type, resultTv.value, sw.ElapsedMilliseconds / 1000f);
            sw.Reset();
            sw.Start();
        }

        return;

        byte[] byteCode = _expEnv.ParseExp(sourceCode);
        
        sw.Stop();
        Debug.LogFormat("=============expr parseExp usedtime={0}", sw.ElapsedMilliseconds / 1000f);
        sw.Reset();
        sw.Start();
        
        resultTv = _expEnv.RunExp(byteCode, identifierProvider1);
        sw.Stop();
        Debug.LogFormat("=============expr runByteCode00 usedtime={0}", sw.ElapsedMilliseconds / 1000f);
        sw.Reset();
        sw.Start();

        Debug.LogFormat("=============expr result00 tv type={0} value={1}", resultTv.type, resultTv.value);
        
        resultTv = _expEnv.RunExp(byteCode, identifierProvider1);

        sw.Stop();
        Debug.LogFormat("=============expr runByteCode11 usedtime={0}", sw.ElapsedMilliseconds / 1000f);
        sw.Reset();
        sw.Start();

        Debug.LogFormat("=============expr result11 tv type={0} value={1}", resultTv.type, resultTv.value);
        /*
        for (int i = 0; i < 10000; i++)
        {
            resultTv = _expEnv.RunExp(byteCode, identifierProvider2);
        }
        
        sw.Stop();
        Debug.LogFormat("=============expr runByteCode22 usedtime={0}", sw.ElapsedMilliseconds / 1000f);
        sw.Reset();
        sw.Start();

        Debug.LogFormat("=============expr result22 tv type={0} value={1}", resultTv.type, resultTv.value);

        resultTv = _expEnv.RunExp(byteCode, identifierProvider2);
        sw.Stop();
        Debug.LogFormat("=============expr runByteCode33 usedtime={0}", sw.ElapsedMilliseconds / 1000f);
        sw.Reset();
        sw.Start();

        Debug.LogFormat("=============expr result33 tv type={0} value={1}", resultTv.type, resultTv.value);
        */
    }
}
