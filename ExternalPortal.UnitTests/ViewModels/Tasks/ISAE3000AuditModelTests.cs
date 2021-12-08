using ExternalPortal.Enums;
using ExternalPortal.ViewModels.Tasks;
using Xunit;

namespace ExternalPortal.UnitTests.ViewModels.Tasks
{
    public class ISAE3000AuditModelTests
    {
        [Fact]
        public void ShouldReturnEmptyModelFromConstructor()
        {
            var actual = new Isae3000AuditModel();

            Assert.Equal(TaskStatus.NotStarted, actual.State);
            Assert.Null(actual.Filename);
            Assert.Null(actual.DocumentId);
        }

        [Fact]
        public void ShouldReturnSetModelStatusInProgressWhenStarted()
        {
            var actual = new Isae3000AuditModel();

            actual.Start();

            Assert.Equal(TaskStatus.InProgress, actual.State);
        }

        [Fact]
        public void ShouldReturnSetModelStatusCompleteWhenFinished()
        {
            var actual = new Isae3000AuditModel();

            actual.Finish();

            Assert.Equal(TaskStatus.Completed, actual.State);
        }
    }
}
