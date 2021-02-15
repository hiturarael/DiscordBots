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
    }
}