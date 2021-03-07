using NUnit.Framework;
using DiscordBots.SQL;
using Nine.Commands;
using Nine;
using System.Threading.Tasks;

namespace Nine_Testing
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            VariableSetting.LaunchSettingsFixture();
        }

        #region WhoPlays
        [TestCase("Herpa", "Derpa", "I'm sorry, there were no records with that name.", Category = "Character")]
        [TestCase("Test Person", "The character Test Person is played by Test Player and pilots the Test Unit.", Category = "Character", Ignore = "Need to add records to the charinfo table")]
        public void WhoPlays_WithNonExistantCharacter_ShouldReturnMsg(string firstName, string lastName, string expectedResult)
        {
            string result = Characters.WhoPlays(firstName,lastName);

            Assert.AreEqual(expectedResult, result);
        }
        #endregion

        #region Add Thread/Add Post
        [TestCase("Test Thread 1", "https://srwignition.com/index.php?threads/hikaru-mizuki.375/", "Test1", Category = "Posts")]
        [TestCase("Test Thread 1", "https://srwignition.com/index.php?threads/hikaru-mizuki.375/", "Test1", Category = "Posts")]
        [TestCase("Test Thread 1", "https://srwignition.com/index.php?threads/hikaru-mizuki.373/", "Test2", Category = "Posts")]
        [TestCase("Test Thread 1", "https://srwignition.com/index.php?threads/hikaru-mizuki.373/", "Test2", Category = "Posts")]
        [TestCase("Test Thread 2", "https://srwignition.com/index.php?threads/hikaru-mizuki.378/", "Test1", Category = "Posts")]
        [TestCase("Test Thread 2", "https://srwignition.com/index.php?threads/hikaru-mizuki.378/", "Test1", Category = "Posts")]
        [TestCase("Test Thread 1", "https://srwignition.com/index.php?threads/hikaru-mizuki.378/", "Test1", Category = "Posts")]
        [TestCase("Test Thread 1", "https://srwignition.com/index.php?threads/hikaru-mizuki.378/", "Test1", Category = "Posts")]
        [TestCase("Test Thread 2", "https://srwignition.com/index.php?threads/hikaru-mizuki.375/", "Test2", Category = "Posts")]
        [TestCase("Test Thread 2", "https://srwignition.com/index.php?threads/hikaru-mizuki.375/", "Test2", Category = "Posts")]
        [TestCase("Test Thread 2", "https://srwignition.com/index.php?threads/hikaru-mizuki.375/", "Test1", Category = "Posts")]
        [TestCase("Test Thread 2", "https://srwignition.com/index.php?threads/hikaru-mizuki.375/", "Test1", Category = "Posts")]
        [TestCase("Test1", "https://fakeurl.com/index2.php", "Test Thread 1", Category = "Posts")]
        [TestCase("Test1", "https://fakeurl.com/index2.php", "Test Thread 1", Category = "Posts")]

        public void AddThread_WithExistingData_Should_ReturnErrMsg(string title, string url, string alias)
        {
            string expectedResult = $"A match has been found. Please remember that title and alias are both searched for both values since meatbags are flawed creatures that mix the two up sometimes.\nTitle: Test Thread 1\nAlias: Test1\nURL: https://srwignition.com/index.php?threads/hikaru-mizuki.375/";

            string result = Posts.AddThread(title, url, alias);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("Test Thread 3", "https://srwignition.com/index.php?threads/hikaru-mizuki.370/", "Test3", "The thread has been added to the database.", Category = "Posts")]
        [TestCase("Test Thread 3", "https://srwignition.com/index.php?threads/hikaru-mizuki.370/", " Test3", "The thread has been added to the database.", Category = "Posts")]
        public void AddThread_WithNonExistantData_Should_AddRecord(string title, string url, string alias, string expectedResult)
        {     

            string deleteRecord = "DELETE FROM `threads` WHERE Title = 'Test Thread 3'";
            //execute
            string result = Posts.AddThread(title, url, alias);

            //delete record
            SqlCommand.ExecuteQuery(deleteRecord, false);

            //pass/fail
            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion

        #region Update Post/Update Thread
        [TestCase("Test1","FakeStatus", Category = "Posts")]
        public void UpdatePost_WithBadStatus_Should_ReturnErrorMsg(string title, string status)
        {
            string expectedResult = $"The status you are trying to update with is not valid. Use one of the following: /nOpen/nComplete/nHiatus/nAbandoned";

            string result = Posts.UpdateThread(title, status);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("https://srwignition.com/index.php?threads/hikaru-mizuki.375/", "open", Category = "Posts")]
        public void UpdatePost_WithURL_ShouldReturnUniqueMsg(string url, string status)
        {
            string expectedResult = $"... Why would you try to update with the url- You have a perfectly good title and alias! *Sigh* Whatever, meatbag... I have updated the status of the thread.";

            string result = Posts.UpdateThread(url, status);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("Test1", "Hiatus", Category ="Posts")]
        [TestCase("Test1", "Abandoned", Category = "Posts")]
        [TestCase("Test Thread 1", "Complete", Category = "Posts")]
        [TestCase("Test Thread 1", "Open", Category = "Posts")]
        public void UpdatePost_Should_UpdateTheThread(string title, string status)
        {
            string expectedResult = $"I have updated the status of the thread.";

            if(status == "Complete")
            {
                expectedResult = $"The status complete can only be assigned with the ThreadComplete commands.";
            }

            string result = Posts.UpdateThread(title, status);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("NonExistantThread","Open", Category = "Posts")]
        public void UpdatePost_WithNonExistantIds_ShouldReturnErrorMsg(string title, string status)
        {
            string expectedResult = $"There is no thread with that identifier. Please try again.";

            string result = Posts.UpdateThread(title, status);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion

        #region Add To Post Order
        [TestCase("Test1",  "Fake", "<@!271158490800712345>", "Fake has already been added to the post order.", Category = "Posts")]
        [TestCase("Test1", "MK", "<@!149524235751129088>", "MK has already been added to the post order.", Category = "Posts")]
        [TestCase("Starfall", "MK", "<@!149524235751129088>", "There is no thread in the database with the Title or Alias 'Starfall'.", Category = "Posts")]
        public void AddToPostOrder_WithExistingArgs_ShouldReturnErrMsg(string Thread, string Mask, string Mention, string expectedResult)
        {           
            string result = Posts.AddToPostOrder(Thread, Mention, Mask);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("Test1","<@!226132112145645578>", "Sess", Category = "Posts")]
        public void AddToPostOrder_WithUniqueValues_ShouldReturnSuccess(string title, string user,string mask)
        {
            string expectedResult = "Sess has been added to the posting order";
            string deleteRecord = "DELETE From postorder where ThreadID='26' AND Player='<@!226132112145645578>'";

            string result = Posts.AddToPostOrder(title, user, mask);

            //delete record
            SqlCommand.ExecuteQuery(deleteRecord, false);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion

        #region Add Player
        [TestCase("<@!226132112145645578>", "Test", "That user has already been added.", Category = "Player")]
        [TestCase("<@!123456789012345678>", "Sess", "That monicker is already in use.", Category = "Player")]
        [TestCase("<@!123456789012345678>", "Test", "Player has been added.", Category = "Player")]
        [TestCase("<@!123456789012345678>", "Test Player", "Player has been added.", Category = "Player")]
        public void AddPlayer_Should_ReturnExpectedMsg(string player, string monicker, string expectedResult)
        {
            string deleteRecord = "DELETE From players where Player='<@!123456789012345678>'";

            string result = Player.AddPlayer(player, monicker);

            //delete record
            if (expectedResult == "Player has been added.")
            {
                SqlCommand.ExecuteQuery(deleteRecord, false);
            }

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion

        #region Post Order        
        [TestCase("Test5", "The posting order for that thread has not yet been established.", Category = "Posts")]
        [TestCase("Fake Thread", "There are no threads under title or alias with the text Fake Thread", Category = "Posts")]
        [TestCase("Post Order", "The order for Post Order is as follows:\n1: Fake\n2: MK\n3: Sess\n4: Tyr", Category = "Posts")]
        public void PostOrder_Should_ReturnMsg(string thread, string expectedMessage)
        {
            string result = Posts.PostOrder(thread);

            StringAssert.AreEqualIgnoringCase(expectedMessage, result);
        }
        #endregion

        #region Remove From Post Order
        [TestCase("fake","MK", 2, Category = "Posts")]
        [TestCase("fake", "<@!149524235751129088>", 2, Category = "Posts")]
        [TestCase("Remove Testing", "<@!123456789012345678>", 3, Category = "Posts")]
        [TestCase("Remove Post", "<@!149524235751129088>", 4, Category = "Posts")]
        public void RemoveFromPostOrder_Should_ReturnErrMsg(string thread, string user, int errMsgNum)
        {
            string expectedMessage = "";
            string result = Posts.RemoveFromOrder(thread, user);

            switch (errMsgNum)
            {              
                case 2:
                    expectedMessage = "Player 'MK' not found in order.";
                    break;
                case 3:
                    expectedMessage = "Player not found in database, please add to database before attempting to remove from order.";
                    break;
                case 4:
                    expectedMessage = "There are no threads under title or alias with the text Remove Post";
                    break;
            }

            StringAssert.AreEqualIgnoringCase(expectedMessage, result);
        }
        #endregion

        #region Reset Post Order
        [Test]
        [Category("Posts")]
        public void ResetPostOdrer_WithNonExistantThread_Should_ThrowErrMsg()
        {
            string thread = "fake thread";
            string expectedMessage = "The thread 'fake thread' was not found in the table.";
            string result = Posts.ResetPostOrder(thread);
            StringAssert.AreEqualIgnoringCase(expectedMessage, result);
        }
        #endregion

        #region Who's Up / Ping
        [Test]
        [Category("Posts")]
        public void WhosUp_Should_ReturnFirstInPostOrder()
        {
            string title = "Remove Testing";
            string expectedMessage = "Currently up to post in Remove Testing is MK";

            string result = Posts.UpNext(title, false);
            StringAssert.AreEqualIgnoringCase(expectedMessage, result);
        }
        #endregion

        #region SetFirstName
        [TestCase("Test", Category = "Character")]
        [TestCase("quit", Category = "Character")]
        public void SetFirstName_Should_SetVariables(string msg)
        {
            bool correctSetVal = true;
            CharInfo info = new CharInfo();

            info = Characters.FirstName(info, msg);

            if (msg == "quit")
            {
                if(!info.Quit)
                {
                    correctSetVal = false;
                }
            }
            else
            {
                if(info.FirstName != msg)
                {
                    correctSetVal = false;
                }
            }

            Assert.IsTrue(correctSetVal);
        }
        #endregion

        #region Set Last Name
        [TestCase("Test", Category = "Character")]
        [TestCase("quit", Category = "Character")]
        [TestCase("Template", Category = "Character")]
        public void SetLastName_Should_SetVariables(string msg)
        {
            bool correctSetVal = true;
            CharInfo info = new CharInfo
            {
                FirstName = "Character"
            };

            info = Characters.LastName(info, msg);

            if (msg == "quit")
            {
                if (!info.Quit)
                {
                    correctSetVal = false;
                }
            }else if (msg == "Template")
            {
                if(!info.Errored)
                {
                    correctSetVal = false;
                }
            }
            else
            {
                if (info.LastName != "Test")
                {
                    correctSetVal = false;
                }
            }

            Assert.IsTrue(correctSetVal);
        }
        #endregion

        #region Set Gender
        [TestCase("Male")]
        [TestCase("Female")]
        [TestCase("quit")]
        [Category("Character")]
        public void SetGender_Should_SetVariables(string msg)
        {
            bool correctSetVal = true;
            CharInfo info = new CharInfo();

            info = Characters.Gender(info, msg);

            if (msg == "quit")
            {
                if (!info.Quit)
                {
                    correctSetVal = false;
                }
            }
            else
            {
                if (info.Gender != msg)
                {
                    correctSetVal = false;
                }
            }

            Assert.IsTrue(correctSetVal);

        }
        #endregion

        #region Set Unit
        [TestCase("Genion")]
        [TestCase("Supreme God Z")]
        [TestCase("Adamatron")]
        [TestCase("Mazinkaiser")]
        [TestCase("FakeUnit")]
        [Category("Character")]
        public void SetUnit_Should_SetVariables(string msg)
        {
            CharInfo info = new CharInfo();
            bool correctSetVal = true;

            info = Characters.Unit(info, msg);

            switch(msg)
            {
                case "Genion":
                    if (info.Unit != msg)
                    {
                        correctSetVal = false;
                    }
                    break;

                case "Supreme God Z":
                case "Adamatron":
                case "Mazinkaiser":
                case "FakeUnit":
                    if(!info.Errored)
                    {
                        correctSetVal = false;
                    }
                    break;
            }

            Assert.IsTrue(correctSetVal);            
        }
        #endregion

        #region Set Faction
        [TestCase("Test", Category = "Character")]
        [TestCase("quit", Category = "Character")]
        [TestCase("Independent", Category = "Character")]
        public void SetFaction_Should_SetVariables(string msg)
        {
            bool correctSetVal = true;
            CharInfo info = new CharInfo();

            info = Characters.Faction(info, msg);

            if (msg == "quit")
            {
                if (!info.Quit)
                {
                    correctSetVal = false;
                }
            }
            else if (msg == "Test")
            {
                if (!info.Errored)
                {
                    correctSetVal = false;
                }
            }
            else
            {
                if (info.Faction != "Independent")
                {
                    correctSetVal = false;
                }
            }

            Assert.IsTrue(correctSetVal);
        }
        #endregion

        #region Set URL
        [TestCase("Test")]
        [TestCase("Ignition.com")]
        [TestCase("https://srwignition.com/index.php?threads/sign-up-sheet.3/")]
        [TestCase("https://srwignition.com/index.php?threads/rai-sol.393/")]
        [Category("Character")]
        public async Task SetURL_Should_SetVariablesAsync(string msg)
        {
            bool correctSetVal = true;
            CharInfo info = new CharInfo();

            info = await Characters.URL(info, msg);

            if(msg.Contains("rai-sol"))
            {
                if(info.Url != msg)
                {
                    correctSetVal = false;
                }
            } else
            {
                if(!info.Errored)
                {
                    correctSetVal = false;
                }
            }

            Assert.IsTrue(correctSetVal);
        }
        #endregion

        #region Add Unit
        [TestCase("Genion", "Sess", Units.UnitStatus.Open)]
        [TestCase("TestFake", "Sess", Units.UnitStatus.Reserved)]
        [TestCase("TestFake", "Sess", Units.UnitStatus.Taken)]
        [Category("Units")]
        public void AddUnit_Should_ReturnExpectedResult(string Unit, string AddedBy, Units.UnitStatus Status, string ReservedFor = "")
        {
            string result = Units.AddUnit(Unit, AddedBy, Status, ReservedFor);
            string expectedResult = "";

            if (Unit == "Genion")
            {
                expectedResult = $"'{Unit}' has already been added to the database.";
            } else if (Unit == "TestFake")
            {
                if (Status == Units.UnitStatus.Reserved)
                {
                    expectedResult = "Since you are trying to reserve a machine, you need to tell me who you're reserving it for by their monicker registered in the player database.";
                } else
                {
                    expectedResult = "Since you are trying to assign a machine, you need to tell me which player it is assigned to by their monicker registered in the player database.";
                }
            }

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion

        #region Update Unit
        [TestCase("TestUnit", Units.UnitStatus.Reserved)]
        [TestCase("TestUnit", Units.UnitStatus.Reserved, "Tyr")]
        [TestCase("TestUnit", Units.UnitStatus.Taken)]
        [TestCase("TestUnit", Units.UnitStatus.Open)]
        [TestCase("TotalFake", Units.UnitStatus.Open)]
        [Category("Units")]

        public void UpdateUnit_Should_ReturnExpectedResult(string Unit, Units.UnitStatus Status, string ReservedFor = "")
        {
            string result = Units.UpdateUnitStatus(Unit, Status, ReservedFor);
            string expectedResult= "";

            if(Status == Units.UnitStatus.Reserved && ReservedFor == "")
            {
                expectedResult = "Since you are trying to reserve a machine, you need to tell me who you're reserving it for by their monicker registered in the player database.";
            } else if(Unit == "TestUnit")
            {
                expectedResult = $"I have updated the status of the unit to {Status}.";
            } else if (Unit == "TotalFake")
            {
                expectedResult = "I can't seem to find that unit in my database. You sure it's been added?";
            }

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion

        #region Faction
        [TestCase("Independent")]
        [TestCase("Fake")]
        [Category("Faction")]
        public void Faction_ShouldReturn_Results(string Faction)
        {
            string result = Factions.Faction(Faction);

            string expectedResult;

            if (Faction == "Independent")
            {
                expectedResult = "Faction: Independent \n     Leader: None\n     Active     \n";
            }
            else
            {
                expectedResult = "That faction is not in my records.";
            }

            StringAssert.AreEqualIgnoringCase(expectedResult, result);

        }
        #endregion

        #region Add Faction
        [TestCase("Lunar Kingdom of Everglory", "Fake", "Character", "NoUrl")]
        [TestCase("Independent", "Fake", "Character", "NoURL")]
        [Category("Faction")]
        public void AddFaction_Should_ReturnExpectedResults(string Faction, string First, string Last, string URL)
        {
            string result = Factions.AddFaction(Faction, First, Last, URL);
            string expectedResult;

            if(Faction == "Independent")
            {
                expectedResult = "That faction already exists in my records.";
            } else
            {
                expectedResult = "The character specified is not yet in the records. Please add them and try again.";
            }

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion

        #region Definition
        [TestCase("Test Term")]
        [TestCase("Fake Term")]
        [Category("Dictionary")]
        public void Definition_Should_ReturnResults(string Term)
        {
            string result = Dictionary.Definition(Term);

            string expectedResult;

            if(Term == "Test Term")
            {
                expectedResult = "Test Term - \n(1) - Test Definition 2";
            } else
            {
                expectedResult = "This term is not in the dictionary yet.";
            }

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion

        #region New Term
        [TestCase("Test Term", "Test Definition", false)]
        [Category("Dictionary")]
        public void NewTerm_Should_ReturnExpectedResults(string Term, string Definition, bool addAnyway)
        {
            string result = Dictionary.NewTerm(Term, Definition, addAnyway);

            string expectedResults = $"This term is already in the database with the following definitions: \n(1) - Test Definition 2";

            StringAssert.AreEqualIgnoringCase(expectedResults, result);
        }
        #endregion

        #region Update Definition
        [TestCase("Fake Term", "Fake Def", "1")]
        [Category("Dictionary")]
        public void UpdateDefinition_Should_ReturnExpectedResults(string Term, string Definition, int DefNum)
        {
            string result = Dictionary.UpdateDefinition(Term, Definition, DefNum);

            string expectedResult = "That term is not present in the dictionary at this time.";

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion
    }
}