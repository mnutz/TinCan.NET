﻿/*
    Copyright 2014 Rustici Software

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/
namespace TinCanTests
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using NUnit.Framework;
    using Newtonsoft.Json.Linq;
    using TinCan;
    using TinCan.Documents;
    using TinCan.Json;
    using TinCan.LRSResponses;

    [TestFixture]
    class RemoteLRSResourceTest
    {
        RemoteLRS lrs;
        Agent agent;
        Verb verb;
        Activity activity;
        Activity parent;
        Context context;
        Result result;
        Score score;
        StatementRef statementRef;
        SubStatement subStatement;

        [SetUp]
        public void Init()
        {
            Console.WriteLine("Running " + TestContext.CurrentContext.Test.FullName);
            lrs = new RemoteLRS(
                "",
                "",
                ""
            );

            agent = new Agent();
            agent.mbox = "mailto:tincancsharp@tincanapi.com";

            verb = new Verb("http://adlnet.gov/expapi/verbs/experienced");
            verb.display = new LanguageMap();
            verb.display.Add("en-US", "experienced");

            activity = new Activity();
            activity.id = new Uri("http://tincanapi.com/TinCanCSharp/Test/Unit/0");
            activity.definition = new ActivityDefinition();
            activity.definition.type = new Uri("http://id.tincanapi.com/activitytype/unit-test");
            activity.definition.name = new LanguageMap();
            activity.definition.name.Add("en-US", "Tin Can C# Tests: Unit 0");
            activity.definition.description = new LanguageMap();
            activity.definition.description.Add("en-US", "Unit test 0 in the test suite for the Tin Can C# library.");

            parent = new Activity();
            parent.id = new Uri("http://tincanapi.com/TinCanCSharp/Test");
            parent.definition = new ActivityDefinition();
            parent.definition.type = new Uri("http://id.tincanapi.com/activitytype/unit-test-suite");
            //parent.definition.moreInfo = new Uri("http://rusticisoftware.github.io/TinCanCSharp/");
            parent.definition.name = new LanguageMap();
            parent.definition.name.Add("en-US", "Tin Can C# Tests");
            parent.definition.description = new LanguageMap();
            parent.definition.description.Add("en-US", "Unit test suite for the Tin Can C# library.");

            statementRef = new StatementRef(Guid.NewGuid());

            context = new Context();
            context.registration = Guid.NewGuid();
            context.statement = statementRef;
            context.contextActivities = new ContextActivities();
            context.contextActivities.parent = new List<Activity>();
            context.contextActivities.parent.Add(parent);

            score = new Score();
            score.raw = 97;
            score.scaled = 0.97;
            score.max = 100;
            score.min = 0;

            result = new Result();
            result.score = score;
            result.success = true;
            result.completion = true;
            result.duration = new TimeSpan(1, 2, 16, 43);

            subStatement = new SubStatement();
            subStatement.actor = agent;
            subStatement.verb = verb;
            subStatement.target = parent;
        }

        [Test]
        public void TestAbout()
        {
            AboutLRSResponse lrsRes = lrs.About();
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestAboutFailure()
        {
            lrs.endpoint = new Uri("http://cloud.scorm.com/tc/3TQLAI9/sandbox/");

            AboutLRSResponse lrsRes = lrs.About();
            Assert.IsFalse(lrsRes.success);
            Console.WriteLine("TestAboutFailure - errMsg: " + lrsRes.errMsg);
        }

        [Test]
        public void TestSaveStatement()
        {
            var statement = new Statement();
            statement.actor = agent;
            statement.verb = verb;
            statement.target = activity;

            StatementLRSResponse lrsRes = lrs.SaveStatement(statement);
            Assert.IsTrue(lrsRes.success);
            Assert.AreEqual(statement, lrsRes.content);
            Assert.IsNotNull(lrsRes.content.id);
        }

        [Test]
        public void TestSaveStatementWithID()
        {
            var statement = new Statement();
            statement.Stamp();
            statement.actor = agent;
            statement.verb = verb;
            statement.target = activity;

            StatementLRSResponse lrsRes = lrs.SaveStatement(statement);
            Assert.IsTrue(lrsRes.success);
            Assert.AreEqual(statement, lrsRes.content);
        }

        [Test]
        public void TestSaveStatementStatementRef()
        {
            var statement = new Statement();
            statement.Stamp();
            statement.actor = agent;
            statement.verb = verb;
            statement.target = statementRef;

            StatementLRSResponse lrsRes = lrs.SaveStatement(statement);
            Assert.IsTrue(lrsRes.success);
            Assert.AreEqual(statement, lrsRes.content);
        }

        [Test]
        public void TestSaveStatementSubStatement()
        {
            var statement = new Statement();
            statement.Stamp();
            statement.actor = agent;
            statement.verb = verb;
            statement.target = subStatement;

            Console.WriteLine(statement.ToJSON(true));

            StatementLRSResponse lrsRes = lrs.SaveStatement(statement);
            Assert.IsTrue(lrsRes.success);
            Assert.AreEqual(statement, lrsRes.content);
        }

        [Test]
        public void TestSaveStatements()
        {
            var statement1 = new Statement();
            statement1.actor = agent;
            statement1.verb = verb;
            statement1.target = parent;

            var statement2 = new Statement();
            statement2.actor = agent;
            statement2.verb = verb;
            statement2.target = activity;
            statement2.context = context;

            var statements = new List<Statement>();
            statements.Add(statement1);
            statements.Add(statement2);

            StatementsResultLRSResponse lrsRes = lrs.SaveStatements(statements);
            Assert.IsTrue(lrsRes.success);
            // TODO: check statements match and ids not null
        }

        [Test]
        public void TestRetrieveStatement()
        {
            var statement = new TinCan.Statement();
            statement.Stamp();
            statement.actor = agent;
            statement.verb = verb;
            statement.target = activity;
            statement.context = context;
            statement.result = result;

            StatementLRSResponse saveRes = lrs.SaveStatement(statement);
            if (saveRes.success)
            {
                StatementLRSResponse retRes = lrs.RetrieveStatement(saveRes.content.id.Value);
                Assert.IsTrue(retRes.success);
                Console.WriteLine("TestRetrieveStatement - statement: " + retRes.content.ToJSON(true));
            }
            else
            {
                // TODO: skipped?
            }
        }

        [Test]
        public void TestQueryStatements()
        {
            var query = new TinCan.StatementsQuery();
            query.agent = agent;
            query.verbId = verb.id;
            query.activityId = parent.id;
            query.relatedActivities = true;
            query.relatedAgents = true;
            query.format = StatementsQueryResultFormat.IDS;
            query.limit = 10;

            StatementsResultLRSResponse lrsRes = lrs.QueryStatements(query);
            Assert.IsTrue(lrsRes.success);
            Console.WriteLine("TestQueryStatements - statement count: " + lrsRes.content.statements.Count);
        }

        [Test]
        public void TestMoreStatements()
        {
            var query = new TinCan.StatementsQuery();
            query.format = StatementsQueryResultFormat.IDS;
            query.limit = 2;

            StatementsResultLRSResponse queryRes = lrs.QueryStatements(query);
            if (queryRes.success && queryRes.content.more != null)
            {
                StatementsResultLRSResponse moreRes = lrs.MoreStatements(queryRes.content);
                Assert.IsTrue(moreRes.success);
                Console.WriteLine("TestMoreStatements - statement count: " + moreRes.content.statements.Count);
            }
            else
            {
                // TODO: skipped?
            }
        }

        [Test]
        public void TestRetrieveStateIds()
        {
            ProfileKeysLRSResponse lrsRes = lrs.RetrieveStateIds(activity, agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveState()
        {
            StateLRSResponse lrsRes = lrs.RetrieveState("test", activity, agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestSaveState()
        {
            var doc = new StateDocument();
            doc.activity = activity;
            doc.agent = agent;
            doc.id = "test";
            doc.content = System.Text.Encoding.UTF8.GetBytes("Test value");

            LRSResponse lrsRes = lrs.SaveState(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestDeleteState()
        {
            var doc = new StateDocument();
            doc.activity = activity;
            doc.agent = agent;
            doc.id = "test";

            LRSResponse lrsRes = lrs.DeleteState(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestClearState()
        {
            LRSResponse lrsRes = lrs.ClearState(activity, agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveActivityProfileIds()
        {
            ProfileKeysLRSResponse lrsRes = lrs.RetrieveActivityProfileIds(activity);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveActivityProfile()
        {
            ActivityProfileLRSResponse lrsRes = lrs.RetrieveActivityProfile("test", activity);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestSaveActivityProfile()
        {
            var doc = new ActivityProfileDocument();
            doc.activity = activity;
            doc.id = "test";
            doc.content = System.Text.Encoding.UTF8.GetBytes("Test value");

            LRSResponse lrsRes = lrs.SaveActivityProfile(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestDeleteActivityProfile()
        {
            var doc = new ActivityProfileDocument();
            doc.activity = activity;
            doc.id = "test";

            LRSResponse lrsRes = lrs.DeleteActivityProfile(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveAgentProfileIds()
        {
            ProfileKeysLRSResponse lrsRes = lrs.RetrieveAgentProfileIds(agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestRetrieveAgentProfile()
        {
            AgentProfileLRSResponse lrsRes = lrs.RetrieveAgentProfile("test", agent);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestSaveAgentProfile()
        {
            var doc = new AgentProfileDocument();
            doc.agent = agent;
            doc.id = "test";
            doc.content = System.Text.Encoding.UTF8.GetBytes("Test value");

            LRSResponse lrsRes = lrs.SaveAgentProfile(doc);
            Assert.IsTrue(lrsRes.success);
        }

        [Test]
        public void TestDeleteAgentProfile()
        {
            var doc = new AgentProfileDocument();
            doc.agent = agent;
            doc.id = "test";

            LRSResponse lrsRes = lrs.DeleteAgentProfile(doc);
            Assert.IsTrue(lrsRes.success);
        }
    }
}