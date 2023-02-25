using System.Collections.Generic;
using UnityEngine;
using System;

public class Keyboard : BuiltinChip
{

    public List<string> chars = new List<string>();
    void Update()
    {
        print(Input.anyKey);
        if (Input.anyKey)
        {
            if (Input.inputString?.ToCharArray()?.Length > 0)
            {
                chars = new List<string>();
                char tmp = Input.inputString.ToCharArray()[0];
                int temp = (int)tmp;

                string binary = Convert.ToString(temp, 2);

                if (binary.Length < 8)
                {
                    for (int i = 8 - binary.Length; i > 0; i--)
                    {
                        binary = "0" + binary;
                    }
                }

                for (int i = 0; i < 8; i++)
                {
                    chars.Add(Convert.ToString(binary[i]));
                }

                for (int i = 0; i < chars.Count; i++)
                {
                    uint outputSignal = uint.Parse(chars[i]);
                    outputPins[i].ReceiveSignal(outputSignal);
                }
            }
        }

        else
        {
            for (int i = 0; i < 8; i++)
            {
                uint outputSignal = 0;
                outputPins[i].ReceiveSignal(outputSignal);
            }
        }
    }
}