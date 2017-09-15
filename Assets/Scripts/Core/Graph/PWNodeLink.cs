﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW;
using PW.Core;

namespace PW.Core
{
	[System.Serializable]
	public class PWNodeLink
	{
		//GUID to identify the node in the LinkTable
		public string			GUID;
	
		//ColorPalette of the link
		public PWColorPalette	colorPalette;
		//link type
		public PWLinkType		type;
		//link hightlight mode 
		public PWLinkHighlight	highlight;
		//is selected ?
		public bool				selected;
	
		//anchor where the link is connected:
		[System.NonSerialized]
		public PWAnchor			fromAnchor;
		[System.NonSerialized]
		public PWAnchor			toAnchor;
		[System.NonSerialized]
		public PWNode			fromNode;
		[System.NonSerialized]
		public PWNode			toNode;

		//called once (when link is created only)
		public void Initialize()
		{
			GUID = System.Guid.NewGuid().ToString();
		}

		//this function will be called twiced, from the two linked anchors
		public void OnAfterDeserialize()
		{
			if (fromAnchor != null)
				fromNode = fromAnchor.nodeRef;
			if (toNode != null)
				toNode = toAnchor.nodeRef;
		}
	
		override public string ToString()
		{
			return "link [" + GUID + "]";
		}
	}
}