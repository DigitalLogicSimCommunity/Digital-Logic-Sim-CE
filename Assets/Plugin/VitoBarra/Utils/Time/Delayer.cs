using System;
using UnityEngine;

namespace VitoBarra.Utils.T
{
    public class Delayer
    {
        private float SavedTime = 0f;
        private float Threshold = 0.1f;


        public Delayer(float threshold)
        {
            Threshold = threshold;
        }
        
        public void StartCount()
        {
            SavedTime = Time.time;
        }

        public bool IsDelayPassed => Time.time - SavedTime > Threshold;
    }
}