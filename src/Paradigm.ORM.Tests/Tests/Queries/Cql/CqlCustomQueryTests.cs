﻿using System;
using FluentAssertions;
using NUnit.Framework;
using Paradigm.ORM.Data.DatabaseAccess;
using Paradigm.ORM.Data.Extensions;
using Paradigm.ORM.Data.Querying;
using Paradigm.ORM.Tests.Fixtures;
using Paradigm.ORM.Tests.Fixtures.Cql;
using Paradigm.ORM.Tests.Mocks.Cql;

namespace Paradigm.ORM.Tests.Tests.Queries.Cql
{
    [TestFixture]
    public class CqlCustomQueryTests
    {
        private QueryFixtureBase Fixture { get; }

        public CqlCustomQueryTests()
        {
            this.Fixture = Activator.CreateInstance(typeof(CqlQueryFixture)) as QueryFixtureBase;
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            this.Fixture.CreateDatabase();
            this.Fixture.CreateParentTable();
            this.Fixture.CreateChildTable();

            using (var databaseAccess = new DatabaseAccess(this.Fixture.Connector, typeof(SingleKeyParentTable)))
            {
                databaseAccess.Insert(this.Fixture.CreateNewEntity());
                databaseAccess.Insert(this.Fixture.CreateNewEntity2());
            }
        }

        [Test]
        public void ShouldRetrieveTwoEntities()
        {
            var result = this.Fixture.Connector.CustomQuery<SingleKeyParentTable>(this.Fixture.SelectClause);
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var entity1 = result.Find(x => x.Name.StartsWith("Test Parent 1"));
            entity1.IsActive.Should().Be(true);
            entity1.Childs.Should().BeNull();
            entity1.CreatedDate.Should().Be(new DateTime(2017, 4, 12));
            entity1.Amount.Should().Be(30.34m);

            var entity2 = result.Find(x => x.Name.StartsWith("Test Parent 2"));
            entity2.IsActive.Should().Be(false);
            entity2.Childs.Should().BeNull();
            entity2.CreatedDate.Should().Be(new DateTime(2017, 6, 21));
            entity2.Amount.Should().Be(215.5m);
        }

        [Test]
        public void QueryWithWhere()
        {
            var result = this.Fixture.Connector.CustomQuery<SingleKeyParentTable>($"{this.Fixture.SelectClause} WHERE {this.Fixture.WhereClause}");
            result.Should().NotBeNull();
            result.Should().HaveCount(1);

            result[0].Name.Should().StartWith("Test Parent 1");
            result[0].IsActive.Should().Be(true);
            result[0].Childs.Should().BeNull();
            result[0].CreatedDate.Should().Be(new DateTime(2017, 4, 12));
            result[0].Amount.Should().Be(30.34m);
        }

        [Test]
        public void QueryWithNotMatchingWhere()
        {
            var result = this.Fixture.Connector.CustomQuery<SingleKeyParentTable>($"{this.Fixture.SelectClause} WHERE \"Id\"=10");
            result.Should().NotBeNull();
            result.Should().HaveCount(0);
        }

        [Test]
        public void QueryObjectMustBeReutilizable()
        {
            var query = new CustomQuery<SingleKeyParentTable>(this.Fixture.Connector, this.Fixture.SelectClause);

            var result = query.Execute();
            var result2 = query.Execute();

            result.Should().NotBeNull();
            result2.Should().NotBeNull();
            result2.Should().HaveSameCount(result);

            query.Dispose();
        }

        [Test]
        public void DisposingTwoTimesShouldBeOk()
        {
            var query = new CustomQuery<SingleKeyParentTable>(this.Fixture.Connector, this.Fixture.SelectClause);

            query.Dispose();
            query.Dispose();
        }

        [Test]
        public void ShouldNotUseDisposedQueryObject()
        {
            var query = new CustomQuery<SingleKeyParentTable>(this.Fixture.Connector, this.Fixture.SelectClause);
            query.Dispose();

            Action executeQuery = () => query.Execute();
            executeQuery.ShouldThrow<NullReferenceException>();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            this.Fixture.DropDatabase();
            this.Fixture.Dispose();
        }
    }
}
