using System;
using System.Collections;
using System.IO;

namespace Simple
{
    // sample class to demostrate ordering full path files
    public class FileLogicalComparer
    {
        public ArrayList files = new ArrayList();

        public FileLogicalComparer()
        { 
        }

        public void AddFile(string file)
        {
            if (file == null)
                return;
            
            if (files == null)
                files = new ArrayList();
            
            files.Add(new DictionaryEntry(Path.GetFileName(file), file));
        }

        // convenience method
        public void AddFiles(string[] f)
        {
            if (f == null)
                return;
            
            for (int i = 0; i < f.Length; i++)
                AddFile(f[i]);
        }

        public ArrayList GetSorted()
        {
            files.Sort(new DictionaryEntryComparer(new NumericComparer()));
            
            return files;
        }

        public static string[]? Sort(string[] files)
        {
            if (files == null)
                return null;
            
            FileLogicalComparer fc = new FileLogicalComparer();
            
            fc.AddFiles(files);
            
            ArrayList list = fc.GetSorted();
            
            if (list == null)
                return files;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is DictionaryEntry dictionaryEntry && dictionaryEntry.Value != null)
                    files[i] = (string)dictionaryEntry.Value;
            }

            return files;
        }

    }//EOC
}