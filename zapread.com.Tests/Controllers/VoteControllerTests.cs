using Microsoft.VisualStudio.TestTools.UnitTesting;
using zapread.com.Services;

namespace zapread.com.Tests.Controllers
{
    [TestClass]
    public class VoteControllerTests
    {
        [TestMethod]
        public void TestDownVoteAsUser()
        {
            // Test downvoting a high-rep user comment
            var userRep = 500000;

            var voterRep = 0;
            var amount = -10;

            var scoreAdj = ReputationService.GetReputationAdjustedAmount(
                        amount: amount,
                        targetRep: userRep,
                        actorRep: voterRep);

            Assert.IsTrue(scoreAdj < 0);
        }
    }
}
