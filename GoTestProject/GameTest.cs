using Go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;

namespace GoTestProject
{


    /// <summary>
    ///This is a test class for GameTest and is intended
    ///to contain all GameTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GameTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for SerializeToSGF
        ///</summary>
        [TestMethod()]
        public void SerializeToSGFTest()
        {
            TestConsole.NewConsole();
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            var sampleGames = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            int numTests = sampleGames.Count();
            int curTest = 0;
            Trace.WriteLine("Running " + numTests + " tests");
            foreach (var sourceFile in sampleGames)
            {
                curTest++;
                Trace.WriteLine("Running test #" + curTest + " on file " + sourceFile);
                string sourceText;
                using (Stream stream = Assembly.GetExecutingAssembly()
                                               .GetManifestResourceStream(sourceFile))
                using (StreamReader reader = new StreamReader(stream))
                {
                    sourceText = reader.ReadToEnd();
                }
                File.WriteAllText("test.sgf", sourceText);

                Game target = Game.SerializeFromSGF("test.sgf")[0];
                GameInfo gi = target.GameInfo;
                string expected = sourceText.Replace("\n", "");
                string actual;
                actual = target.SerializeToSGF(null);
                actual = Regex.Replace(actual, @"\s", "");
                actual = Regex.Replace(actual, @"A[BWE](\[[^]]+\])+", "");
                expected = Regex.Replace(expected, @"\s", "");
                expected = Regex.Replace(expected, @"A[BWE](\[[^]]+\])+", "");
                Assert.AreEqual(expected, actual);
            }
            TestConsole.CloseConsole();
        }

        /// <summary>
        ///A test for SerializeToSGF
        ///</summary>
        [TestMethod()]
        public void SerializeToSGFTest1()
        {
            GameInfo gi = new GameInfo();
            gi.BoardSizeX = gi.BoardSizeY = 9;
            gi.FreePlacedHandicap = false;
            gi.Handicap = 3;
            gi.Komi = 7.5;
            gi.StartingPlayer = Content.White;
            Game target = new Game(gi);
            target.SetupMove(5, 5, Content.Black);
            target.SetupMove(6, 5, Content.Black);
            target.SetupMove(8, 5, Content.White);
            target.MakeMove(5, 7).MakeMove(6, 7);
            target.MakeMove(5, 8);
            string expected = @"(;AB[cc][gg][gc][ff][gf]AW[if]HA[3]PL[W]KM[7.50]SZ[9](;W[fh];B[gh])(;W[fi]))";
            string actual;
            actual = target.SerializeToSGF(null);
            Assert.AreEqual(expected, actual);
        }
    }
}
