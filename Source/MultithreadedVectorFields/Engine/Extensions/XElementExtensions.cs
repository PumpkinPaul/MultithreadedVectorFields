//using System;
//using System.Xml.Linq;
//using Microsoft.Xna.Framework;
//using PumpkinGames.Glitchangels.Util;

//namespace PumpkinGames.Glitchangels
//{
//    /// <summary>
//    /// Extension methos for XElement.
//    /// </summary>
//    public static class XElementExtensions
//    {        
//        public static bool GetAttributeBool(this XElement source, string name)
//        {
//            return GetAttributeBool(source, name, default(bool));
//        }

//        public static bool GetAttributeBool(this XElement source, string name, bool defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var attribute = source.Attribute(name);

//            if (attribute == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(attribute.Value);
//            return ConversionHelper.ToBool(value, defaultValue);
//        }

//        public static string GetAttributeString(this XElement source, string name)
//        {
//            return GetAttributeString(source, name, default(string));
//        }

//        public static string GetAttributeString(this XElement source, string name, string defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var attribute = source.Attribute(name);

//            if (attribute == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(attribute.Value);
//            return value;
//        }

//        public static byte GetAttributeByte(this XElement source, string name)
//        {
//            return GetAttributeByte(source, name, default(byte));
//        }

//        public static byte GetAttributeByte(this XElement source, string name, byte defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var attribute = source.Attribute(name);

//            if (attribute == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(attribute.Value);
//            return ConversionHelper.ToByte(value, defaultValue);
//        }

//        public static int GetAttributeInt32(this XElement source, string name)
//        {
//            return GetAttributeInt32(source, name, default(int));
//        }

//        public static int GetAttributeInt32(this XElement source, string name, int defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var attribute = source.Attribute(name);

//            if (attribute == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(attribute.Value);
//            return ConversionHelper.ToInt32(value, defaultValue);
//        }

//        public static float GetAttributeSingle(this XElement source, string name)
//        {
//                return GetAttributeSingle(source, name, default(float));
//        }

//        public static float GetAttributeSingle(this XElement source, string name, float defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var attribute = source.Attribute(name);

//            if (attribute == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(attribute.Value);
//            return ConversionHelper.ToSingle(value, defaultValue);
//        }

//        public static Range GetAttributeRange(this XElement source, string name)
//        {
//            var vector = GetAttributeVector2(source, name, Vector2.Zero);
//            return new Range(vector.X, vector.Y);
//        }

//        public static Vector2 GetAttributeVector2(this XElement source, string name)
//        {
//            return GetAttributeVector2(source, name, Vector2.Zero);
//        }

//        public static Vector2 GetAttributeVector2(this XElement source, string name, Vector2 defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var attribute = source.Attribute(name);

//            if (attribute == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(attribute.Value);
//            return ConversionHelper.ToVector2(value);
//        }

//        public static Vector4 GetAttributeVector4(this XElement source, string name)
//        {
//            return GetAttributeVector4(source, name, Vector4.Zero);
//        }

//        public static Vector4 GetAttributeVector4(this XElement source, string name, Vector4 defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var attribute = source.Attribute(name);

//            if (attribute == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(attribute.Value);
//            return ConversionHelper.ToVector4(value);
//        }

//        public static Box2 GetAttributeBox2(this XElement source, string name)
//        {
//            return GetAttributeBox2(source, name, Box2.Empty);
//        }

//        public static Box2 GetAttributeBox2(this XElement source, string name, Box2 defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var attribute = source.Attribute(name);

//            if (attribute == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(attribute.Value);
//            return ConversionHelper.ToBox2(value);
//        }

//        public static Rect GetAttributeRect(this XElement source, string name)
//        {
//            return GetAttributeRect(source, name, Rect.Zero);
//        }

//        public static Rect GetAttributeRect(this XElement source, string name, Rect defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var attribute = source.Attribute(name);

//            if (attribute == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(attribute.Value);
//            return ConversionHelper.ToRect(value);
//        }

//        public static Color GetAttributeColor(this XElement source, string name)
//        {
//            return GetAttributeColor(source, name, Color.White);
//        }

//        public static Color GetAttributeColor(this XElement source, string name, Color defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var attribute = source.Attribute(name);

//            if (attribute == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(attribute.Value);
//            return ConversionHelper.ToColor(value);
//        }

//        public static Color GetAttributePreMultipliedColor(this XElement source, string name)
//        {
//            return GetAttributePreMultipliedColor(source, name, Color.White);
//        }

//        public static Color GetAttributePreMultipliedColor(this XElement source, string name, Color defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var attribute = source.Attribute(name);

//            if (attribute == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(attribute.Value);
//            return ConversionHelper.ToPreMultipliedColor(value);
//        }

//        public static T GetAttributeEnum<T>(this XElement source, string name)
//        {
//            return GetAttributeEnum(source, name, default(T));
//        }

//        public static T GetAttributeEnum<T>(this XElement source, string name, T defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var attribute = source.Attribute(name);

//            if (attribute == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(attribute.Value);
//            return (T)Enum.Parse(typeof(T), value, false);
//        }

//        public static int GetValueInt32(this XElement source)
//        {
//            return GetElementInt32(source, null, default(int));
//        }

//        public static int GetValueInt32(this XElement source, int defaultValue)
//        {
//            return GetElementInt32(source, null, defaultValue);
//        }

//        public static int GetElementInt32(this XElement source)
//        {
//            return GetElementInt32(source, null, default(int));
//        }

//        public static int GetElementInt32(this XElement source, string name)
//        {
//            return GetElementInt32(source, name, default(int));
//        }

//        public static int GetElementInt32(this XElement source, string name, int defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var element = name == null ? source : source.Element(name);

//            if (element == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(element.Value);
//            return ConversionHelper.ToInt32(value, defaultValue);
//        }

//        public static bool GetElementBool(this XElement source, string name)
//        {
//            return GetElementBool(source, name, default(bool));
//        }

//        public static bool GetElementBool(this XElement source, string name, bool defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var element = source.Element(name);

//            if (element == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(element.Value);
//            return ConversionHelper.ToBool(value, defaultValue);
//        }

//        public static float GetElementSingle(this XElement source, string name)
//        {
//            return GetElementSingle(source, name, default(float));
//        }

//        public static float GetElementSingle(this XElement source, string name, float defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var element = source.Element(name);

//            if (element == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(element.Value);
//            return ConversionHelper.ToSingle(value, defaultValue);
//        }

//        public static string GetElementString(this XElement source, string name)
//        {
//            return GetElementString(source, name, default(string));
//        }

//        public static string GetElementString(this XElement source, string name, string defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var element = source.Element(name);

//            if (element == null)
//                return defaultValue;

//            return ScriptManager.Parse(element.Value);
//        }

//        public static Vector2 GetElementVector2(this XElement source, string name)
//        {
//            return GetElementVector2(source, name, default(Vector2));
//        }

//        public static Vector2 GetElementVector2(this XElement source, string name, Vector2 defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var element = source.Element(name);

//            if (element == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(element.Value);
//            return ConversionHelper.ToVector2(value);
//        }

//        public static Vector4 GetElementVector4(this XElement source, string name)
//        {
//            return GetElementVector4(source, name, default(Vector4));
//        }

//        public static Vector4 GetElementVector4(this XElement source, string name, Vector4 defaultValue)
//        {
//            if (source == null)
//                return defaultValue;

//            var element = source.Element(name);

//            if (element == null)
//                return defaultValue;

//            var value = ScriptManager.Parse(element.Value);
//            return ConversionHelper.ToVector4(value);
//        }
//    }
//}