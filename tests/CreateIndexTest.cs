using System;
using System.Linq;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SetUp = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#else
using NUnit.Framework;
#endif

namespace SQLite.Tests
{
    [TestFixture]
    public class CreateIndexTest
    {
        private TestDb _db;

        public class TestObject
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }
            public String Text { get; set; }

            public override string ToString()
            {
                return string.Format("[TestObj: Id={0}, Text={1}]", Id, Text);
            }
        }

        public class TestDb : SQLiteConnection
        {
            public TestDb(String path)
                : base(path)
            {
                CreateTable<TestObject>();
            }
        }

        [SetUp]
        public void Setup()
        {
            _db = new TestDb(TestPath.GetTempFileName());
        }

        [TearDown]
        public void TearDown()
        {
            if (_db != null) _db.Close();
        }

        [Test]
        public void CreateUniqueIndexOnNonUniqueColumn()
        {
            int numToAdd = 2;
            var q = from i in Enumerable.Range(1, numToAdd)
                    select new TestObject()
                    {
                        Text = "SameText"
                    };
            var testObjects = q.ToArray();

            var numAdded = _db.InsertAll(testObjects);

            Assert.AreEqual(numAdded, numToAdd, "Num inserted must = num objects");

            ExceptionAssert.Throws<UniqueConstraintViolationException>(() => _db.CreateIndex<TestObject>(a => a.Text, unique: true));
        }

        [Test]
        public void AddNonUniqueDataToUniqueColumn()
        {
            int numToAdd = 2;
            var q = from i in Enumerable.Range(1, numToAdd)
                    select new TestObject()
                    {
                        Text = "SameText"
                    };
            var testObjects = q.ToArray();

            _db.CreateIndex<TestObject>(a => a.Text, unique: true);

            ExceptionAssert.Throws<UniqueConstraintViolationException>(() => _db.InsertAll(testObjects));

            Assert.AreEqual(0, _db.Table<TestObject>().Count());
        }
    }
}
