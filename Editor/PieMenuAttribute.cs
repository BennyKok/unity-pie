using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;

namespace BennyKok.Pie.Editor
{
    [AttributeUsage(System.AttributeTargets.Method)]
    public class PieMenuAttribute : Attribute
    {
        public string path;
        public PieMenuAttribute() { }
        public PieMenuAttribute(string path)
        {
            this.path = path;
        }
    }
}