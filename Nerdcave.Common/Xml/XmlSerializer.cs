/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

	Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
	Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using Nerdcave.Common.Extensions;
using System.Reflection;
using System.Collections;

namespace Nerdcave.Common.Xml
{
	public static class XmlSerializer
	{
		//list that mappes type-names to primitive (treating string as primitive type) types

		private static Dictionary<string, Type> knownTypes = new Dictionary<string, Type>() 
			{
				{"boolean", typeof(Boolean)},
				{"dateTime", typeof(DateTime)},
                {"string", typeof(string)},
				{"double", typeof(double)},
				{"int32", typeof(int)},				
				{"int64", typeof(long)},
				{"uint64", typeof(ulong)},
				{"guid", typeof(Guid)},
				{"list", typeof(IEnumerable)}
			};
		
		private static Dictionary<string, Func<XElement, object>> knownTypesParsers = new Dictionary<string, Func<XElement, object>>() 
			{
				{"boolean", parseBoolean},
				{"dateTime", parseDateTime},
				{"string", parseString},
                {"double", parseDouble},
				{"int32", parseInt32},				
				{"int64", parseInt64},
				{"uint64", parseUInt64},
				{"guid", parseGuid},
				{"list", parseList}
			};

		private static Dictionary<string, Func<object, string, object>> knownTypesSerializers = new Dictionary<string, Func<object, string, object>>() 
			{
				{"boolean", serializeBoolean},
				{"dateTime", serializeDateTime},
                {"string", serializeString},
				{"double", serializeDouble},
				{"int32", serializeInt32},				
				{"int64", serializeInt64},
				{"uint64", serializeUInt64},
				{"guid", serializeGuid},
				{"list", serializeList}
			};





		//list of known Types and their names that implement ISerializable (loaded at startup)
		private static Dictionary<string, Type> registeredTypes = new Dictionary<string, Type>();



		public static void RegisterType(Type type, string xmlName)
		{
			if (xmlName.IsNullOrEmpty())
				throw new ArgumentException("Value of xmlName may not be null or empty");

			if (!type.GetInterfaces().Contains(typeof(ISerializable)))
				throw new TypeNotSupportedException(String.Format("{0} does not implement ISerializable", type.Name));


			if (registeredTypes.ContainsKey(xmlName))
			{
				registeredTypes[xmlName] = type;
			}
			else
			{
				registeredTypes.Add(xmlName, type);
			}
		}


		/// <summary>
		/// Serializes the specified object to XML
		/// </summary>
		/// <param name="o">The object to serialize</param>
		/// <param name="ns">The xml element's namespace</param>
		/// <returns>Returns the XML representation of the specified object</returns>
		public static XElement Serialize(object o, string ns = "")
		{

			var elementName = "object";

			if (o == null)
				return null;

			object content;
			string name;

			if(o is ISerializable && registeredTypes.ContainsValue(o.GetType()))
			{
				name = registeredTypes.Keys.First(x => registeredTypes[x] == o.GetType());
				
				//serialize value
				content = (o as ISerializable).Serialize();
			}
			else if (knownTypes.ContainsValue(o.GetType()))
			{
				name = knownTypes.Keys.First(x => knownTypes[x] == o.GetType());

				//serialize value 
				content = knownTypesSerializers[name].Invoke(o, ns);
			}
			else
			{
				throw new TypeNotSupportedException("Type {0} is not a known type and does not implement ISerializable");
			}


			XElement result;
			if (ns.IsNullOrEmpty())
				result = new XElement(elementName, content);
			else
				result = new XElement(XName.Get(elementName, ns), content);


			result.Add(new XAttribute("type", name));


			return result;
		}


		/// <summary>
		/// Deserializes a object's XML representation into a object
		/// </summary>
		/// <param name="xml">The XML to deserialize</param>
		/// <returns>Retunrs a object deserailzed from XML</returns>
		/// <exception cref="TypeNotSupportedException">The specified XML is a unknown type and could not be deserialized</exception>
		public static object Deserialize(XElement xml)
		{
			//check if xml is valid
			if (xml == null)
				throw new NullReferenceException();

			//check if xml is "object" element
			if (xml.Name.LocalName != "object")
			{
				throw new TypeNotSupportedException();
			}

			//get the type the object will be deserialized to
			string typeName = xml.Attribute("type") == null ? null : xml.Attribute("type").Value.ToLower();

			//check validity of typeName
			if (typeName.IsNullOrEmpty())
				throw new TypeNotSupportedException();


			//check if item is primitive type => parse value if true
			if (knownTypesParsers.ContainsKey(xml.Attribute("type").Value))
			{
				return knownTypesParsers[xml.Attribute("type").Value].Invoke(xml);
			}
			//check if item is known complex type (implements ISerializeable)
			else if (registeredTypes.ContainsKey(typeName))
			{
				Type type = registeredTypes[typeName];
				ISerializable instance = (ISerializable)Activator.CreateInstance(type);
				instance.Deserialize(xml.Elements().First());

				return instance;
			}
			//type is not supported
			else
			{
				throw new TypeNotSupportedException("XmlSerializer.Deserialize(): Type {0} unknown.", typeName);
			}

		}

		public static T DeserializeAs<T>(XElement xml)
		{

			if (knownTypes.ContainsValue(typeof(T)))
			{
                var nameQuery = from item in knownTypes
                                where item.Value == typeof(T)
                                select item.Key;
				
                var name = nameQuery.First();

				object value = knownTypesParsers[name].Invoke(xml);

    			return (T)value;

			}
			else if (registeredTypes.ContainsValue(typeof(T)))
			{
                var nameQuery = from item in registeredTypes
                                where item.Value == typeof(T)
                                select item.Key;

				var name = nameQuery.First();

				ISerializable result = (ISerializable) Activator.CreateInstance(registeredTypes[name]);
				result.Deserialize(xml);

				return (T)result;
			}
			else
			{
				throw new TypeNotSupportedException("{0} is not suppoerted by XmlSerializer", typeof(T).Name);
			}
		}

		private static object parseBoolean(XElement xml)
		{
			bool outValue;

            if (bool.TryParse(xml.Value, out outValue))
                return outValue;
            else
                throw new ArgumentException();
		}

		private static object parseDateTime(XElement xml)
		{
			DateTime outValue;
			if (DateTime.TryParse(xml.Value, out outValue)) //, CultureInfo.CreateSpecificCulture("en-gb")
				return outValue;
			else
                throw new ArgumentException();
		}

		private static object parseDouble(XElement xml)
		{
			double outValue;

			if (double.TryParse(xml.Value, out outValue))
				return outValue;
			else
                throw new ArgumentException();
		}

		private static object parseInt32(XElement xml)
		{
			int outValue;

			if (int.TryParse(xml.Value, out outValue))
				return outValue;
			else
                throw new ArgumentException();
		}

		private static object parseInt64(XElement xml)
		{
			long outValue;

			if (long.TryParse(xml.Value, out outValue))
				return outValue;
			else
                throw new ArgumentException();
		}

		private static object parseUInt64(XElement xml)
		{
			ulong outValue;

			if (ulong.TryParse(xml.Value, out outValue))
				return outValue;
			else
                throw new ArgumentException();
		}

		private static object parseString(XElement xml)
		{
			if (xml.Value == null)
				return "";
			else
				return xml.Value;
		}

		private static object parseGuid(XElement xml)
		{
			Guid outValue;

			if (Guid.TryParse(xml.Value, out outValue))
				return outValue;
			else
                throw new ArgumentException();
		}

		private static object parseList(XElement xml)
		{
			return xml.Elements("object").Select(x => Deserialize(xml)).ToList<object>();
		}


		private static object serializeBoolean(object value, string ns)
		{
			if (value is bool)
				return value.ToString();
			else
				return null;
		}

		private static object serializeDateTime(object value, string ns)
		{
			if (value is DateTime)
				return ((DateTime)value).ToUniversalTime().ToString("o");
			else
				return null;
		}

		private static object serializeDouble(object value, string ns)
		{
			if (value is double)
				return value.ToString();
			else
				return null;
		}

		private static object serializeInt32(object value, string ns)
		{
			if (value is int)
				return value.ToString();
			else
				return null;
		}

		private static object serializeInt64(object value, string ns)
		{
			if (value is long)
				return value.ToString();
			else
				return null;
		}
		
		private static object serializeUInt64(object value, string ns)
		{
			if (value is ulong)
				return value.ToString();
			else
				return null;
		}

		private static object serializeString(object value, string ns)
		{
			if (value is string)
				return value;
			else
				return null;
		}
		
		private static object serializeGuid(object value, string ns)
		{
			if (value is Guid)
				return ((Guid)value).ToString();
			else
				return null;
		}

		private static object serializeList(object value, string ns)
		{
			if (value is IEnumerable)
			{
				List<XElement> result = new List<XElement>();

				foreach (var item in (value as IEnumerable))
				{
					var xItem =  Serialize(item, ns);

					if(xItem != null)
						result.Add(xItem);
				}

				return result;
			}
			else
			{
				return null;
			}
		}




	}

	/// <summary>
	/// Exception that will be thrown by methods of <see cref="XmlSerializer"/> if a unsupported type is encoutnerd
	/// </summary>
	public class TypeNotSupportedException: Exception
	{
		/// <summary>
		/// Initializes a new instance of the TypeNotSupportedException
		/// </summary>
		public TypeNotSupportedException()
			: base()
		{ }

		/// <summary>
		/// Initializes a new instance of the TypeNotSupportedException
		/// </summary>
		public TypeNotSupportedException(string message)
			: base(message)
		{ }

		/// <summary>
		/// Initializes a new instance of the TypeNotSupportedException
		/// </summary>
		public TypeNotSupportedException(string formatstring, params object[] args)
			: base(String.Format(formatstring, args))
		{ }

	}


}
