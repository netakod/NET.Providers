using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Diagnostics;

namespace Simple
{
    public static class XmlHelper
    {
        private const string dictionaryItemNodeName = "Item";
        private const string dictionaryKeyNodeName = "Key";
        private const string dictionaryValueNodeName = "Value";
        public const string DefaultGroupName = "Properties";

        public static string SerializeObject<T>(T value) where T : IXmlSerializable
        {
            MemoryStream memoryStream = new MemoryStream();
            XmlSerializer xmlSerializer = new XmlSerializer(value.GetType());
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlSerializer.Serialize(xmlTextWriter, value);
            memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
            
            return StringHelper.UTF8ByteArrayToString(memoryStream.ToArray());
        }

        public static T DeserializeObject<T>(string xml) where T : IXmlSerializable
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            MemoryStream memoryStream = new MemoryStream(StringHelper.StringToUTF8ByteArray(xml));
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            
            return (T)xmlSerializer.Deserialize(memoryStream);
        }

        public static void ReadDictionaryFromXml<TKey, TValue>(IDictionary<TKey, TValue> dictionary, XmlReader reader)
        {
            bool isEmpty = reader.IsEmptyElement;
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));

            reader.Read();

            if (isEmpty)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                TKey key ;
                if (reader.Name == dictionaryItemNodeName)
                {
                    reader.Read();
                    //Type keyType = Type.GetType(reader.GetAttribute("type"));
                    //if (keyType != null)
                    //{
                    reader.Read();
                    key = (TKey)keySerializer.Deserialize(reader);
                    reader.ReadEndElement();
                        
                    Type valueType = (reader.HasAttributes) ? Type.GetType(reader.GetAttribute("type")) : null;
                        
                    if (valueType != null)
                    {
                        reader.Read();
                        dictionary.Add(key, (TValue)new XmlSerializer(valueType).Deserialize(reader));
                        reader.ReadEndElement();
                    }
                    else
                    {
                        dictionary.Add(key, default(TValue));
                        reader.Skip();
                    }
                    //}
                    reader.ReadEndElement();
                    reader.MoveToContent();
                }
                else
				{
                    reader.ReadEndElement();
                    reader.MoveToContent();

                    //reader.Read();
				}
            }

            reader.ReadEndElement();
        }

        public static void WriteDictionaryToXml<TKey, TValue>(IDictionary<TKey, TValue> dictionary, XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            writer.WriteString("\r\n");

            foreach (var dictionaryItem in dictionary)
            {
                writer.WriteString("\r\n");
               
                //create <item>
                writer.WriteStartElement(dictionaryItemNodeName);
                writer.WriteString("\r\n    ");
                //create <key> under <item>
                writer.WriteStartElement(dictionaryKeyNodeName);
                //writer.WriteAttributeString(string.Empty, "type", string.Empty, dictionaryItem.Key.GetType().AssemblyQualifiedName);
                
                if (dictionaryItem.Key is string) 
                    writer.WriteString(dictionaryItem.Key.ToString());
                else
                    keySerializer.Serialize(writer, dictionaryItem.Key);

                //end </key> element               
                writer.WriteEndElement();
                
                //create <value> under <item>
                writer.WriteStartElement(dictionaryValueNodeName);
                
                if (dictionaryItem.Value != null)
                {
                    writer.WriteAttributeString(string.Empty, "type", string.Empty, dictionaryItem.Value.GetType().FullName);//  AssemblyQualifiedName);

                    if (dictionaryItem.Value.GetType() == typeof(TValue))
                    {
                        valueSerializer.Serialize(writer, dictionaryItem.Value);
                    }
                    else
                    {
                        new XmlSerializer(dictionaryItem.Value.GetType()).Serialize(writer, dictionaryItem.Value);
                    }
                }
                
                //end </value>  
                writer.WriteEndElement();
                
                //end </item>
                writer.WriteEndElement();
            }
        }


  //      public static void ReadDictionaryFromXml(IDictionary dictionary, string inputFileName)
  //      {

  //      }
        
  //      public static void ReadDictionaryFromXml(IDictionary dictionary, XmlReader reader, string groupName = DefaultGroupName)
		//{
  //      }


        public static void WriteDictionaryToXml(IDictionary dictionary, Stream outputStream, string groupName = DefaultGroupName, bool writeStartDocument = true)
        {
            using (XmlWriter writer = XmlWriter.Create(outputStream, GetXmlWriterSettings()))
            {
                WriteDictionaryToXml(dictionary, writer, groupName, writeStartDocument);
            }
        }

        public static void WriteDictionaryToXml(IDictionary dictionary, string outputFileName, string groupName = DefaultGroupName)
        {
            using (XmlWriter writer = XmlWriter.Create(outputFileName, GetXmlWriterSettings())) 
            {
                WriteDictionaryToXml(dictionary, writer, groupName, writeStartDocument: true);
            };
        }

        public static XmlWriterSettings GetXmlWriterSettings() => new XmlWriterSettings()
                                                                  {
                                                                      Indent = true,
                                                                      IndentChars = "\t",
                                                                      NewLineOnAttributes = true,
                                                                      NewLineHandling = NewLineHandling.Replace,
                                                                      CloseOutput = true
                                                                  };
        public static XmlReaderSettings GetXmlReadSettings()   => new XmlReaderSettings()
                                                                  {
                                                                      IgnoreWhitespace = true
                                                                  };

        public static void ReadDictionaryFromXml(IDictionary dictionary, Stream inputStream, string groupName = DefaultGroupName)
        {
            using (XmlReader reader = XmlReader.Create(inputStream, GetXmlReadSettings()))
            {
                ReadDictionaryFromXml(dictionary, reader, groupName);
            };
        }

        public static void ReadDictionaryFromXml(IDictionary dictionary, string inputFileName, string groupName = DefaultGroupName)
		{
            using (XmlReader reader = XmlReader.Create(inputFileName, GetXmlReadSettings()))
            {
                ReadDictionaryFromXml(dictionary, reader, groupName);
            };
        }


        public static void ReadDictionaryFromXml(IDictionary dictionary, XmlReader reader, string groupName = DefaultGroupName)
		{
            if (reader.IsEmptyElement)
                return;
            
            try
			{
                reader.ReadToFollowing(groupName);

                while (!reader.EOF && reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == groupName)
                        break;

                    string key = reader.Name;

                    reader.Read();

                    object? value = (reader.HasValue) ? (object)reader.Value : null;

                    dictionary[key] = value;

                    if (reader.NodeType != XmlNodeType.EndElement)
                        reader.Read();
                }
            }
            catch (Exception ex)
			{
                Debug.WriteLine("Error ReadDictionaryFromXml: " + ex.GetFullErrorMessage());
                
                return;
			}
        }

        public static void WriteDictionaryToXml(IDictionary dictionary, XmlWriter writer, string groupName, bool writeStartDocument)
		{
            if (writeStartDocument)
                writer.WriteStartDocument();

            if (dictionary.Count > 0)
            {
                writer.WriteStartElement(groupName);

                foreach (DictionaryEntry item in dictionary)
                {
                    string key = item.Key.ToString();
                    object value = item.Value; //.ValueToString();

                    if (value == null)
                        value = String.Empty;

                    writer.WriteStartElement(key);
                    writer.WriteValue(value); // null value and String.Empty has the same empty value, and will be read as null!
                    writer.WriteEndElement();
                }

                writer.WriteEndElement(); // groupName
            }

            if (writeStartDocument && dictionary.Count > 0)
                writer.WriteEndDocument();
        }

        //Example: https://stackoverflow.com/questions/41128542/xmlreader-get-element-by-name-and-write-out-childelements-c-sharp-webforms
    }
}
