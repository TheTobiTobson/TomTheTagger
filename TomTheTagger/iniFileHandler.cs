using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TomTheTagger
{
    class iniFileHandler // revision 10
    {
        string configFilePath;
        string databaseFileLocation;
        string EXE = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32")]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public iniFileHandler(string IniPath = null)
        {
            configFilePath = new FileInfo(IniPath ?? EXE + ".ini").FullName.ToString();
        }

        public string getConfigFileLocation()
        {
            return configFilePath;
        }

        public string getDatabaseFileLocation()
        {
            return databaseFileLocation;
        }

        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, configFilePath);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? EXE, Key, Value, configFilePath);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }

        /// <summary>
        /// Load Location of JSON database file
        /// true > Key exists in File
        /// false > Key does not exist in file
        /// </summary>
        public bool loadDatabaseFileLocation()
        {
            //load location of database file
            if (!KeyExists("databaseFile", "SETTINGS"))
            {
                return false;
            }
            databaseFileLocation = Read("databaseFile", "SETTINGS");
            return true;
        }
       
    }
}
