using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FolderNameValidator
{
    static string ValidChars = "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789()[]";
    public static string ValidateFolderName(string text, bool endEdit = false)
    {
        string validName = "";
        for (int i = 0; i < text.Length; i++)
            if (i < 12 && ValidChars.Contains(text[i].ToString()))
                validName += text[i];

        return endEdit ? validName.Trim() : validName.TrimStart();
    }
}
