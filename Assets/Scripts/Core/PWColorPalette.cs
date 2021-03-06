﻿using System.Collections.Generic;
using UnityEngine;
using System;
using PW.Core;

public static class PWColorPalette  {

	public static Color		orange = HexToColor(0xFFA500);

	static Dictionary< string, Color > colors = new Dictionary< string, Color >{
		{"blueNode", HexToColor(0x4864ef)},
		{"greenNode", HexToColor(0x30b030)},
		{"yellowNode", HexToColor(0xe8ed55)},
		{"orangeNode", HexToColor(0xff9e4f)},
		{"redNode", HexToColor(0xde4747)},
		{"cyanNode", HexToColor(0x55efea)},
		{"purpleNode", HexToColor(0x814bed)},
		{"pinkNode", HexToColor(0xf659d7)},
		{"greyNode", HexToColor(0x646464)},
		{"whiteNode", HexToColor(0xFFFFFF)},
		{"selectedNode", HexToColor(0x101868)},
		{"borderNode", HexToColor(0x777777)},
		{"blueAnchor", HexToColor(0x4864ef)},
		{"greenAnchor", HexToColor(0x30b030)},
		{"yellowAnchor", HexToColor(0xe8ed55)},
		{"orangeAnchor", HexToColor(0xff9e4f)},
		{"redAnchor", HexToColor(0xde4747)},
		{"cyanAnchor", HexToColor(0x55efea)},
		{"purpleAnchor", HexToColor(0x814bed)},
		{"pinkAnchor", HexToColor(0xf659d7)},
		{"greyAnchor", HexToColor(0x646464)},
		{"whiteAnchor", HexToColor(0xFFFFFF)},
		{"selected", HexToColor(0x101868)},
		{"defaultAnchor", new Color(.75f, .75f, .75f, 1)},
		{"transparentBackground", new Color(.75f, .75f, .75f, .5f)}
	};

	static bool			IsType(Type t, params Type[] types)
	{
		foreach (var type in types)
			if (t == type || t.IsSubclassOf(type))
				return true;
		return false;
	}
	
	public static Color GetAnchorColorByType(Type t)
	{
		if (IsType(t, typeof(int), typeof(float), typeof(Vector2), typeof(Vector3),
			typeof(Vector4), typeof(Texture2D), typeof(Mesh), typeof(GameObject),
			typeof(Material), typeof(Color), typeof(BiomeSurfaceMaps)))
			return GetColor("redAnchor");
		else if (IsType(t, typeof(ChunkData)))
			return GetColor("blueAnchor");
		else if (IsType(t, typeof(BiomeData), typeof(BiomeTerrain), typeof(BiomeSurfaces)))
			return GetColor("purpleAnchor");
		else if (IsType(t, typeof(Biome)))
			return GetColor("cyanAnchor");
		else if (IsType(t, typeof(Sampler)))
			return GetColor("greenAnchor");
		else if (IsType(t, typeof(PWValues), typeof(PWValue)))
			return GetColor("whiteAnchor");
		else
			return GetColor("defaultAnchor");
	}


	public static Color	GetColor(string key)
	{
		return colors[key];
	}
	
	public static Color	HexToColor(int color, bool missingAlpha = true)
	{
		byte alpha = (missingAlpha) ? (byte)0xFF : (byte)((color >> 24) & 0xFF);
		
		return new Color32(
			(byte)((color >> 16) & 0xFF),
			(byte)((color >>  8) & 0xFF),
			(byte)((color >>  0) & 0xFF),
			alpha);
	}

	public static int ColorToHex(Color c, bool alpha)
	{
		byte	a = (alpha) ? (byte)(c.a * 255) : (byte)0;
		byte	r = (byte)(c.r * 255);
		byte	g = (byte)(c.g * 255);
		byte	b = (byte)(c.b * 255);

		return ((a << 24) | (r << 16) | (g << 8) | (b));
	}

	public static void ColorToByte(Color c, out byte r, out byte g, out byte b, out byte a)
	{
		a = (byte)(c.a * 255);
		r = (byte)(c.r * 255);
		g = (byte)(c.g * 255);
		b = (byte)(c.b * 255);
	}

	public static Color ByteToColor(byte r, byte g, byte b, byte a)
	{
		return new Color32(r, g, b, a);
	}

	public static Texture2D ColorToTexture(Color c)
	{
		Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		tex.SetPixel(0, 0, c);
		tex.Apply();
		return tex;
	}
}
