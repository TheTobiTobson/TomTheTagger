using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace TomTheTagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DatabaseManager oDataBase = new DatabaseManager("JSONTestData.txt");

        public MainWindow()
        {
            InitializeComponent();
            oDataBase.LoadJsonDatabaseFile();        
        }

        private void TagBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                MessageBox.Show(oDataBase.SearchDatabaseForTag("hi").ToString());
            }
        }
    }

    public class TaggedFile
    {
        public string Path { get; set; }
        public bool Active { get; set; }
        public IList<string> Tags { get; set; }
    }

    class DatabaseManager
    {
        public string mDatabaseLocation; //Location of JSON Database file
        public List<TaggedFile> mTaggedFileListe; //Database in RAM

        // Constructor
        public DatabaseManager(string pDatabaseLocation)
        {
            mDatabaseLocation = pDatabaseLocation;
        }
        
        // Load Database from File to RAM
        public void LoadJsonDatabaseFile()
        {
            string JsonDatabaseFileInString = File.ReadAllText(mDatabaseLocation);
            mTaggedFileListe = JsonConvert.DeserializeObject<List<TaggedFile>>(JsonDatabaseFileInString);
        }

        public List<TaggedFile> SearchDatabaseForTag(string pTagToSearchFor)
        {
            List<TaggedFile> localSearchResults = new List<TaggedFile>();

            localSearchResults.Add(mTaggedFileListe[0]);
            localSearchResults.Add(mTaggedFileListe[2]);

            return localSearchResults;
        }
    }
}
