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
    /* Datasets */
    // Place to store data from JSON Database file
    class TaggedFile
    {
        public int IdentNr { get; set; }
        public string Path { get; set; }
        public bool Active { get; set; }
        public IList<string> Tags { get; set; }
    }

    // Place to store merged search information
    class SearchRelatedInfosMerged
    {
        public int mIdentNr { get; set; }
        public List<string> mAllTagInThisFileThatCorrespondsToQuery { get; set; }

        // Constructor
        public SearchRelatedInfosMerged(int pIdentNr, string pTagsToAdd)
        {
            mAllTagInThisFileThatCorrespondsToQuery = new List<string>();

            mIdentNr = pIdentNr;
            mAllTagInThisFileThatCorrespondsToQuery.Add(pTagsToAdd);
        }

        public void addTag(string pNewTag)
        {
            mAllTagInThisFileThatCorrespondsToQuery.Add(pNewTag);
        }
    }

    // Place to store NEW search information
    class SearchRelatedInfosNew
    {
        public int mIdentNr { get; set; }
        public string mOneTagInThisFileThatCorrespondsToQuery { get; set; }
    }

    // Data for GUI
    class SearchResultsForGUI
    {
        public int IdentNr { get; set; }
        public string Path { get; set; }
        public bool Active { get; set; }
        public List<string> TagsCorrespondingToSearch { get; set; }
    }
    
    
    /* classes with logic */
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DatabaseManager oDataBase = new DatabaseManager("JSONTestData.txt");

        // List with search Results
        List<SearchRelatedInfosMerged> oSearchResultsWithCorrespondingTags = new List<SearchRelatedInfosMerged>();

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
                oSearchResultsWithCorrespondingTags = oDataBase.SearchDatabaseForTag(TagBox1.Text, TagBox2.Text, TagBox3.Text, TagBox4.Text);
                lvUsers.ItemsSource = oDataBase.JoinListsForGui(oSearchResultsWithCorrespondingTags);
            }
        }
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

        ///////////////////////////////////////////////////////
        // Input: SearchTags from GUI
        // Output: 
        //          IdentNr: 1234, Tags: "VAT", "APC", "TW"  
        //          IdentNr: 3333, Tags: "BMW", "APC", "TW"  
        ///////////////////////////////////////////////////////
        public List<SearchRelatedInfosMerged> SearchDatabaseForTag(string pTagToSearchForFromTagBox1, string pTagToSearchForFromTagBox2, string pTagToSearchForFromTagBox3, string pTagToSearchForFromTagBox4)
        {
            // Local Storage for Search Infos
            List<SearchRelatedInfosMerged> MergedInfos = new List<SearchRelatedInfosMerged>();

            int amountOfTagsToBeSearchedFor = 0;

            //Create Array with Tags to be searched
            string[] TagsToBeSearchedFor = { pTagToSearchForFromTagBox1, pTagToSearchForFromTagBox2, pTagToSearchForFromTagBox3, pTagToSearchForFromTagBox4 };

            foreach (var item in TagsToBeSearchedFor)
            {
                
                if(item.Length != 0)
                {
                    amountOfTagsToBeSearchedFor++;
                }
            }

            if (amountOfTagsToBeSearchedFor == 0)
            {

            }
            //////////////// 1 TAGS ////////////////
            else if (amountOfTagsToBeSearchedFor == 1)
            {
                var Search1 = from TaggedFiles in mTaggedFileListe
                              from TagsinTaggedFiles in TaggedFiles.Tags
                              where TagsinTaggedFiles == pTagToSearchForFromTagBox1
                              select new SearchRelatedInfosMerged(TaggedFiles.IdentNr, pTagToSearchForFromTagBox1);

                MergedInfos = Search1.ToList();
            }
            //////////////// 2 TAGS ////////////////
            else if (amountOfTagsToBeSearchedFor == 2)
            {
                var Search1 = from TaggedFiles in mTaggedFileListe
                              from TagsinTaggedFiles in TaggedFiles.Tags
                              where TagsinTaggedFiles == pTagToSearchForFromTagBox1
                              select new SearchRelatedInfosMerged(TaggedFiles.IdentNr, pTagToSearchForFromTagBox1);

                MergedInfos = Search1.ToList();

                var Search2 = from TaggedFiles in mTaggedFileListe
                              from TagsinTaggedFiles in TaggedFiles.Tags
                              where TagsinTaggedFiles == pTagToSearchForFromTagBox2
                              select new SearchRelatedInfosNew() { mIdentNr = TaggedFiles.IdentNr, mOneTagInThisFileThatCorrespondsToQuery = pTagToSearchForFromTagBox2 };

                foreach (var IdentONEtagCombi in Search2)
                {
                    bool subLoopEnteredSomethingInMergedInfo = false;

                    foreach (var IdentMULTIPLEtagCombi in MergedInfos)
                    {
                        if (IdentONEtagCombi.mIdentNr == IdentMULTIPLEtagCombi.mIdentNr)
                        {
                            IdentMULTIPLEtagCombi.addTag(IdentONEtagCombi.mOneTagInThisFileThatCorrespondsToQuery);
                            subLoopEnteredSomethingInMergedInfo = true;

                            // There is only one matching IdentNr in IdentMULTIPLEtagCombi >> work is done for this loop
                            break;
                        }
                    }

                    if (!subLoopEnteredSomethingInMergedInfo)
                    {
                        MergedInfos.Add(new SearchRelatedInfosMerged(IdentONEtagCombi.mIdentNr, IdentONEtagCombi.mOneTagInThisFileThatCorrespondsToQuery));
                    }
                }
            }
            //////////////// 3 TAGS ////////////////
            else if (amountOfTagsToBeSearchedFor == 3)
            {
                // Search for Tag 1
                var Search1 = from TaggedFiles in mTaggedFileListe
                              from TagsinTaggedFiles in TaggedFiles.Tags
                              where TagsinTaggedFiles == pTagToSearchForFromTagBox1
                              select new SearchRelatedInfosMerged(TaggedFiles.IdentNr, pTagToSearchForFromTagBox1);

                MergedInfos = Search1.ToList();

                // Search for Tag 2
                var Search2 = from TaggedFiles in mTaggedFileListe
                              from TagsinTaggedFiles in TaggedFiles.Tags
                              where TagsinTaggedFiles == pTagToSearchForFromTagBox2
                              select new SearchRelatedInfosNew() { mIdentNr = TaggedFiles.IdentNr, mOneTagInThisFileThatCorrespondsToQuery = pTagToSearchForFromTagBox2 };

                // Merge Search1 and Search2
                foreach (var IdentONEtagCombi in Search2)
                {
                    bool subLoopEnteredSomethingInMergedInfo = false;

                    foreach (var IdentMULTIPLEtagCombi in MergedInfos)
                    {
                        if (IdentONEtagCombi.mIdentNr == IdentMULTIPLEtagCombi.mIdentNr)
                        {
                            IdentMULTIPLEtagCombi.addTag(IdentONEtagCombi.mOneTagInThisFileThatCorrespondsToQuery);
                            subLoopEnteredSomethingInMergedInfo = true;

                            // There is only one matching IdentNr in IdentMULTIPLEtagCombi >> work is done for this loop
                            break;
                        }
                    }

                    if (!subLoopEnteredSomethingInMergedInfo)
                    {
                        MergedInfos.Add(new SearchRelatedInfosMerged(IdentONEtagCombi.mIdentNr, IdentONEtagCombi.mOneTagInThisFileThatCorrespondsToQuery));
                    }
                }

                // Search for Tag 3
                var Search3 = from TaggedFiles in mTaggedFileListe
                              from TagsinTaggedFiles in TaggedFiles.Tags
                              where TagsinTaggedFiles == pTagToSearchForFromTagBox3
                              select new SearchRelatedInfosNew() { mIdentNr = TaggedFiles.IdentNr, mOneTagInThisFileThatCorrespondsToQuery = pTagToSearchForFromTagBox3 };

                // Merge Search3 
                foreach (var IdentONEtagCombi in Search3)
                {
                    bool subLoopEnteredSomethingInMergedInfo = false;

                    foreach (var IdentMULTIPLEtagCombi in MergedInfos)
                    {
                        if (IdentONEtagCombi.mIdentNr == IdentMULTIPLEtagCombi.mIdentNr)
                        {
                            IdentMULTIPLEtagCombi.addTag(IdentONEtagCombi.mOneTagInThisFileThatCorrespondsToQuery);
                            subLoopEnteredSomethingInMergedInfo = true;

                            // There is only one matching IdentNr in IdentMULTIPLEtagCombi >> work is done for this loop
                            break;
                        }
                    }

                    if (!subLoopEnteredSomethingInMergedInfo)
                    {
                        MergedInfos.Add(new SearchRelatedInfosMerged(IdentONEtagCombi.mIdentNr, IdentONEtagCombi.mOneTagInThisFileThatCorrespondsToQuery));
                    }
                }
            }



            return MergedInfos;
        }

        public List<SearchResultsForGUI> JoinListsForGui (List<SearchRelatedInfosMerged> pMergedSearchInfos)
        {
            List<SearchResultsForGUI> localListForGui = new List<SearchResultsForGUI>();

            var ListForGui = from p in pMergedSearchInfos
                             join t in mTaggedFileListe on p.mIdentNr equals t.IdentNr
                             select new SearchResultsForGUI() { IdentNr = t.IdentNr, Active = t.Active, Path = t.Path, TagsCorrespondingToSearch = p.mAllTagInThisFileThatCorrespondsToQuery };

            localListForGui= ListForGui.ToList();

            return localListForGui;
        }
    }
}
