using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class LGActivityGeneratorTests
    {
        private string GetExampleFilePath(string fileName)
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "Examples\\" + fileName;
        }
        

        [TestMethod]
        public void TestBasicActivity()
        {   
            var engine = TemplateEngine.FromFile(GetExampleFilePath("BasicActivity.lg")); 
            var activityGenerator = new LGActivityGenerator(engine);
            var activity = activityGenerator.GenerateActivity("RecentTasks", new { recentTasks = new[] { "Task1" } });
            Assert.AreEqual("Your most recent task is Task1. You can let me know if you want to add or complete a task.", activity.Text);
            Assert.AreEqual("Your most recent task is Task1. You can let me know.", activity.Speak);

            // Test whitespace.
            // Only the last whitespace before separtor will be removed.
            // That means you need to type two whitespace if you want to keep one whitespace at the end of your Text string.
            // All whitespace after separtor(before speak string) will be removed.
            activity = activityGenerator.GenerateActivity("RecentTasks", new { recentTasks = new[] { "Task1", "Task2" } });
            Assert.AreEqual("Your most recent tasks are Task1 and Task2. You can let me know if you want to add or complete a task.  ", activity.Text);
            Assert.AreEqual("Your most recent tasks are Task1 and Task2. You can let me know. ", activity.Speak);

            // Use "&&" as separtor
            activity = activityGenerator.GenerateActivity("RecentTasks", new { recentTasks = new[] { "Task1", "Task2", "Task3" } }, "&&");
            Assert.AreEqual("Your most recent tasks are Task1, Task2 and Task3. You can let me know if you want to add or complete a task.", activity.Text);
            Assert.AreEqual("Your most recent tasks are Task1, Task2 and Task3. You can let me know.", activity.Speak);
        }

        [TestMethod]
        public void TestAdaptiveCardActivity()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("AdaptiveCardActivity.lg"));
            var activityGenerator = new LGActivityGenerator(engine);
            var activity = activityGenerator.GenerateAdaptiveCardActivity("", null);
        }

        [TestMethod]
        public void TestNonAdaptiveCardActivity()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("NonAdaptiveCardActivity.lg"));
            var activityGenerator = new LGActivityGenerator(engine);
            var activity = activityGenerator.GenerateNonAdaptiveCardActivity("", null);
        }
    }
}
