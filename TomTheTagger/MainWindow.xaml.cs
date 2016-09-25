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
using System.Collections.ObjectModel;

namespace TomTheTagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DatabaseManager oDataBase = new DatabaseManager("JSONTestData.txt");

        // List with search Results
        List<SearchRelatedInfosForOneFile> oSearchResultsWithCorrespondingTags = new List<SearchRelatedInfosForOneFile>();

        public MainWindow()
        {
            InitializeComponent();
            oDataBase.LoadJsonDatabaseFile();
            lvUsers.ItemsSource = oSearchResultsWithCorrespondingTags;

        }

        private void TagBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                oSearchResultsWithCorrespondingTags = oDataBase.SearchDatabaseForTag(TagBox1.Text, TagBox2.Text);
                lvUsers.ItemsSource = oSearchResultsWithCorrespondingTags;
            }
        }
    }

    // Place to store data from JSON Database file
    class TaggedFile
    {
        public int IdentNr { get; set; }   
        public string Path { get; set; }
        public bool Active { get; set; }
        public IList<string> Tags { get; set; }
    }

    // Place to store data from JSON Database file
    class SearchRelatedInfosForOneFile
    {
        public int IdentNr { get; set; }        
        public string[] TagsInThisFileWhichCorrespondToQuery { get; set; }
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

        // Search in Database
        public List<SearchRelatedInfosForOneFile> SearchDatabaseForTag(string pTagToSearchForFromTagBox1, string pTagToSearchForFromTagBox2)
        {

            //Create Array with Tags to be searched
            string[] TagsToBeSearchedFor = { pTagToSearchForFromTagBox1, pTagToSearchForFromTagBox2 };

            List<SearchRelatedInfosForOneFile> localSearchResults = new List<SearchRelatedInfosForOneFile>();

            foreach (var SearchTag in TagsToBeSearchedFor)
            {
                var Search = from TaggedFiles in mTaggedFileListe
                             from TagsinTaggedFiles in TaggedFiles.Tags
                             where TagsinTaggedFiles == SearchTag
                             //select TaggedFiles.IdentNr;
                             select new SearchRelatedInfosForOneFile { IdentNr = TaggedFiles.IdentNr, TagsInThisFileWhichCorrespondToQuery = new string[] { SearchTag } };

                //Noch wird Liste an dieser Stelle Überschrieben. Hier muss jetzt Logik hin um Liste zusammen zu fügen
                localSearchResults = Search.ToList();
            }


            return localSearchResults;
        }
    }
}
