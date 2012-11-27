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

namespace Nerdcave.Common.Xml
{
	public static class XmlSerializationHelper
	{		
        //list that mappes type-names to primitive (treating string as primitive type) types
		private static Dictionary<string, Type> primitiveTypes = new Dictionary<string, Type>() 
			{
				{"string", typeof(string)},
				{"int32", typeof(int)},				
                {"int64", typeof(long)},
                {"long", typeof(long)},         //backwards compatibility
				{"uint64", typeof(ulong)},
                {"ulong", typeof(ulong)},       //backwards compatibility
				{"double", typeof(double)},
				{"boolean", typeof(bool)},
				{"datetime", typeof(DateTime)}
			};


        //list of known Types and their names that implement ISerializable (loaded at startup)
		private static Dictionary<string, Type> registeredTypes = new Dictionary<string, Type>();


		static XmlSerializationHelper()
		{
			loadTypes();
		}


        /// <summary>
        /// Serializes the specified object to XML
        /// </summary>
        /// <param name="o">The object to serialize</param>
        /// <returns>Returns the XML representation of the specified object</returns>
		public static XElement SerializeToXmlObjectElement(object o)
		{
			if (o == null)
				return null;

			XElement result = new XElement("object");

            //item is knwon complex type
			if (o is ISerializable && registeredTypes.ContainsValue(o.GetType()))
			{
				result.Add(new XAttribute("type", registeredTypes.Keys.First(x=> registeredTypes[x] == o.GetType())));
				result.Add((o as ISerializable).Serialize());
				return result;
			}
            //item is list => serialize all items
			else if (o is IEnumerable<object>)
			{
				IEnumerable<object> list = (IEnumerable<object>)o;
				result.Add(new XAttribute("type", "list"));
				foreach (object item in list)
				{
					result.Add(SerializeToXmlObjectElement(item));
				}

				return result;
			}
            //item is primitive type
			else if (primitiveTypes.ContainsValue(o.GetType()))
			{
				Type t = o.GetType();

				result.Add(new XAttribute("type", primitiveTypes.Keys.Where(x => primitiveTypes[x] == t).First()));
				
                if (t == typeof(double))				
					result.Value = t.ToString().Replace(',', '.');				
                else if (t == typeof(DateTime))				
                    result.Value = ((DateTime)o).ToString("o");				
				else				
					result.Value = o.ToString();
				
				return result;

			}
            //type not supported
			else
			{
				throw new TypeNotSupportedException("Type {0} is not a supported serialization type or does not implement ISerializable<T>");
			}

		}


        /// <summary>
        /// Deserializes a object's XML representation into a object
        /// </summary>
        /// <param name="xml">The XML to deserialize</param>
        /// <returns>Retunrs a object deserailzed from XML</returns>
        /// <exception cref="TypeNotSupportedException">The specified XML is a unknown type and could not be deserialized</exception>
		public static object DeserializeFromXMLObjectElement(XElement xml)
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
			if (primitiveTypes.ContainsKey(xml.Attribute("type").Value))
			{
				switch (typeName)
				{
					case "string":
						return xml.Value;
					case "int32":
						return int.Parse(xml.Value);
                    case "ulong":                           //backwards compatibility
                    case "uint64":
						return ulong.Parse(xml.Value);
					case "int64":
                    case "long":                            //backwards compatibility
						return long.Parse(xml.Value);
					case "double":
						return double.Parse(xml.Value);
					case "boolean":
						return bool.Parse(xml.Value);
					case "datetime":
						return DateTime.Parse(xml.Value, CultureInfo.CreateSpecificCulture("en-gb"));
					default:
						throw new NotImplementedException("Unimplemented case in switch statement.");
				}

			}
            //check if item is list of object => deserialize all items if true
			else if (typeName == "list")
			{
                var objects = xml.Elements("object").Select(x => DeserializeFromXMLObjectElement(x));
                return objects.ToList<object>();
			}
            //check if item is known complex type (implements ISerializeable)
			else if (registeredTypes.ContainsKey(typeName))
			{
				Type type = registeredTypes[typeName];
				return ((ISerializable)Activator.CreateInstance(type)).Deserialize(xml.Elements().First());
			}
            //type is not supported
			else
			{
				throw new TypeNotSupportedException("XmlSerializationHelper.DeserializeFromXMLObjectElement(): Type name {0} unknown.", typeName);
			}

		}


        /// <summary>
        /// Loads all types from all assemblies that implement <see cref="Nerdcave.Common.Xml.ISerializable"/> and 
        /// saves them ti the registeredTypes dictionary
        /// </summary>
		private static void loadTypes()
		{

            if (Assembly.GetEntryAssembly() == null)
                return;

            //Get a list of all assemblies referenced by the entry assembly and add the entry-assembly itself to the list as well
            IEnumerable<Assembly> assemblies = Assembly.GetEntryAssembly()
                                                    .GetReferencedAssemblies().Select(x => Assembly.Load(x))
                                                    .Union(new Assembly[] { Assembly.GetEntryAssembly() })
                                                .ToList<Assembly>();

            //iterate through all types of all loaded assemblies
			foreach (Assembly a in assemblies)
			{
				foreach (Type t in a.GetTypes())
				{
                    //check if type implements ISerializable
					if (t.GetInterfaces().Contains(typeof(ISerializable)))
					{
                        //try to get the type's aml type-name and add it to the list of registered types
                        try
                        {
                            var attributes = t.GetCustomAttributes(typeof(XmlTypeNameAttribute), false);
                            string name = attributes.Any() ? ((XmlTypeNameAttribute)attributes.First()).Name : null;

                            if(!String.IsNullOrEmpty(name))
                                registeredTypes.Add(name.ToLower(), t);
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }						
					}
				}
			}
		}

	}



    /// <summary>
    /// Exception that will be thrown by methods of <see cref="XmlSerializationHelper"/> if a unsupported type is encoutnerd
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
