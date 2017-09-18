﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PW.Core
{
	public static class PWAnchorUtils {
	
		static bool				AreAssignable(Type from, Type to)
		{
			if (to == typeof(object))
				return true;

			if (from == to)
				return true;
			
			if (from.IsAssignableFrom(to))
				return true;

			return false;
		}

		public static bool		AnchorAreAssignable(PWAnchor from, PWAnchor to, bool verbose = false)
		{
			if (from.anchorType == to.anchorType)
				return false;

			Type fromType = from.fieldType;
			Type toType = to.fieldType;
			
			if (verbose)
				Debug.Log(fromType.ToString() + " is assignable from " + to.fieldType.ToString() + ": " + AreAssignable(fromType, toType));
			return AreAssignable(fromType, toType);
		}
	
	}
}