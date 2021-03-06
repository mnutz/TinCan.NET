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
    using NUnit.Framework;
    using Newtonsoft.Json.Linq;
    using TinCan;
    using TinCan.Json;

    [TestFixture]
    class StatementTest
    {
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

            agent = new Agent();
            agent.mbox = "mailto:tincancsharp@tincanapi.com";

            verb = new Verb("http://adlnet.gov/expapi/verbs/experienced");

            activity = new Activity();
            activity.id = new Uri("http://tincanapi.com/TinCanCSharp/Test/Unit/0");
            parent = new Activity();
            parent.id = new Uri("http://tincanapi.com/TinCanCSharp/Test");

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
        public void TestEmptyCtr()
        {
            Statement obj = new Statement();
            Assert.IsInstanceOf<Statement>(obj);
            Assert.IsNull(obj.id);
            Assert.IsNull(obj.actor);
            Assert.IsNull(obj.verb);
            Assert.IsNull(obj.target);
            Assert.IsNull(obj.result);
            Assert.IsNull(obj.context);
            Assert.IsNull(obj.version);
            Assert.IsNull(obj.timestamp);
            Assert.IsNull(obj.stored);

            StringAssert.AreEqualIgnoringCase("{\"version\":\"1.0.1\"}", obj.ToJSON());
        }

        [Test]
        public void TestJObjectCtrSubStatement()
        {
            JObject cfg = new JObject();
            cfg.Add("actor", agent.ToJObject());
            cfg.Add("verb", verb.ToJObject());
            cfg.Add("object", subStatement.ToJObject());

            Statement obj = new Statement(cfg);
            Assert.IsInstanceOf<Statement>(obj);
            Assert.IsNotNull(obj.target);
        }
    }
}
