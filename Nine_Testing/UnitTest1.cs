using NUnit.Framework;
using DiscordBots.SQL;

namespace Nine_Testing
{
    public class Tests
    {
        private static readonly string author = "<@!605539684508237849>"; //verit
        private static readonly string adminAuth = "<@!226132112145645578>"; //sess

        [SetUp]
        public void Setup()
        {
            VariableSetting.LaunchSettingsFixture();
        }

        #region Base Commands
        [TestCase("9 ping", Category = "Base Commands")]
        [TestCase("9ping", Category = "Base Commands")]
        public void Ping_ShouldReturn_ExpectedResult(string message)
        {
            string expectedResult = "Do I look like an old Atari to you?";
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("9 FakeCommand", Category = "Base Commands")]
        [TestCase("9FakeCommand", Category = "Base Commands")]
        public void Nine_ShouldReturn_ExpectedResult(string message)
        {
            string expectedResult = "No command exists for FakeCommand";
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            Assert.AreEqual(expectedResult, result);
        }
        #endregion

        #region WhoPlays
        [TestCase("9 whoplays", Category = "Character Commands")]
        public void WhoPlays_WithNoArgs_ShouldReturnErrMsg(string message)
        {
            string expectedResult = "Your request was put in the wrong format. Correct format is 9 whoplays <character name>";
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("9 whoplays Test", Category = "Character Commands")]
        public void WhoPlays_WithBroadArg_ShouldReturnMsg(string message)
        {
            string expectedResult = "There are multiple results containing the name Test, please try again and narrow down your search.";
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("9 whoplays HerpaDerpa", Category = "Character Commands")]
        public void WhoPlays_WithNonExistantCharacter_ShouldReturnMsg(string message)
        {
            string expectedResult = "I'm sorry, there were no records with that name.";
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("9 Whoplays Test Person", Category = "Character Commands")]
        public void WhoPlays_WithExistantCharacter_ShouldReturnMsg(string message)
        {
            string expectedResult = "The character Test Person is played by Test Player and pilots the Test Unit.";
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            Assert.AreEqual(expectedResult, result);
        }
        #endregion

        #region Add Thread/Add Post
        [TestCase("9 AddThread", Category = "Posts")]

        public void AddThread_NoArgs_Should_ReturnErrMsg(string message)
        {
            string expectedResult = $"One or more fields were empty. Please try again.";
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 AddThread https://fakeurl.com/index.php Test1 Test Thread", "AddThread", Category = "Posts")]
        [TestCase("9 AddPost https://fakeurl.com/index.php Test1 Test Thread", "AddPost", Category = "Posts")]
        public void AddThread_WrongFormat_Should_ReturnErrMsg(string message, string command)
        {
            string expectedResult = $"Your request was put in the wrong format. Correct format is 9 {command} <thread title> <url> <alias>";
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 AddThread TestThread https://fakeurl.com/index.php ", Category = "Posts")]
        [TestCase("9 AddPost TestThread https://fakeurl.com/index.php", Category = "Posts")]
        [TestCase("9 AddThread https://fakeurl.com/index.php Test1", Category = "Posts")]
        [TestCase("9 AddPost https://fakeurl.com/index.php Test1", Category = "Posts")]
        [TestCase("9 AddThread TestThread", Category = "Posts")]
        [TestCase("9 AddPost TestThread", Category = "Posts")]
        [TestCase("9 AddThread https://fakeurl.com/index.php",  Category = "Posts")]
        [TestCase("9 AddPost https://fakeurl.com/index.php",  Category = "Posts")]
        [TestCase("9 AddThread Test1", Category = "Posts")]
        [TestCase("9 AddPost Test1", Category = "Posts")]

        public void AddThread_MissingArgs_Should_ReturnErrMsg(string message)
        {
            string expectedResult = $"One or more fields were empty. Please try again.";
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("addthread", "Test Thread 1", "https://srwignition.com/index.php?threads/hikaru-mizuki.375/", "Test1", Category = "Posts")]
        [TestCase("AddPost", "Test Thread 1", "https://srwignition.com/index.php?threads/hikaru-mizuki.375/", "Test1", Category = "Posts")]
        [TestCase("addthread", "Test Thread 1", "https://srwignition.com/index.php?threads/hikaru-mizuki.373/", "Test2", Category = "Posts")]
        [TestCase("AddPost", "Test Thread 1", "https://srwignition.com/index.php?threads/hikaru-mizuki.373/", "Test2", Category = "Posts")]
        [TestCase("addthread", "Test Thread 2", "https://srwignition.com/index.php?threads/hikaru-mizuki.378/", "Test1", Category = "Posts")]
        [TestCase("AddPost", "Test Thread 2", "https://srwignition.com/index.php?threads/hikaru-mizuki.378/", "Test1", Category = "Posts")]
        [TestCase("addthread", "Test Thread 1", "https://srwignition.com/index.php?threads/hikaru-mizuki.378/", "Test1", Category = "Posts")]
        [TestCase("AddPost", "Test Thread 1", "https://srwignition.com/index.php?threads/hikaru-mizuki.378/", "Test1", Category = "Posts")]
        [TestCase("addthread", "Test Thread 2", "https://srwignition.com/index.php?threads/hikaru-mizuki.375/", "Test2", Category = "Posts")]
        [TestCase("AddPost", "Test Thread 2", "https://srwignition.com/index.php?threads/hikaru-mizuki.375/", "Test2", Category = "Posts")]
        [TestCase("addthread", "Test Thread 2", "https://srwignition.com/index.php?threads/hikaru-mizuki.375/", "Test1", Category = "Posts")]
        [TestCase("AddPost", "Test Thread 2", "https://srwignition.com/index.php?threads/hikaru-mizuki.375/", "Test1", Category = "Posts")]
        [TestCase("addthread", "Test Thread 2", "https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099", "Test1", Category = "Posts")]
        [TestCase("AddPost", "Test Thread 2", "https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099", "Test1", Category = "Posts")]
        [TestCase("AddThread", "Test1", "https://fakeurl.com/index2.php", "Test Thread 1", Category = "Posts")]
        [TestCase("AddPost", "Test1", "https://fakeurl.com/index2.php", "Test Thread 1", Category = "Posts")]

        public void AddThread_WithExistingData_Should_ReturnErrMsg(string command, string title, string url, string alias)
        {
            string message = $"9 {command} {title} {url} {alias}";

            string expectedResult = $"A match has been found. Please remember that title and alias are both searched for both values since meatbags are flawed creatures that mix the two up sometimes.\nTitle: Test Thread 1\nAlias: Test1\nURL: https://srwignition.com/index.php?threads/hikaru-mizuki.375/";

            if(url.Contains("#post-"))
            {
                expectedResult = "URL Cannot be linked to a specific post for threads.";
            }
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 addthread Test Thread 3 https://srwignition.com/index.php?threads/hikaru-mizuki.370/ Test3", "The thread has been added to the database.", Category = "Posts")]
        [TestCase("9 Addpost Test Thread 3 https://srwignition.com/index.php?threads/hikaru-mizuki.370/ Test3", "The thread has been added to the database.", Category = "Posts")]
        public void AddThread_WithNonExistantData_Should_AddRecord(string message, string expectedResult)
        {     

            string deleteRecord = "DELETE From threads where Alias='Test3'";
            //execute
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            //delete record
            SqlCommand.ExecuteQuery(deleteRecord, false);

            //pass/fail
            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion

        #region Update Post/Update Thread
        [TestCase("9 updatepost", "updatepost", Category = "Posts")]
        [TestCase("9 updatethread", "updatethread", Category ="Posts")]
        [TestCase("9 UpdatePost Test1", "UpdatePost", Category = "Posts")]
        [TestCase("9 updatethread Test1", "updatethread", Category = "Posts")]
        public void UpdatePost_WithNoArgs_Should_ReturnErrMsg(string message, string command)
        {
            string expectedResult = $"Your request was put in the wrong format. Correct format is 9 {command} <thread title or alias> <thread satus>";

            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);

        }

        [TestCase("9 UpdatePost Test1 FakeStatus", Category = "Posts")]
        [TestCase("9 updatethread Test1 FakeStatus", Category = "Posts")]
        public void UpdatePost_WithBadStatus_Should_ReturnErrorMsg(string message)
        {
            string expectedResult = $"The status you are trying to update with is not valid. Use one of the following: /nOpen/nComplete/nHiatus/nAbandoned";

            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 updatepost https://srwignition.com/index.php?threads/hikaru-mizuki.375/ open")]
        [TestCase("9 updatethread https://srwignition.com/index.php?threads/hikaru-mizuki.375/ complete")]
        public void UpdatePost_WithURL_ShouldReturnUniqueMsg(string message)
        {
            string expectedResult = $"... Why would you try to update with the url- You have a perfectly good title and alias! *Sigh* Whatever, meatbag... I have updated the status of the thread.";

            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 updatepost Test1 Hiatus", Category ="Posts")]
        [TestCase("9 updatethread Test1 Abandoned", Category = "Posts")]
        [TestCase("9 updatepost Test Thread 1 Complete", Category = "Posts")]
        [TestCase("9 updatethread Test Thread 1 Hiatus", Category = "Posts")]
        public void UpdatePost_Should_UpdateTheThread(string message)
        {
            string expectedResult = $"I have updated the status of the thread.";

            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 updatepost NonExistantThread Open", Category = "Posts")]
        [TestCase("9 updatethread NonExistantThread Open", Category = "Posts")]
        public void UpdatePost_WithNonExistantIds_ShouldReturnErrorMsg(string message)
        {
            string expectedResult = $"There is no thread with that identifier. Please try again.";

            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion

        #region Add To Post Order
        [TestCase("9 addtopostorder",1, Category = "Posts")]
        [TestCase("9 addtopostorder @!1234567 1", 1, Category = "Posts")]
        [TestCase("9 addtopostorder Test1 1", 1, Category = "Posts")]
        [TestCase("9 addtopostorder Test Thread 1 1", 2, Category = "Posts")]
        [TestCase("9 addtopostorder Starfall 1", 1, Category = "Posts")]
        [TestCase("9 addtopostorder Test1 1 123456 1", 2, Category = "Posts")]
        [TestCase("9 addtopostorder Test1 1 <@!226132112145645578> z", 3, Category = "Posts")]
        public void AddToPostOrder_WithBadArgs_ShouldReturnErrMsg(string message, int errMsg)
        {
            string expectedResult = "";

            switch(errMsg)
            {
                case 1:
                    expectedResult = "Your request was put in the wrong format. Correct format is 9 AddToPostOrder <thread title or alias> <@player> <post order position>";
                    break;
                case 2:
                    expectedResult = "User mentioned not found in the database. Please add them before you add them to posts.";
                    break;
                case 3:
                    expectedResult = $"Your request was put in the wrong format. Correct format is 9 AddToPostOrder <thread title or alias> <@player> <post order position>. And if you add in a non numeric character for position again I will send spiderbots to watch you in your sleep.";
                    break;
            }

            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 addtopostorder Test1 <@!271158490800712345> 1", "Fake has already been added to the post order.", Category = "Posts")]
        [TestCase("9 addtopostorder Test1 <@!149524235751129088> 1", "1 has already been added to the post order.", Category = "Posts")]
        [TestCase("9 addtopostorder Starfall <@!149524235751129088> 1", "There is no thread in the database with the Title or Alias 'Starfall'.", Category = "Posts")]
        public void AddToPostOrder_WithExistingArgs_ShouldReturnErrMsg(string message, string expectedResult)
        {
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 addtopostorder Test1 <@!226132112145645578> 99")]
        [TestCase("9 addtopostorder Test1 Sess 99")]
        public void AddToPostOrder_WithUniqueValues_ShouldReturnSuccess(string message)
        {
            string expectedResult = "Sess has been added to the posting order";
            string deleteRecord = "DELETE From postorder where ThreadID='26' AND PostPosition='99'";

            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            //delete record
            SqlCommand.ExecuteQuery(deleteRecord, false);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion

        #region Add Player
        [TestCase("9 addplayer <@!123456789012345678>", Category = "Players")]
        [TestCase("9 addplayer Sess", Category ="Players")]
        [TestCase("9 addplayer Test Test1", Category= "Players")]
        public void AddPlayer_WithBadArgs_ShouldReturnErrMsg(string message)
        {
            string expectedMessage = "Invalid argument format, correct format is 9 addplayer @<player> monicker";

            if(!message.Contains("@!") && 
                message.Split(" ").Length > 3)
            {
                expectedMessage = "You must mention the player you are adding for the records.";
            }

            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            StringAssert.AreEqualIgnoringCase(expectedMessage, result);
        }

        [TestCase("<@!226132112145645578>", "Test", "That user has already been added.")]
        [TestCase("<@!123456789012345678>", "Sess", "That monicker is already in use.")]
        [TestCase("<@!123456789012345678>", "Test", "Player has been added.")]
        [TestCase("<@!123456789012345678>", "Test Player", "Player has been added.")]
        public void AddPlayer_Should_ReturnExpectedMsg(string player, string monicker, string expectedResult)
        {
            string deleteRecord = "DELETE From players where Player='<@!123456789012345678>'";

            string message = $"9 addplayer {player} {monicker}";
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            //delete record
            if (expectedResult == "Player has been added.")
            {
                SqlCommand.ExecuteQuery(deleteRecord, false);
            }

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion

        #region Post Order
        
        [TestCase("9 PostOrder", "The command is in the incorrect format. Format should be 9 PostOrder <thread name|alias>", Category = "Posts")]
        [TestCase("9 PostOrder Test5", "The posting order for that thread has not yet been established.", Category = "Posts")]
        [TestCase("9 PostOrder Fake Thread", "There are no threads under title or alias with the text Fake Thread", Category = "Posts")]
        [TestCase("9 postorder Post Order", "The order for Post Order is as follows:\n1: Fake\n2: MK\n3: Sess\n4: Tyr", Category = "Posts")]
        public void PostOrder_Should_ReturnMsg(string message, string expectedMessage)
        {
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            StringAssert.AreEqualIgnoringCase(expectedMessage, result);
        }
        #endregion

        #region Remove From Post Order
        [TestCase("9 RemoveFromPostOrder", 1)]
        [TestCase("9 RemoveFromPostOrder Test1 MK", 2)]
        [TestCase("9 RemoveFromPostOrder Test1 <@!149524235751129088>", 2)]
        [TestCase("9 RemoveFromPostOrder Remove Testing", 1)]
        [TestCase("9 RemoveFromPostOrder Remove Testing <@!123456789012345678>", 3)]
        [TestCase("9 RemoveFromPostOrder Remove Post <@!149524235751129088>", 4)]
        public void RemoveFromPostOrder_Should_ReturnErrMsg(string message, int errMsgNum)
        {
            string expectedMessage = "";
            string result = Nine.Commands.Commands.ExecCommand("9", message, author);

            switch (errMsgNum)
            {
                case 1:
                    expectedMessage = "The command is in the incorrect format. Format should be 9 RemoveFromPostOrder <thread> player";
                    break;
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
        public void ResetPostOrder_WithNotEnoughArgs_Should_ThrowErrMsg()
        {
            string message = "9 resetpostorder";
            string expectedMessage = "Incorrect command format, correct format is 9 resetpostorder <thread>.";
            string result = Nine.Commands.Commands.ExecCommand("9", message, adminAuth);
            StringAssert.AreEqualIgnoringCase(expectedMessage, result);
        }

        [Test]
        public void ResetPostOdrer_WithNonExistantThread_Should_ThrowErrMsg()
        {
            string message = "9 resetpostorder fake thread";
            string expectedMessage = "The thread 'fake thread' was not found in the table.";
            string result = Nine.Commands.Commands.ExecCommand("9", message, adminAuth);
            StringAssert.AreEqualIgnoringCase(expectedMessage, result);
        }

        [Test]
        public void ResetPostOdrer_WithRegularUser_Should_ThrowErrMsg()
        {
            string message = "9 resetpostorder fake thread";
            string expectedMessage = "You do not have permissions to execute that command.";

            string result = Nine.Commands.Commands.ExecCommand("9", message, author);
            StringAssert.AreEqualIgnoringCase(expectedMessage, result);
        }
        #endregion

        #region Who's Up / Ping
        [Test]
        public void WhosUp_Should_ReturnFirstInPostOrder()
        {
            string message = "9 whosup Remove Testing";
            string expectedMessage = "Currently up to post in Remove Testing is Sess";

            string result = Nine.Commands.Commands.ExecCommand("9", message, author);
            StringAssert.AreEqualIgnoringCase(expectedMessage, result);
        }
        #endregion

        #region Posted
        [Test]
        public void Posted_Should_ReturnTagForNextInOrder()
        {
            string message = "9 posted Remove Testing";
            string expectedMessage = "Thank you for your post, Sess. You're up, MK";

            string result = Nine.Commands.Commands.ExecCommand("9", message, adminAuth);
            StringAssert.AreEqualIgnoringCase(expectedMessage, result);
        }
        #endregion
    }
}