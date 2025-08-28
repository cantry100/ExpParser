/*
Copyright(C) 2022 yly(cantry100@163.com) - All Rights Reserved
示例
author：尧
*/
using ExpParseV;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ExpParseV.ExpLexer;

public class TestExpLexer : MonoBehaviour
{
    [Multiline]
    public string sourceCode = "";

    ExpLexer _expLexer = new ExpLexer();
     
    // Start is called before the first frame update
    void Start()
    {
        if(true && false || true)
        {
            Debug.LogFormat("=========11");
        }
        _expLexer.SetSourceCode(sourceCode);

        TokenType tokenType = 0;
        while (tokenType != TokenType.TK_EOS)
        {
            tokenType = _expLexer.NextToken();
            Debug.LogFormat("=========tokenType={0} lex={1}", tokenType, _expLexer.tokenStr);
        }
    }
}
