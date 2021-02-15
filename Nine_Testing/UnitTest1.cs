using NUnit.Framework;
using DiscordBots.SQL;

namespace Nine_Testing
{
    public class Tests
    {
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
            string result = Nine.Commands.Commands.ExecCommand("9", message);

            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("9 FakeCommand", Category = "Base Commands")]
        [TestCase("9FakeCommand", Category = "Base Commands")]
        public void Nine_ShouldReturn_ExpectedResult(string message)
        {
            string expectedResult = "No command exists for FakeCommand";
            string result = Nine.Commands.Commands.ExecCommand("9", message);

            Assert.AreEqual(expectedResult, result);
        }
        #endregion

        #region WhoPlays
        [TestCase("9 whoplays", Category = "Character Commands")]
        public void WhoPlays_WithNoArgs_ShouldReturnErrMsg(string message)
        {
            string expectedResult = "Your request was put in the wrong format. Correct format is 9 whoplays <character name>";
            string result = Nine.Commands.Commands.ExecCommand("9", message);

            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("9 whoplays Test", Category = "Character Commands")]
        public void WhoPlays_WithBroadArg_ShouldReturnMsg(string message)
        {
            string expectedResult = "There are multiple results containing the name Test, please try again and narrow down your search.";
            string result = Nine.Commands.Commands.ExecCommand("9", message);

            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("9 whoplays HerpaDerpa", Category = "Character Commands")]
        public void WhoPlays_WithNonExistantCharacter_ShouldReturnMsg(string message)
        {
            string expectedResult = "I'm sorry, there were no records with that name.";
            string result = Nine.Commands.Commands.ExecCommand("9", message);

            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("9 Whoplays Test Person", Category = "Character Commands")]
        public void WhoPlays_WithExistantCharacter_ShouldReturnMsg(string message)
        {
            string expectedResult = "The character Test Person is played by Test Player and pilots the Test Unit.";
            string result = Nine.Commands.Commands.ExecCommand("9", message);

            Assert.AreEqual(expectedResult, result);
        }
        #endregion

        #region Add Thread/Add Post
        [TestCase("9 AddThread", "AddThread", Category = "Posts")]

        public void AddThread_NoArgs_Should_ReturnErrMsg(string message, string command)
        {
            string expectedResult = $"One or more fields were empty. Please try again.";
            string result = Nine.Commands.Commands.ExecCommand("9", message);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 AddThread https://fakeurl.com/index.php Test1 Test Thread", "AddThread", Category = "Posts")]
        [TestCase("9 AddPost https://fakeurl.com/index.php Test1 Test Thread", "AddPost", Category = "Posts")]
        [TestCase("9 AddThread Test1 https://fakeurl.com/index.php Test Thread", "AddThread", Category = "Posts")]
        [TestCase("9 AddPost Test1 https://fakeurl.com/index.php Test Thread", "AddPost", Category = "Posts")]
        public void AddThread_WrongFormat_Should_ReturnErrMsg(string message, string command)
        {
            string expectedResult = $"Your request was put in the wrong format. Correct format is 9 {command} <thread title> <url> <alias>";
            string result = Nine.Commands.Commands.ExecCommand("9", message);

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
            string result = Nine.Commands.Commands.ExecCommand("9", message);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 addthread Test Thread 1 https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099 Test1", "The title, url, and alias are already in my records.", Category = "Posts")]
        [TestCase("9 AddPost Test Thread 1 https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099 Test1", "The title, url, and alias are already in my records.", Category = "Posts")]

        [TestCase("9 addthread Test Thread 1 https://srwignition.com/index.php?threads/hikaru-mizuki.373/#post-3099 Test2", "The title of the thread you are trying to add is already in the database under the alias Test1 with the url https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099.", Category = "Posts")]
        [TestCase("9 AddPost Test Thread 1 https://srwignition.com/index.php?threads/hikaru-mizuki.373/#post-3099 Test2", "The title of the thread you are trying to add is already in the database under the alias Test1 with the url https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099.", Category = "Posts")]

        [TestCase("9 addthread Test Thread 2 https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3093 Test1", "The alias you are trying to use is already taken for the thread Test Thread 1 with the url https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099.", Category = "Posts")]
        [TestCase("9 AddPost Test Thread 2 https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3093 Test1", "The alias you are trying to use is already taken for the thread Test Thread 1 with the url https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099.", Category = "Posts")]

        [TestCase("9 addthread Test Thread 1 https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3093 Test1", "The alias and title are already in use for url https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099.", Category = "Posts")]
        [TestCase("9 AddPost Test Thread 1 https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3093 Test1", "The alias and title are already in use for url https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099.", Category = "Posts")]

        [TestCase("9 addthread Test Thread 2 https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099 Test2", "The url you are trying to use is already taken for the thread Test Thread 1 with the alias Test1.", Category = "Posts")]
        [TestCase("9 AddPost Test Thread 2 https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099 Test2", "The url you are trying to use is already taken for the thread Test Thread 1 with the alias Test1.", Category = "Posts")]

        [TestCase("9 addthread Test Thread 2 https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099 Test1", "The alias and url are already in use for the thread Test Thread 1.", Category = "Posts")]
        [TestCase("9 AddPost Test Thread 2 https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099 Test1", "The alias and url are already in use for the thread Test Thread 1.", Category = "Posts")]
        public void AddThread_WithExistingData_Should_ReturnErrMsg(string message, string expectedResult)
        {
            string result = Nine.Commands.Commands.ExecCommand("9", message);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 addthread Test Thread 3 https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3092 Test3", "The thread has been added to the database.", Category = "Posts")]
        [TestCase("9 Addpost Test Thread 3 https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3092 Test3", "The thread has been added to the database.", Category = "Posts")]
        public void AddThread_WithNonExistantData_Should_AddRecord(string message, string expectedResult)
        {
            string deleteRecord = "DELETE From threads where Alias='Test3'";
            //execute
            string result = Nine.Commands.Commands.ExecCommand("9", message);

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

            string result = Nine.Commands.Commands.ExecCommand("9", message);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);

        }

        [TestCase("9 UpdatePost Test1 FakeStatus", Category = "Posts")]
        [TestCase("9 updatethread Test1 FakeStatus", Category = "Posts")]
        public void UpdatePost_WithBadStatus_Should_ReturnErrorMsg(string message)
        {
            string expectedResult = $"The status you are trying to update with is not valid. Use one of the following: /nOpen/nComplete/nHiatus/nAbandoned";

            string result = Nine.Commands.Commands.ExecCommand("9", message);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 updatepost https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099 open")]
        [TestCase("9 updatethread https://srwignition.com/index.php?threads/hikaru-mizuki.375/#post-3099 complete")]
        public void UpdatePost_WithURL_ShouldReturnUniqueMsg(string message)
        {
            string expectedResult = $"... Why would you try to update with the url- You have a perfectly good title and alias! *Sigh* Whatever, meatbag... I have updated the status of the thread.";

            string result = Nine.Commands.Commands.ExecCommand("9", message);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 updatepost Test1 Hiatus", Category ="Posts")]
        [TestCase("9 updatethread Test1 Abandoned", Category = "Posts")]
        [TestCase("9 updatepost Test Thread 1 Complete", Category = "Posts")]
        [TestCase("9 updatethread Test Thread 1 Hiatus", Category = "Posts")]
        public void UpdatePost_Should_UpdateTheThread(string message)
        {
            string expectedResult = $"I have updated the status of the thread.";

            string result = Nine.Commands.Commands.ExecCommand("9", message);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 updatepost NonExistantThread Open", Category = "Posts")]
        [TestCase("9 updatethread NonExistantThread Open", Category = "Posts")]
        public void UpdatePost_WithNonExistantIds_ShouldReturnErrorMsg(string message)
        {
            string expectedResult = $"There is no thread with that identifier. Please try again.";

            string result = Nine.Commands.Commands.ExecCommand("9", message);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion

        #region Add To Post Order
        [TestCase("9 addtopostorder", Category = "Posts")]
        [TestCase("9 addtopostorder @!1234567 1", Category = "Posts")]
        [TestCase("9 addtopostorder Test1 1", Category = "Posts")]
        [TestCase("9 addtopostorder Test Thread 1 1", Category = "Posts")]
        [TestCase("9 addtopostorder Starfall 1", Category = "Posts")]
        [TestCase("9 addtopostorder Test1 1 123456 1", Category = "Posts")]
        [TestCase("9 addtopostorder Test1 1 @!1234567 z", Category = "Posts")]
        public void AddToPostOrder_WithBadArgs_ShouldReturnErrMsg(string message)
        {
            string expectedResult = "Your request was put in the wrong format. Correct format is 9 AddToPostOrder <thread title or alias> <@player> <post order position>";

            if(message.Contains('z'))
            {
                expectedResult = $"Your request was put in the wrong format. Correct format is 9 AddToPostOrder <thread title or alias> <@player> <post order position>. And if you add in a non numeric character for position again I will send spiderbots to watch you in your sleep.";
            }

            string result = Nine.Commands.Commands.ExecCommand("9", message);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 addtopostorder Test1 <@!271158490800717824> 1", "<@!271158490800717824> has already been added to the post order.", Category = "Posts")]
        [TestCase("9 addtopostorder Test1 <@!271158490800712345> 1", "1 has already been added to the post order.", Category = "Posts")]
        [TestCase("9 addtopostorder Starfall <@!271158490800717824> 1", "There is no thread in the database with the Title or Alias 'Starfall'.", Category = "Posts")]
        public void AddToPostOrder_WithExistingArgs_ShouldReturnErrMsg(string message, string expectedResult)
        {
            string result = Nine.Commands.Commands.ExecCommand("9", message);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }

        [TestCase("9 addtopostorder Test1 <@!271158490800712345> 99")]
        public void AddToPostOrder_WithUniqueValues_ShouldReturnSuccess(string message)
        {
            string expectedResult = "<@!271158490800712345> has been added to the posting order";
            string deleteRecord = "DELETE From postorder where ThreadID='1' AND Player='<@!271158490800712345>' AND PostPosition='99'";

            string result = Nine.Commands.Commands.ExecCommand("9", message);


            //delete record
            SqlCommand.ExecuteQuery(deleteRecord, false);

            StringAssert.AreEqualIgnoringCase(expectedResult, result);
        }
        #endregion
    }
}