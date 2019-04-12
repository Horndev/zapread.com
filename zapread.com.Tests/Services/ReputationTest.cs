using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zapread.com.Services;

namespace zapread.com.Tests.Services
{
    [TestClass]
    public class ReputationTest
    {
        [TestMethod]
        public void TestRepA()
        {
            Int64 InitialScore = 1000;
            Int64 authorRep = 500000;
            Int64 userRep = 200;

            Int64 vote;

            // Voted up
            vote = +100000;
            var adj = ReputationService.GetReputationAdjustedAmount(vote, authorRep, userRep);
            var newScore = InitialScore + adj;

            Assert.IsTrue(newScore > InitialScore);

            // Voted down
            vote = -100000;
            adj = ReputationService.GetReputationAdjustedAmount(vote, authorRep, userRep);
            newScore = InitialScore + adj;
            Assert.IsTrue(newScore < InitialScore);
        }

        [TestMethod]
        public void TestRepB()
        {
            Int64 InitialScore = 1000;
            Int64 authorRep = 500000;
            Int64 userRep = 500000;

            Int64 vote;

            // Voted up
            vote = +100000;
            var adj = ReputationService.GetReputationAdjustedAmount(vote, authorRep, userRep);
            var newScore = InitialScore + adj;

            Assert.IsTrue(newScore > InitialScore);

            // Voted down
            vote = -100000;
            adj = ReputationService.GetReputationAdjustedAmount(vote, authorRep, userRep);
            newScore = InitialScore + adj;
            Assert.IsTrue(newScore < InitialScore);
        }
    }
}
