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
    /*** Datasets ***/
    /// <summary>
    /// Place to store data from JSON Database file
    /// </summary>
    class TaggedFile
    {
        public int IdentNr { get; set; }
        public string Path { get; set; }
        public bool Active { get; set; }
        public List<string> Tags { get; set; }
    }

    /// <summary>
    /// Pair of one IdentNr and one Tag
    /// </summary>
    class identTagPair
    {
        public int mIdentNr { get; set; }
        public string mOneTag { get; set; }
    }

    /// <summary>
    /// One IdentNr and multiple Tags
    /// </summary>
    class identAndMultipleTags
    {
        public int mIdentNr { get; set; }
        public List<string> mAllTagInThisFileThatCorrespondsToQuery { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
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
    
    

    /*** Classes with logic ***/      
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DatabaseManager oDataBase = new DatabaseManager("JSONTestData.txt");
        SearchOperations oSearchOps = new SearchOperations();
        
        public MainWindow()
        {
            InitializeComponent();
            oDataBase.LoadJsonDatabaseFile();
        }

        /// <summary>
        /// Method is called after keydown in tagbox
        /// </summary>
        private void TagBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                string[] tagsFromGui = { TagBox1.Text, TagBox2.Text, TagBox3.Text, TagBox4.Text };
                string[] validTagStringArray = null;

                // Validate string
                if (!oSearchOps.validateAndOrganizeTags(tagsFromGui, ref validTagStringArray))
                {
                    MessageBox.Show("No TAGs to search for");
                }
                else
                {
                    // Start Search
                    lvUsers.ItemsSource = oSearchOps.searchForMultipleTags(validTagStringArray, oDataBase.mTaggedFileListe);
                }
                
            }
        }
    }

    /// <summary>
    /// Implements all functions to execute a search and merge results
    /// </summary>
    public class SearchOperations
    {
        /// <summary>
        /// Checks if tags are vaild. Search for tags. Merge all search results. Merge data from database for GUI
        /// </summary>       
        internal List<TaggedFile> searchForMultipleTags(string[] pSearchtagsFromGui, List<TaggedFile> pjsonDatabaseInRam)
        {
            List<identTagPair>[] arrayOfIdentTagPairLists = new List<identTagPair>[10]; // später pSearchtagsFromGui.Length()

            int pAmountOfTagsToBeSearchedFor = pSearchtagsFromGui.Length;

            // Excecute a search for every tag
            for (int i = 0; i < pAmountOfTagsToBeSearchedFor; i++)
            {
                arrayOfIdentTagPairLists[i] = searchForOneTag(pSearchtagsFromGui[i], pjsonDatabaseInRam);
                //if( arrayOfIdentTagPairLists[0].Count == 0) // Kein Tag gefunden > Daten nicht und nicht mergen
            }

            // initialize List<identAndMultipleTags> with identTagPair for the first Tag
            List<identAndMultipleTags> identAndMultipleTagsList = initIdentAndMultipleTagsList(arrayOfIdentTagPairLists[0]);

            // merge all identTagPairs to List<identAndMultipleTags>
            for (int i = 1; i < pAmountOfTagsToBeSearchedFor; i++)
            {
                mergeOneIdentTagPair(arrayOfIdentTagPairLists[i], identAndMultipleTagsList);
            }

            //Merge Database information and identAndMultipleTagsList together
            return mergeDatabaseAndSearchResults(identAndMultipleTagsList, pjsonDatabaseInRam);

        }

        internal bool validateAndOrganizeTags (string[] ptagsFromGUI, ref string[] pValidTagString)
        {

            List<string> localTagsFromGuiList = new List<string>(ptagsFromGUI);
            List<string> tagListWithoutDublicates = new List<string>();

            // remove all elements which are null
            localTagsFromGuiList.RemoveAll(q => q == "");
                        
            if(localTagsFromGuiList.Count == 0)
            {
                // Nothing to search for
                return false;
            }

            // Remove dublicates
            tagListWithoutDublicates = localTagsFromGuiList.Distinct().ToList();

            pValidTagString = tagListWithoutDublicates.ToArray<string>();

            return true;

            //string[] Test1 = { "", "", "ARD", "", "ZDF", "", "ZDF", "USA", "ARD", "ARD" , "USA" };
            ////string[] Test2 = new string[];

            //List<string> TestList = new List<string>();

            //List<string> TestRemovedDublicates = new List<string>();

            //TestList = Test1.ToList<string>();

            //TestList.RemoveAll(q => q == "");

            //TestRemovedDublicates = TestList.Distinct().ToList();


        }

        /// <summary>
        /// Method to search for one Tag in Database
        /// </summary>
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

        /// <summary>
        /// Merge Database information and identAndMultipleTagsList together in order to have all relevant information for GUI
        /// </summary>
        private List<TaggedFile> mergeDatabaseAndSearchResults(List<identAndMultipleTags> pidentAndMultipleTagsList, List<TaggedFile> pjsonDatabaseInRam)
        {
            List<TaggedFile> localMergedList = new List<TaggedFile>();

            var Search = from p in pidentAndMultipleTagsList
                         join t in pjsonDatabaseInRam on p.mIdentNr equals t.IdentNr
                         select new TaggedFile() { IdentNr = t.IdentNr, Active = t.Active, Path = t.Path, Tags = p.mAllTagInThisFileThatCorrespondsToQuery };

            localMergedList = Search.ToList();

            return localMergedList;
        }
    }

    /// <summary>
    /// Loads json database file to RAM and offers access to the rest of the assembly
    /// </summary>
    class DatabaseManager
    {
        public string mDatabaseLocation; //Location of JSON Database file
        public List<TaggedFile> mTaggedFileListe; //Database in RAM

        /// <summary>
        /// Constructor
        /// </summary>
        public DatabaseManager(string pDatabaseLocation)
        {
            mDatabaseLocation = pDatabaseLocation;
        }

        /// <summary>
        /// Load Database from json file to RAM
        /// </summary>
        public void LoadJsonDatabaseFile()
        {
            string JsonDatabaseFileInString = File.ReadAllText(mDatabaseLocation);
            mTaggedFileListe = JsonConvert.DeserializeObject<List<TaggedFile>>(JsonDatabaseFileInString);
        }
    }
}
