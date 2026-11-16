using System.Collections.Generic;
using UnityEngine;

// Ресурс (дерево) — гравець підбирає, просто підійшовши
public class Resource : MonoBehaviour
{
    public static readonly List<Resource> All = new List<Resource>();
    void OnEnable()  { All.Add(this); }
    void OnDisable() { All.Remove(this); }
}
