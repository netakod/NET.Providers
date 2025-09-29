using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Simple.Collections;

namespace Simple.AppContext
{
    public class AppSettings
    {
        private SimpleDictionary<string, object> dictionary = null;
        private bool requireSaving = false;
        //private Dictionary<string, object> dictionary = null;
        private object lockObject = new object();

        public AppSettings(string filePath)
        {
            this.FilePath = filePath;
        }

        private SimpleDictionary<string, object> Dictionary
        {
            get
            {
                if (this.dictionary == null)
                {
                    if (File.Exists(this.FilePath))
                    {
                        this.Load();

                        //StreamReader streamReeader = null;
                        
                        //try
                        //{
                        //    lock (lockObject)
                        //    {
                        //        this.dictionary = new SimpleDictionary<string, object>();
                                
                                
                        //        //this.dictionary = XmlHelper.DeserializeObject<SimpleDictionary<string, object>>(streamReeader.ReadToEnd());
                                
                                
                                
                        //        //this.dictionary = new Dictionary<string, object>();
                        //        //streamReeader = File.OpenText(this.FilePath); //new StreamReader(this.filePath);
                        //        //var xmlReader = XmlReader.Create(streamReeader.BaseStream);
                                
                        //        //XmlHelper.ReadDictionaryFromXml<string, object>(this.dictionary = new Dictionary<string, object>(), xmlReader);
                                
                        //        //streamReeader.Close();
                        //    }
                        //}
                        //catch
                        //{
                        //    //this.dictionary = new Dictionary<string, object>();

                        //    if (streamReeader != null)
                        //    {
                        //        streamReeader.Close();
                        //        streamReeader.Dispose();
                        //    }

                        //    //throw new Exception(exception.Message + "\r\n" + this.FilePath);
                        //    //MessageBox.Show(exception.Message + "\r\n" + this.filePath, this.AppContext.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        //}
                    }
                    else
                    {
                        // this.dictionary = new Dictionary<string, object>();
                        this.dictionary = new SimpleDictionary<string, object>();
                    }
                }

                return this.dictionary;
            }
        }

        //public AppContextBase AppContext { get; private set; }
		public string FilePath { get; private set; }
        public string GroupName { get; set; } = "Settings";

        public T GetValue<T>(string key)
        {
            return this.GetValue<T>(key, default(T));
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            T value;
            object valueObject = defaultValue;
            
            if (this.Dictionary.TryGetValue(key, out valueObject))
                value = Conversion.TryChangeType<T>(valueObject, defaultValue);
            else
                value = defaultValue;

            return value;
        }

        public void SetValue<T>(string key, T value, T defaultValue = default(T))
		{
            bool isDefault = object.Equals(value, defaultValue);

            if (!isDefault && typeof(T) == typeof(String) && String.IsNullOrEmpty(value as string) && String.IsNullOrEmpty(defaultValue as string)) // -> null string or String.Empty is considering to be the same default value for string
                isDefault = true;

            if (isDefault)
            {
                if (this.Dictionary.ContainsKey(key))
                {
                    this.Dictionary.Remove(key);
                    this.requireSaving = true;
                }
            }
            else 
            {
                this.Dictionary[key] = value;
                this.requireSaving = true;
            }
		}

        //public object GetValue(string key)
        //{
        //    return this.GetValue(key, null);
        //}

        //public object GetValue(string key, object defaultValue)
        //{
        //    object value = defaultValue;
        //    this.Dictionary.TryGetValue(key, out value);

        //    return value;
        //}

        //public void SetValue(string key, object value)
        //{
        //    this.Dictionary[key] = value;
        //}

        public void Load(XmlReader reader = null)
		{
            if (this.dictionary == null)
                this.dictionary = new SimpleDictionary<string, object>();
            else
                this.dictionary.Clear();

            if (reader != null)
			{
                XmlHelper.ReadDictionaryFromXml(this.dictionary, reader, this.GroupName);
            }
            else
			{
                XmlHelper.ReadDictionaryFromXml(this.dictionary, this.FilePath, this.GroupName);
			}

            this.requireSaving = false;
        }

        public void Save(XmlWriter writer = null)
        {
            if (!this.requireSaving)
                return;
            
            //try
            //{
            lock (lockObject)
            {
                //string xml = XmlHelper.SerializeObject<SimpleDictionary<string, object>>(this.Dictionary);
                //FileInfo file = new FileInfo(this.FilePath);
                //StreamWriter streamWriter = file.CreateText();
                //streamWriter.Write(xml);
                //streamWriter.Close();
                //streamWriter.Dispose();
                //string nullString = null;

                //this.dictionary["TestNull"] = null;
                //this.dictionary["TestNullString"] = default(string);
                //this.dictionary["TestEmptyString"] = String.Empty;
                //this.dictionary["TestString"] = "Normal String as is!";

                if (writer != null)
				{
                    // Continuing adding segments into writer
                    XmlHelper.WriteDictionaryToXml(this.Dictionary, writer, this.GroupName, writeStartDocument: false);
                }
                else
				{
                    XmlHelper.WriteDictionaryToXml(this.Dictionary, this.FilePath, this.GroupName);

                    // By using Stream
                    //FileInfo file = new FileInfo(this.FilePath);
                    //using (StreamWriter streamWriter = file.CreateText())
                    //{
                    //    XmlHelper.WriteDictionaryToXml(this.Dictionary, streamWriter.BaseStream, groupName);
                    //    streamWriter.Flush();
                    //    streamWriter.Close();
                    //}
                }

                this.requireSaving = false;
            }
        }
    }
}
