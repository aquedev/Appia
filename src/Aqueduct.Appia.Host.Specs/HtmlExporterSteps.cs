using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using Xunit;
using System.IO;
using Aqueduct.Appia.Core;
using System.Reflection;

namespace Aqueduct.Appia.Host.Specs
{
    [Binding]
    public class StepDefinitions
    {
        HtmlExporter _exporter;
        private Configuration _configuration = new Configuration();
        private string _oldCurrentDir;

        protected string _websitePath;
        protected string _exportPath;

        public StepDefinitions()
        {
            _exportPath = Path.Combine(Directory.GetCurrentDirectory(), "export");
            _websitePath = Path.Combine(Directory.GetCurrentDirectory(), "website");
            Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), "Aqueduct.Appia.Razor.dll"));
        }
        protected void CreateWebsiteFiles(string folder, string extension, int number)
        {
            string destination = Path.Combine(_websitePath, folder);
            Directory.CreateDirectory(destination);
            for (int i = 0; i < number; i++)
                using (StreamWriter writer = File.CreateText(Path.Combine(destination, String.Format("file{0}{1}", i, extension))))
                {
                    writer.WriteLine("Test file content");
                }
        }

        protected void CreateWebsiteFile(string filePath, string contents)
        {
            
            string fullPath = Path.Combine(_websitePath, filePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            using (StreamWriter writer = File.CreateText(fullPath))
            {
                writer.Write(contents);
            }
        }

        protected void AssertExportedFileContainsText(string filePath, string text)
        {
            var fullPath = Path.Combine(_exportPath, filePath);
            Assert.True(File.Exists(fullPath));
            Assert.Contains(text, File.ReadAllText(fullPath));
        }
        
        
        protected void AssertFilesExistInDestination(string folder, string extension, int number)
        {
            string destination = Path.Combine(_exportPath, folder);
            Assert.True(Directory.Exists(destination));
            Assert.Equal(number, Directory.GetFiles(destination, "*" + extension).Length);
        }

        protected void AssertFileExistsInDestination(string filePath)
        {
            string destination = Path.Combine(_exportPath, filePath);
            Assert.True(File.Exists(destination));
        }

        [BeforeScenario]
        public void Setup()
        {
            InitialiseFolders();

            _oldCurrentDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(_websitePath);
            
        }

        private void InitialiseFolders()
        {
            Directory.CreateDirectory(_websitePath);
            Directory.CreateDirectory(Path.Combine(_websitePath, _configuration.PagesPath));
        }

        [AfterScenario]
        public void Cleanup()
        {
            GC.Collect(); // Otherwise the nancy viewengine still keeps references to the files
            Directory.SetCurrentDirectory(_oldCurrentDir);
            Directory.Delete(_exportPath, true);
            Directory.Delete(_websitePath, true);
        }

        [Given(@"a new HtmlExporter")]
        public void GivenANewHtmlExporter()
        {
            _exporter = new HtmlExporter(_exportPath, _configuration, new Bootstrapper());
            _exporter.Initialise();
        }


        [Given(@"I have css folder with 3 files")]
        public void GivenIHaveCssFolderWith3Files()
        {
            CreateWebsiteFiles("css", ".css", 3);
        }

        [Given(@"I have js folder with 3 files")]
        public void GivenIHaveJsFolderWith3Files()
        {
            CreateWebsiteFiles("js", ".js", 3);
        }

        [Then(@"I have css folder with 3 files in the export folder")]
        public void ThenIHaveCssFolderWith3FilesInTheExportFolder()
        {
            AssertFilesExistInDestination("css", ".css", 3);
        }

        [Then(@"I have js folder with 3 files in the export folder")]
        public void ThenIHaveJsFolderWith3FilesInTheExportFolder()
        {
            AssertFilesExistInDestination("js", ".js", 3);
        }

        [When(@"I export")]
        public void WhenIExport()
        {
            _exporter.Export();
        }

        [Given(@"I have static file ""static\.txt""")]
        public void GivenIHaveStaticFileStatic_Txt()
        {
            CreateWebsiteFiles("", ".txt", 1);
        }


        [Then(@"I have file ""static\.txt"" in the export folder")]
        public void ThenIHaveFileStatic_TxtInTheExportFolder()
        {
            AssertFilesExistInDestination("", ".txt", 1);
        }

        [When(@"I initialise the exporter")]
        public void WhenIInitialiseTheExporter()
        {
            new HtmlExporter(_exportPath, new Configuration(), new Bootstrapper()).Initialise();
        }

        [Then(@"an export folder is created")]
        public void ThenAnExportFolderIsCreated()
        {
            var fullExportPath = Path.Combine(Directory.GetCurrentDirectory(), _exportPath);
            Assert.True(Directory.Exists(fullExportPath));
        }

        [Then(@"a new empty export folder is created in place of the old one")]
        public void ThenANewEmptyExportFolderIsCreatedInPlaceOfTheOldOne()
        {
            Assert.Equal(0, Directory.GetFiles(_exportPath).Length);
        }

        [Given(@"I have an export folder already containing 2 files")]
        public void GivenIHaveAnExportFolderAlreadyContaining2Files()
        {
            for (int i = 0; i < 2; i++)
                using (StreamWriter writer = File.CreateText(Path.Combine(_exportPath, String.Format("file{0}.txt", i))))
                {
                    writer.WriteLine("Test file content");
                }
        }

        [Given(@"I have 4 cshtml files in the pages folder")]
        public void GivenIHave4CshtmlFilesInThePagesFolder()
        {
            CreateWebsiteFiles("pages", ".cshtml", 4);
        }

        [Then(@"I have 4 html files in the export directory")]
        public void ThenIHave4HtmlFilesInTheExportDirectory()
        {
            AssertFilesExistInDestination("", ".html", 4);
        }

        [Given(@"I have page ""page_with_layout\.cshtml""")]
        public void GivenIHavePagePage_With_Layout_Cshtml()
        {
            CreateWebsiteFile("pages\\page_with_layout.cshtml", @"@{
    Layout = ""layout"";
}
<div>text</div>");
        }

        [Given(@"it uses layout ""layout\.cshtml""")]
        public void GivenItUsesLayoutLayout_Cshtml()
        {
            CreateWebsiteFile("layouts\\layout.cshtml", "<h1>layout</h1>{{content}}<h2>content</h2>");
        }

        [Then(@"""page_with_layout\.html"" contains html content from layout ""layout\.cshtml""")]
        public void ThenPage_With_Layout_HtmlContainsHtmlContentFromLayoutLayout_Cshtml()
        {
            AssertExportedFileContainsText("page_with_layout.html", "<h1>layout");
        }

        [Then(@"""page_with_layout\.html"" contains html content from page ""page_with_layout\.cshtml""")]
        public void ThenPage_With_Layout_HtmlContainsHtmlContentFromPagePage_With_Layout_Cshtml()
        {
            AssertExportedFileContainsText("page_with_layout.html", "<div>text</div>");
        }

        [Then(@"I have html page ""page_with_layout\.html""")]
        public void ThenIHaveHtmlPagePage_With_Layout_Html()
        {
            AssertFileExistsInDestination("page_with_layout.html");
        }

        [Given(@"I have page ""(.*)""")]
        public void GivenIHavePagePage_With_Partial_Cshtml(string filename)
        {
            CreateWebsiteFile(_configuration.PagesPath + "\\" + filename, "<span>page</span> @RenderPartial(\"partial\")");
        }

        [Given(@"it uses partial ""(.*)""")]
        public void GivenItUsesPartialPartial_Cshtml(string filename)
        {
            CreateWebsiteFile(_configuration.PartialsPath + "\\" + filename, "<div>partial</div>");
        }

        [Then(@"""(.*)"" contains html content from page ""page_with_partial\.cshtml""")]
        public void ThenPage_With_Partial_HtmlContainsHtmlContentFromPagePage_With_Partial_Cshtml(string filename)
        {
            AssertExportedFileContainsText(filename, "<span>page</span>");
        }

        [Then(@"""(.*)"" contains html content from partial ""partial\.cshtml""")]
        public void ThenPage_With_Partial_HtmlContainsHtmlContentFromPartialPartial_Cshtml(string filename)
        {
            AssertExportedFileContainsText(filename, "<div>partial");
        }

    }

}
