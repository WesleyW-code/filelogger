using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;

// Sources/Articles that helped me understand how to do all of this!:
// https://docs.microsoft.com/en-us/visualstudio/test/walkthrough-creating-and-running-unit-tests-for-managed-code
// https://docs.microsoft.com/en-us/visualstudio/test/getting-started-with-unit-testing?view=vs-2022&tabs=dotnet%2Cmstest
// https://riptutorial.com/moq
// https://daedtech.com/introduction-to-unit-testing-part-6-test-doubles/
// https://www.pluralsight.com/guides/testing-.net-core-apps-with-visual-studio-code
// https://docs.microsoft.com/en-us/visualstudio/get-started/tutorial-projects-solutions?view=vs-2022
// The below link helped me understand how to check if a file exists, reading the text, and elsewhere it’s saving text to a file.
// https://makolyte.com/csharp-unit-testing-code-that-does-file-io/
// Tutorial on using Moq.
// https://spin.atomicobject.com/2017/08/07/intro-mocking-moq/
// https://hamidmosalla.com/2017/08/03/moq-working-with-setupget-verifyget-setupset-verifyset-setupproperty/#:~:text=Moq%20VerifyGet&text=VerifyGet%20helps%20us%20verify%20that,the%20getter%20of%20FirstName%20property.

namespace FileLoggerKata.Tests
{
    // This class will have all my unit tests in.
    [TestClass]
    public class FileLoggerTests
    {
        // Creating information that i will pass through my test doubles:

        // Creating all the days of the week needed to test certian scenarios. 

        private static readonly DateTime Saturday = new DateTime(2022, 3, 5);// Saturday

        private static readonly DateTime Sunday = new DateTime(2022, 3, 6);// Sunday
        private DateTime Today_Default => new DateTime(2022, 3, 11); // Friday (A day during the week)

        // Creating all the neccesarry things needed to test certian file instances.

        private const string TestMessage = "WesleyUnitTests"; // Standard Message needed to add to the text files.

        private string LogFileName_Default => $"log{Today_Default:yyyyMMdd}.txt"; // Creates a default name for a file.

        private string LogFileName_Weekend => "weekend.txt"; // Creates weekend filename.

        // Need these to create date objects and manipulate files.

        private Mock<IDateProvider> DateProvider_Mock { get; } // Creates a mock IDateProvider object

        private Mock<IFileSystem> FileSystem_Mock { get; } // Creates a mock IFileSystem object

        // Need this to access Filelogger and test its methods.

        private FileLogger file_logger { get; } // This allows me to get access to the FileLogger class so that i can test the methods within.

        // This method will be used to set up my DateProvider_Mock and FileSystem_Mock so i can use them throughout the tests.
        public FileLoggerTests()
        {
            // This is allowing my Mock DateProvider to have the same behavioral traits as IDateProvider (Allowing it to do the same things)
            DateProvider_Mock = new Mock<IDateProvider>(MockBehavior.Strict);

            // Setting up DateProvider_Mock so that is returns my created Today_Default.  
            DateProvider_Mock.Setup(d => d.Today).Returns(Today_Default);

            // This is allowing my Mock FileSystem to have the same behavioral traits as IFileSystem (Allowing it to do the same things)
            FileSystem_Mock = new Mock<IFileSystem>(MockBehavior.Strict);

            // Setting up the structure so that it doesnt give errors during run time / allows me to populate the characteristics later on.
            FileSystem_Mock.Setup(f => f.Exists(It.IsNotNull<string>())).Returns(true);
            FileSystem_Mock.Setup(f => f.Create(It.IsNotNull<string>()));
            FileSystem_Mock.Setup(f => f.Append(It.IsNotNull<string>(), It.IsNotNull<string>()));
            FileSystem_Mock.Setup(f => f.GetLastWriteTime(It.IsNotNull<string>())).Returns(DateTime.Now);
            FileSystem_Mock.Setup(f => f.Rename(It.IsNotNull<string>(), It.IsNotNull<string>()));

            // Setting up the FileLogger variable. It needs a IFilesystem and IDateProvider input which i have populated with my mock objects above
            file_logger = new FileLogger(FileSystem_Mock.Object, DateProvider_Mock.Object);
        }

        // This test method will check if a file is created if one doesnt exist and then appends to it onces its made.
        [TestMethod]
        public void IfLogNotExists_CreatesLog_and_AppendsMsg()
        {
            // Setting up FileSystem_Mock so it returns that there is no file.
            FileSystem_Mock.Setup(f => f.Exists(LogFileName_Default)).Returns(false);

            // Passing my default test message through the log() method.
            file_logger.Log(TestMessage);

            // Then checking/Verifying if the expected file now exists.
            // Times.once refers to how many times it verifys
            FileSystem_Mock.Verify(f => f.Exists(LogFileName_Default), Times.Once);
            
            // Verifying that the expected file was created.
            FileSystem_Mock.Verify(f => f.Create(LogFileName_Default), Times.Once);

            // Verifying that the message was appended to the file correctly.
            FileSystem_Mock.Verify(f => f.Append(LogFileName_Default, TestMessage), Times.Once);
        }
        [TestMethod]
        public void IfLogExists_NotCreatesLog_and_AppendsMsg()
        {
            // Setting up FileSystem_Mock so it returns that there is a file.
            FileSystem_Mock.Setup(f => f.Exists(LogFileName_Default)).Returns(true);

            // Passing my default test message through the log() method.
            file_logger.Log(TestMessage);

            // Then checking/Verifying if the expected file now exists.
            // Times.once refers to how many times it verifys
            FileSystem_Mock.Verify(f => f.Exists(LogFileName_Default), Times.Once);

            // Verifying that the expected file IS NOT created. So that i can test if the method just appends correctly
            FileSystem_Mock.Verify(f => f.Create(LogFileName_Default), Times.Never, "A Log.txt file was created incorectly!");

            // Verifying that the message was appended to the file correctly.
            FileSystem_Mock.Verify(f => f.Append(LogFileName_Default, TestMessage), Times.Once,"The message was not appended to the Log.txt File");
        }
        [TestMethod]
        public void IfWNKDLogExistAndSunday_NotCreatesLog_and_AppendsMsg()
        {
            // Setting up FileSystem_Mock so it returns that there is a file.
            FileSystem_Mock.Setup(f => f.Exists(LogFileName_Weekend)).Returns(true);

            // Setting the date to be a Sunday
            DateProvider_Mock.Setup(d => d.Today).Returns(Sunday);

            // Passing my default test message through the log() method.
            file_logger.Log(TestMessage);

            // Then checking/Verifying if the expected file now exists.
            // When the date is on a weekend the Exists property is called multiple times.
            FileSystem_Mock.Verify(f => f.Exists(LogFileName_Weekend), Times.AtLeastOnce);

            // Verifying that the expected file IS NOT created. So that i can test if the method just appends correctly
            FileSystem_Mock.Verify(f => f.Create(LogFileName_Weekend), Times.Never,"A Weekend.txt file was created incorectly!");

            // Verifying that the message was appended to the file correctly.
            FileSystem_Mock.Verify(f => f.Append(LogFileName_Weekend, TestMessage), Times.Once, "The message was not appended to the Weekend.txt File");
        }
        [TestMethod]
        public void IfSaturday_CreatesWeekendLog_and_AppendsMsg()
        {
            // Setting up FileSystem_Mock so it returns that there is no Weekend file.
            FileSystem_Mock.Setup(f => f.Exists(LogFileName_Weekend)).Returns(false);

            // Setting the date to be a Saturday
            DateProvider_Mock.Setup(d => d.Today).Returns(Saturday);

            // Passing my default test message through the log() method.
            file_logger.Log(TestMessage);

            // Times.once refers that the action must only happen once.
            // Times.AtLeastOnce refers that the action must happen atleast once.
            // VerifyGet is only used when there is a { get; } on the property we are checking. This will only apply to IDateProvider.Today.
            DateProvider_Mock.VerifyGet(d => d.Today, Times.AtLeastOnce,"Todays date was not fetched!");

            // Then checking/Verifying if the expected file now exists.
            // On a weekend it checks if the file exists more than once.
            FileSystem_Mock.Verify(f => f.Exists(LogFileName_Weekend), Times.AtLeastOnce);

            // Verifying that the expected file was created.
            FileSystem_Mock.Verify(f => f.Create(LogFileName_Weekend), Times.Once,"New Weekend.txt File was not created!");

            // Verifying that the message was appended to the file correctly.
            FileSystem_Mock.Verify(f => f.Append(LogFileName_Weekend, TestMessage), Times.Once, "The message was not appended to the Weekend.txt File");
        }
        [TestMethod]
        public void IfSunday_CreatesWeekendLog_and_AppendsMsg()
        {
            // Setting up FileSystem_Mock so it returns that there is no Weekend file.
            FileSystem_Mock.Setup(f => f.Exists(LogFileName_Weekend)).Returns(false);

            // Setting the date to be a Sunday
            DateProvider_Mock.Setup(d => d.Today).Returns(Sunday);

            // Passing my default test message through the log() method.
            file_logger.Log(TestMessage);

            // Times.once refers that the action must only happen once.
            // Times.AtLeastOnce refers that the action must happen atleast once.
            // VerifyGet is only used when there is a { get; } on the property we are checking. This will only apply to IDateProvider.Today.
            DateProvider_Mock.VerifyGet(d => d.Today, Times.AtLeastOnce,"Todays date was not fetched!");

            // Then checking/Verifying if the expected file now exists.
            // On a weekend it checks if the file exists more than once.
            FileSystem_Mock.Verify(f => f.Exists(LogFileName_Weekend), Times.AtLeastOnce);

            // Verifying that the expected file was created.
            FileSystem_Mock.Verify(f => f.Create(LogFileName_Weekend), Times.Once,"New Weekend.txt File was not created!");

            // Verifying that the message was appended to the file correctly.
            FileSystem_Mock.Verify(f => f.Append(LogFileName_Weekend, TestMessage), Times.Once,"The message was not appended to the Weekend.txt File");
        }
        [TestMethod]
        public void IfSunday_And_WeekendLogExists_AppendsMsg()
        {
            // Setting up FileSystem_Mock so it returns that there is a Weekend file.
            FileSystem_Mock.Setup(f => f.Exists(LogFileName_Weekend)).Returns(true);

            // Setting the date to be a Sunday
            DateProvider_Mock.Setup(d => d.Today).Returns(Sunday);

            // Passing my default test message through the log() method.
            file_logger.Log(TestMessage);

            // Times.once refers that the action must only happen once.
            // Times.AtLeastOnce refers that the action must happen atleast once.
            // VerifyGet is only used when there is a { get; } on the property we are checking. This will only apply to IDateProvider.Today.
            DateProvider_Mock.VerifyGet(d => d.Today, Times.AtLeastOnce, "Todays date was not fetched!");

            // Then checking/Verifying if the expected file now exists.
            // On a weekend it checks if the file exists more than once.
            FileSystem_Mock.Verify(f => f.Exists(LogFileName_Weekend), Times.AtLeastOnce);

            // Verifying that the expected file was created.
            FileSystem_Mock.Verify(f => f.Create(LogFileName_Weekend), Times.Never, "New Weekend.txt File was created and shouldn't!");

            // Verifying that the message was appended to the file correctly.
            FileSystem_Mock.Verify(f => f.Append(LogFileName_Weekend, TestMessage), Times.Once, "The message was not appended to the Weekend.txt File");
        }

        [TestMethod]
        public void IfWKNDLogExists_ButFromPastWKND_EditedOnSaturday_RenameOldLog()
        {
            // Creating an old date.
            DateTime OldDate = new DateTime(2022, 2, 19);// Saturday-Two weekends back to test better.
            var OldDateFileName = $"weekend-{OldDate:yyyyMMdd}.txt";

            // Setting todays Date.
            DateProvider_Mock.Setup(d => d.Today).Returns(Saturday);

            // Setting up FileSystem_Mock so it returns that there is a Weekend file.
            FileSystem_Mock.Setup(f => f.Exists(LogFileName_Weekend)).Returns(true);

            // Telling the system that the file was written on the OldDate
            FileSystem_Mock.Setup(f => f.GetLastWriteTime(LogFileName_Weekend)).Returns(OldDate);

            // Passing my default test message through the log() method.
            file_logger.Log(TestMessage);

            // Then checking/Verifying that the old Weekend.txt file is remaned as exepected.
            FileSystem_Mock.Verify(f => f.Rename(LogFileName_Weekend, OldDateFileName), Times.Once,"The File was not renamed as expected!");

            // This verifying that the new Weekend.txt file is appended to.
            FileSystem_Mock.Verify(f => f.Append(LogFileName_Weekend,TestMessage), Times.Once,"The message was not appended to the new Weekend.txt file!");

        }
        [TestMethod]
        public void IfWKNDLogExists_ButFromPastWKND_EditedOnSunday_RenameOldLog_WithSaturdayDate()
        {
            // Creating an old date.
            DateTime OldSunday = new DateTime(2022, 2, 20);// Sunday-Two weekends back to test better.
            DateTime OldSaturday = new DateTime(2022, 2, 19);// Saturday-For the file name.
            var CorrectArcFileName = $"weekend-{OldSaturday:yyyyMMdd}.txt"; 
            
            // Setting todays Date.
            DateProvider_Mock.Setup(d => d.Today).Returns(Saturday);

            // Setting up FileSystem_Mock so it returns that there is a Weekend file.
            FileSystem_Mock.Setup(f => f.Exists(LogFileName_Weekend)).Returns(true);

            // Telling the system that the file was written on the OldDate
            FileSystem_Mock.Setup(f => f.GetLastWriteTime(LogFileName_Weekend)).Returns(OldSunday);

            // Passing my default test message through the log() method.
            file_logger.Log(TestMessage);

            // Then checking/Verifying that the old Weekend.txt file is remaned as expected.
            FileSystem_Mock.Verify(f => f.Rename(LogFileName_Weekend, CorrectArcFileName), Times.Once, "The File was renamed with the Old Sunday Date instead of the Old Saturday Date!");

            // This verifying that the new Weekend.txt file is appended to.
            FileSystem_Mock.Verify(f => f.Append(LogFileName_Weekend, TestMessage), Times.Once, "The message was not appended to the new Weekend.txt file!");

        }

    }
}