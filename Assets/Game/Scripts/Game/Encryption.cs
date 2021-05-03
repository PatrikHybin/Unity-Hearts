using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class Encryption
{
    public static string EncryptDecrypt(string inputText) {

        StringBuilder outSb = new StringBuilder(inputText.Length);
        for (int i = 0; i < inputText.Length; i++) {
            char ch = (char)(inputText[i] ^ Settings.key);
            outSb.Append(ch);
        }

        return outSb.ToString();
    }
}
