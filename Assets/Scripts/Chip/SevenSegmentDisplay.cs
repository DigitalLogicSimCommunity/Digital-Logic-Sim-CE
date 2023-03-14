using System.Collections;
using System.Collections.Generic;
using UnityEngine;


	public class SevenSegmentDisplay : BuiltinChip
	{
		[SerializeField] MeshRenderer[] segments;
		public Color offCol;
		public Color onCol;
		public Color highlightCol;

		protected override void Start()
		{
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i].sharedMaterial = Material.Instantiate(segments[i].sharedMaterial);
		  }
		}
    protected override void Awake()
    {
        base.Awake();
    }
   
   protected override void ProcessOutput()
		{
			for (int i = 0; i < inputPins.Length  ; i++)
           {
				Color col = inputPins[i].State == 1 ? onCol : offCol;
				segments[i].sharedMaterial.color = col;
			}
		}
	}

