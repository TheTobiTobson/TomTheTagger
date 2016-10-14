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

    // Pair of one IdentNr and one Tag
    class identTagPair
    {
        public int mIdentNr { get; set; }
        public string mOneTag { get; set; }
    }

    // One IdentNr and multiple Tags
    class identAndMultipleTags
    {
        public int mIdentNr { get; set; }
        public List<string> mAllTagInThisFileThatCorrespondsToQuery { get; set; }

        // Constructor
        public identAndMultipleTags(int pIdentNr, string pTagsToAdd)
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
        SearchOperations oSearchOps = new SearchOperations();

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
                string[] tagsFromGui = { TagBox1.Text, TagBox2.Text, TagBox3.Text, TagBox4.Text };
                oSearchOps.searchForMultipleTags(tagsFromGui, oDataBase.mTaggedFileListe);
                //oSearchResultsWithCorrespondingTags = oDataBase.SearchDatabaseForTag(TagBox1.Text, TagBox2.Text, TagBox3.Text, TagBox4.Text);
                //lvUsers.ItemsSource = oDataBase.JoinListsForGui(oSearchResultsWithCorrespondingTags);
            }
        }
    }

    public class SearchOperations
    {
        internal List<SearchRelatedInfosMerged> searchForMultipleTags(string[] pSearchtagsFromGui, List<TaggedFile> pjsonDatabaseInRam)
        {   
            List<identTagPair>[] arrayOfIdentTagPairLists = new List<identTagPair>[10]; // später pSearchtagsFromGui.Length()
            int amountOfTagsToBeSearchedFor = 0;

            // Check if Tags are valid and count
            foreach (var item in pSearchtagsFromGui){
                if (item.Length != 0){
                    amountOfTagsToBeSearchedFor++;
                }
            }

            //if(amountOfTagsToBeSearchedFor == 0) // Schlau abbrechen

            // Excecute a search for every tag
            for (int i = 0; i < amountOfTagsToBeSearchedFor; i++)
            {
                arrayOfIdentTagPairLists[i] = searchForOneTag(pSearchtagsFromGui[i], pjsonDatabaseInRam);
                //if( arrayOfIdentTagPairLists[0].Count == 0) // Kein Tag gefunden > Daten nicht und nicht mergen
            }

            // initialize List<identAndMultipleTags> with identTagPair for the first Tag
            List<identAndMultipleTags> identAndMultipleTagsList = initIdentAndMultipleTagsList(arrayOfIdentTagPairLists[0]);

            // merge all identTagPairs 
            mergeOneIdentTagPair(arrayOfIdentTagPairLists[1], identAndMultipleTagsList);
            mergeOneIdentTagPair(arrayOfIdentTagPairLists[2], identAndMultipleTagsList);

            List<SearchRelatedInfosMerged> TEST = new List<SearchRelatedInfosMerged>();
            return TEST;

        }

        /// <summary>
        /// Method to search for one Tag in Database
        /// </summary>
        /// <param name="pTagToBeSearcheFor"></param>
        /// <param name="pjsonDatabaseInRam"></param>
        /// <returns></returns>
        private List<identTagPair> searchForOneTag(string pTagToBeSearcheFor, List<TaggedFile> pjsonDatabaseInRam)
        {
            List<identTagPair> localIdentTagPairList = new List<identTagPair>();

           var Search = from TaggedFiles in pjsonDatabaseInRam
                         from TagsinTaggedFiles in TaggedFiles.Tags
                         where TagsinTaggedFiles == pTagToBeSearcheFor
                         select new identTagPair() { mIdentNr = TaggedFiles.IdentNr, mOneTag = pTagToBeSearcheFor };

            localIdentTagPairList = Search.ToList();

            return localIdentTagPairList;
        }

        /// <summary>
        /// Method to initialize List<identAndMultipleTags> which will be use to gather all search results
        /// </summary>
        /// <param name="pIdentTagPairListForInit"></param>
        /// <returns></returns>
        private List<identAndMultipleTags> initIdentAndMultipleTagsList(List<identTagPair> pIdentTagPairListForInit)
        {
            List<identAndMultipleTags> oLocalIdentAndMultipleTagsList = new List<identAndMultipleTags>();

            foreach (var item in pIdentTagPairListForInit)
            {
                oLocalIdentAndMultipleTagsList.Add(new identAndMultipleTags(item.mIdentNr, item.mOneTag));
            }

            return oLocalIdentAndMultipleTagsList;
        }

        /// <summary>
        /// Method to merge identTagPair to identAndMultipleTagsList
        /// </summary>
        /// <param name="pIdentTagPairToMerge"></param>
        /// <param name="pidentAndMultipleTagsList"></param>
        private void mergeOneIdentTagPair(List<identTagPair> pIdentTagPairToMerge, List<identAndMultipleTags> pidentAndMultipleTagsList)
        {
            foreach (var IdentTagPairItem in pIdentTagPairToMerge)
            {
                bool subLoopEnteredSomethingInMergedInfo = false;

                foreach (var IdentAndMultipleTagItem in pidentAndMultipleTagsList)
                {
                    if (IdentTagPairItem.mIdentNr == IdentAndMultipleTagItem.mIdentNr)
                    {
                        
                        IdentAndMultipleTagItem.addTag(IdentTagPairItem.mOneTag);
                        subLoopEnteredSomethingInMergedInfo = true;

                        // There is only one matching IdentNr in IdentMULTIPLEtagCombi >> work is done for this loop
                        break;
                    }
                }

                if (!subLoopEnteredSomethingInMergedInfo)
                {
                    pidentAndMultipleTagsList.Add(new identAndMultipleTags(IdentTagPairItem.mIdentNr, IdentTagPairItem.mOneTag));
                }
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
