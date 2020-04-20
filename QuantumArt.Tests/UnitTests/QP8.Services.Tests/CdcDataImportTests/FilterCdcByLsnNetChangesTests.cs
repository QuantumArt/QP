using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using AutoMapper;
using FluentAssertions;
using QP8.Infrastructure.TestTools.AutoFixture.Specimens;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc.Enums;
using Quantumart.QP8.Constants.DbColumns;
using Xunit;

namespace QP8.Services.Tests.CdcDataImportTests
{
    public class FilterCdcByLsnNetChangesTests
    {
        private readonly IFixture _fixture;

        public FilterCdcByLsnNetChangesTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization(){ ConfigureMembers = true});
            _fixture.Customizations.Add(new NameValueSpecimenBuilder());
            Mapper.Reset();
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<CdcTableTypeModel, CdcTableTypeModel>();
                cfg.CreateMap<CdcEntityModel, CdcEntityModel>();
            });
            Mapper.AssertConfigurationIsValid();

            QPContext.CurrentDbConnectionString = _fixture.Create<string>();
        }

        [Theory, AutoData, Trait("CdcTarantool", "DataImportJob")]
        public void GivenSimpleDataList_WhenContainsSimpleKey_ShouldReturnCorrectAndFilteredData(string transactionLsn)
        {
            // Fixture setup
            var cdcTable1 = CreateCdcTable(transactionLsn, "B");
            var cdcTable2 = CreateCdcTable(transactionLsn, "C");
            var cdcTable3 = CreateCdcTable(transactionLsn, "A");

            var listOfModels = new List<CdcTableTypeModel>
            {
                cdcTable1,
                cdcTable2,
                cdcTable3
            }.OrderBy(item =>item.SequenceLsn);

            var expected = cdcTable2;

            // Exercise system
            var actual = listOfModels.GetCdcDataFilteredByLsnNetChanges(cdc => new { cdc.TransactionLsn });

            // Verify outcome
            actual.Should().ContainSingle().Which.Should().BeEquivalentTo(expected);
        }

        [Theory, AutoData, Trait("CdcTarantool", "DataImportJob")]
        public void GivenSimpleDataList_WhenContainsSimpleKeyWithUpsert_ShouldReturnCorrectAndFilteredData(string transactionLsn)
        {
            // Fixture setup
            var cdcTable1 = CreateCdcTable(transactionLsn, "B", CdcOperationType.Insert);
            var cdcTable2 = CreateCdcTable(transactionLsn, "C", CdcOperationType.Update);
            var cdcTable3 = CreateCdcTable(transactionLsn, "A", CdcOperationType.Update);

            var listOfModels = new List<CdcTableTypeModel>
            {
                cdcTable1,
                cdcTable2,
                cdcTable3
            }.OrderBy(item =>item.SequenceLsn);

            var expected = cdcTable2;

            // Exercise system
            var actual = listOfModels.GetCdcDataFilteredByLsnNetChanges(cdc => new { cdc.TransactionLsn });

            // Verify outcome
            actual.Should().ContainSingle(cdc => cdc.Action == CdcOperationType.Insert).Which.Should().BeEquivalentTo(expected, options => options.Excluding(cdc => cdc.Action));
        }

        [Theory, AutoData, Trait("CdcTarantool", "DataImportJob")]
        public void GivenComplexDataList_WhenContainsSimpleKey_ShouldReturnCorrectAndFilteredData(string transactionLsn, string anotherTransactionLsn)
        {
            // Fixture setup
            var cdcTable1 = CreateCdcTable(transactionLsn, "A");
            var cdcTable2 = CreateCdcTable(transactionLsn, "D");
            var cdcTable3 = CreateCdcTable(transactionLsn, "C");
            var cdcTable4 = CreateCdcTable(anotherTransactionLsn, "B");

            var listOfModels = new List<CdcTableTypeModel>
            {
                cdcTable1,
                cdcTable2,
                cdcTable3,
                cdcTable4
            }.OrderBy(item =>item.SequenceLsn);

            var expected = new List<CdcTableTypeModel>
            {
                cdcTable2,
                cdcTable4
            };

            // Exercise system
            var actual = listOfModels.GetCdcDataFilteredByLsnNetChanges(cdc => new { cdc.TransactionLsn });

            // Verify outcome
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory, AutoData, Trait("CdcTarantool", "DataImportJob")]
        public void GivenComplexDataList_WhenContainsSimpleKeyWithUpsert_ShouldReturnCorrectAndFilteredData(string transactionLsn, string anotherTransactionLsn)
        {
            // Fixture setup
            var cdcTable1 = CreateCdcTable(transactionLsn, "A", CdcOperationType.Insert);
            var cdcTable2 = CreateCdcTable(transactionLsn, "D", CdcOperationType.Update);
            var cdcTable3 = CreateCdcTable(transactionLsn, "C", CdcOperationType.Update);
            var cdcTable4 = CreateCdcTable(anotherTransactionLsn, "B");

            var listOfModels = new List<CdcTableTypeModel>
            {
                cdcTable1,
                cdcTable2,
                cdcTable3,
                cdcTable4
            }.OrderBy(item =>item.SequenceLsn);

            var expected = new List<CdcTableTypeModel>
            {
                cdcTable2,
                cdcTable4
            };

            // Exercise system
            var actual = listOfModels.GetCdcDataFilteredByLsnNetChanges(cdc => new { cdc.TransactionLsn });

            // Verify outcome
            actual.Should().HaveCount(2);
            actual.Should().BeEquivalentTo(expected, options => options.Excluding(cdc => cdc.Action));
            Assert.True(actual.Single(cdc => cdc.SequenceLsn == "D").Action == CdcOperationType.Insert);
        }

        [Theory, AutoData, Trait("CdcTarantool", "DataImportJob")]
        public void GivenSimpleDataList_WhenContainsComplexKey_ShouldReturnCorrectAndFilteredData(decimal contentItemId, string transactionLsn)
        {
            // Fixture setup
            var cdcTable1 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "B");
            var cdcTable2 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "C");
            var cdcTable3 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "A");

            var listOfModels = new List<CdcTableTypeModel>
            {
                cdcTable1,
                cdcTable2,
                cdcTable3
            }.OrderBy(item =>item.SequenceLsn);

            var expected = cdcTable2;

            // Exercise system
            var actual = listOfModels.GetCdcDataFilteredByLsnNetChanges(cdc => new { contentItemId = cdc.Entity.Columns[ContentItemColumnName.ContentItemId], cdc.TransactionLsn });

            // Verify outcome
            actual.Should().ContainSingle().Which.Should().BeEquivalentTo(expected);
        }

        [Theory, AutoData, Trait("CdcTarantool", "DataImportJob")]
        public void GivenSimpleDataList_WhenContainsComplexKeyWithUpsert_ShouldReturnCorrectAndFilteredData(decimal contentItemId, string transactionLsn)
        {
            // Fixture setup
            var cdcTable1 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "B", CdcOperationType.Insert);
            var cdcTable2 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "C", CdcOperationType.Update);
            var cdcTable3 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "A", CdcOperationType.Update);

            var listOfModels = new List<CdcTableTypeModel>
            {
                cdcTable1,
                cdcTable2,
                cdcTable3
            }.OrderBy(item =>item.SequenceLsn);

            var expected = cdcTable2;

            // Exercise system
            var actual = listOfModels.GetCdcDataFilteredByLsnNetChanges(cdc => new { contentItemId = cdc.Entity.Columns[ContentItemColumnName.ContentItemId], cdc.TransactionLsn });

            // Verify outcome
            actual.Should().ContainSingle(cdc => cdc.Action == CdcOperationType.Insert).Which.Should().BeEquivalentTo(expected, options => options.Excluding(cdc => cdc.Action));
        }

        [Theory, AutoData, Trait("CdcTarantool", "DataImportJob")]
        public void GivenComplexDataList_WhenContainsComplexKey_ShouldReturnCorrectAndFilteredData(decimal contentItemId, string transactionLsn, string anotherTransactionLsn)
        {
            // Fixture setup
            var cdcTable1 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "A");
            var cdcTable2 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "D");
            var cdcTable3 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "C");
            var cdcTable4 = CreateCdcTableWithColumns(contentItemId, anotherTransactionLsn, "B");

            var listOfModels = new List<CdcTableTypeModel>
            {
                cdcTable1,
                cdcTable2,
                cdcTable3,
                cdcTable4
            }.OrderBy(item =>item.SequenceLsn);

            var expected = new List<CdcTableTypeModel>
            {
                cdcTable2,
                cdcTable4
            };

            // Exercise system
            var actual = listOfModels.GetCdcDataFilteredByLsnNetChanges(cdc => new { contentItemId = cdc.Entity.Columns[ContentItemColumnName.ContentItemId], cdc.TransactionLsn });

            // Verify outcome
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory, AutoData, Trait("CdcTarantool", "DataImportJob")]
        public void GivenComplexDataList_WhenContainsComplexKeyWithUpsert_ShouldReturnCorrectAndFilteredData(decimal contentItemId, string transactionLsn)
        {
            // Fixture setup
            var cdcTable1 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "A", CdcOperationType.Insert);
            var cdcTable2 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "D", CdcOperationType.Update);
            var cdcTable3 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "C", CdcOperationType.Update);
            var cdcTable4 = CreateCdcTable(transactionLsn, "B");

            var listOfModels = new List<CdcTableTypeModel>
            {
                cdcTable1,
                cdcTable2,
                cdcTable3,
                cdcTable4
            }.OrderBy(item =>item.SequenceLsn);

            var expected = new List<CdcTableTypeModel>
            {
                cdcTable2,
                cdcTable4
            };

            // Exercise system
            var actual = listOfModels.GetCdcDataFilteredByLsnNetChanges(cdc => new { contentItemId = cdc.Entity.Columns[ContentItemColumnName.ContentItemId], cdc.TransactionLsn });

            // Verify outcome
            actual.Should().HaveCount(2);
            actual.Should().BeEquivalentTo(expected, options => options.Excluding(cdc => cdc.Action));
            Assert.True(actual.Single(cdc => cdc.SequenceLsn == "D").Action == CdcOperationType.Insert);
        }

        [Theory, AutoData, Trait("CdcTarantool", "DataImportJob")]
        public void GivenContentDataList_WhenContainsInsertUpdate_ShouldReturnCorrectAndFilteredData(decimal contentItemId, string transactionLsn)
        {
            // Fixture setup
            var cdcTable1 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "B", CdcOperationType.Insert);
            var cdcTable2 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "C", CdcOperationType.Update);
            var cdcTable3 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "A");

            var listOfModels = new List<CdcTableTypeModel>
            {
                cdcTable1,
                cdcTable2,
                cdcTable3
            }.OrderBy(item =>item.SequenceLsn);

            var expectedColumnsFromFirstTable = cdcTable1.Entity.Columns.ToList();
            var expectedColumnsFromSecondTable = cdcTable2.Entity.Columns.ToList();
            var expected = cdcTable2;

            // Exercise system
            var actual = listOfModels.GetCdcDataFilteredByLsnNetChangesWithColumnsCopy(cdc => new { contentItemId = cdc.Entity.Columns[ContentItemColumnName.ContentItemId], cdc.TransactionLsn });

            // Verify outcome
            actual.Should().ContainSingle(cdc => cdc.Action == CdcOperationType.Insert).Which.Should().BeEquivalentTo(expected, options =>
            {
                options.Excluding(cdc => cdc.Action);
                options.Excluding(cdc => cdc.Entity.Columns);
                return options;
            });

            expectedColumnsFromFirstTable.Should().BeSubsetOf(actual.Single().Entity.Columns);
            expectedColumnsFromSecondTable.Should().BeSubsetOf(actual.Single().Entity.Columns);
        }

        [Theory, AutoData, Trait("CdcTarantool", "DataImportJob")]
        public void GivenContentDataList_WhenContainsInsertUpdateUpdate_ShouldReturnCorrectAndFilteredData(decimal contentItemId, string transactionLsn)
        {
            // Fixture setup
            var cdcTable1 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "B", CdcOperationType.Update);
            var cdcTable2 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "C", CdcOperationType.Update);
            var cdcTable3 = CreateCdcTableWithColumns(contentItemId, transactionLsn, "A", CdcOperationType.Insert);

            var overrideName = _fixture.Create<string>();
            var oderrideValue = (object)_fixture.Create<decimal>();
            cdcTable1.Entity.Columns.Add(overrideName, oderrideValue);
            cdcTable3.Entity.Columns.Add(overrideName, _fixture.Create<decimal>());

            var listOfModels = new List<CdcTableTypeModel>
            {
                cdcTable1,
                cdcTable2,
                cdcTable3
            }.OrderBy(item => item.SequenceLsn);

            var expectedColumnsFromFirstTable = cdcTable1.Entity.Columns.ToList();
            var expectedColumnsFromSecondTable = cdcTable2.Entity.Columns.ToList();
            var expectedColumnsFromThirdTable = cdcTable3.Entity.Columns.Where(c => c.Key != overrideName).ToList();
            var expectedColumn = new KeyValuePair<string, object>(overrideName, oderrideValue);
            var expected = cdcTable2;

            // Exercise system
            var actual = listOfModels.GetCdcDataFilteredByLsnNetChangesWithColumnsCopy(cdc => new { contentItemId = cdc.Entity.Columns[ContentItemColumnName.ContentItemId], cdc.TransactionLsn });

            // Verify outcome
            actual.Should()
                .ContainSingle(cdc => cdc.Action == CdcOperationType.Insert && cdc.Entity.Columns.Contains(expectedColumn))
                .Which.Should().BeEquivalentTo(expected, options =>
                {
                    options.Excluding(cdc => cdc.Action);
                    options.Excluding(cdc => cdc.Entity.Columns);
                    return options;
                });

            expectedColumnsFromFirstTable.Should().BeSubsetOf(actual.Single().Entity.Columns);
            expectedColumnsFromSecondTable.Should().BeSubsetOf(actual.Single().Entity.Columns);
            expectedColumnsFromThirdTable.Should().BeSubsetOf(actual.Single().Entity.Columns);
        }

        [Theory, AutoData, Trait("CdcTarantool", "DataImportJob")]
        public void GivenLinkIdList_WhenContainsInsertUpdate_ShouldReturnCorrectAndFilteredData(decimal linkId, decimal leftId, decimal rightId, string transactionLsn)
        {
            // Fixture setup
            var cdcTable1 = CreateLinkTableWithColumns(linkId, leftId, rightId, transactionLsn, "0x00011CA000030DEF00E6");
            var cdcTable2 = CreateLinkTableWithColumns(linkId, rightId, leftId, transactionLsn, "0x00011CA000030DEF00EB", CdcOperationType.Insert);
            var cdcTable3 = CreateLinkTableWithColumns(linkId, rightId, leftId, transactionLsn, "0x00011CA000030F480047", CdcOperationType.Update);

            var listOfModels = new List<CdcTableTypeModel>
            {
                cdcTable1,
                cdcTable2,
                cdcTable3
            }.OrderBy(item => item.SequenceLsn);

            var expected = new List<CdcTableTypeModel>
            {
                cdcTable1,
                cdcTable3
            };

            // Exercise system
            var actual = listOfModels.GetCdcDataFilteredByLsnNetChanges(cdc => new
            {
                linkId = cdc.Entity.Columns[ItemToItemColumnName.LinkId],
                leftId = cdc.Entity.Columns[ItemToItemColumnName.LItemId],
                rightId = cdc.Entity.Columns[ItemToItemColumnName.RItemId],
                cdc.TransactionLsn
            });

            // Verify outcome
            actual.Should().HaveCount(2);
            actual.Should().BeEquivalentTo(expected, options => options.Excluding(cdc => cdc.Action));
            Assert.True(actual.Single(cdc => cdc.SequenceLsn == "0x00011CA000030F480047").Action == CdcOperationType.Insert);
        }

        private CdcTableTypeModel CreateCdcTable(string transactionLsn, string sequenceLsn, CdcOperationType operationType = CdcOperationType.PreUpdate)
            => CreateCdcTableWithColumns(_fixture.Create<decimal>(), transactionLsn, sequenceLsn, operationType);

        private CdcTableTypeModel CreateCdcTableWithColumns(decimal contentItemId, string transactionLsn, string sequenceLsn, CdcOperationType operationType = CdcOperationType.PreUpdate)
        {
            var entity = _fixture
                .Build<CdcEntityModel>()
                .With(data => data.Columns, _fixture.CreateMany<KeyValuePair<string, decimal>>().ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value))
                .Create();

            entity.Columns.Add(ContentItemColumnName.ContentItemId, contentItemId);
            return _fixture
                .Build<CdcTableTypeModel>()
                .With(cdc => cdc.Entity, entity)
                .With(cdc => cdc.Action, operationType)
                .With(cdc => cdc.TransactionLsn, transactionLsn)
                .With(cdc => cdc.SequenceLsn, sequenceLsn)
                .Create();
        }

        private CdcTableTypeModel CreateLinkTableWithColumns(decimal linkId, decimal leftId, decimal rightId, string transactionLsn, string sequenceLsn, CdcOperationType operationType = CdcOperationType.PreUpdate)
        {
            var entity = _fixture
                .Build<CdcEntityModel>()
                .With(data => data.Columns, _fixture.CreateMany<KeyValuePair<string, decimal>>().ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value))
                .Create();

            entity.Columns.Add(ItemToItemColumnName.LinkId, linkId);
            entity.Columns.Add(ItemToItemColumnName.LItemId, leftId);
            entity.Columns.Add(ItemToItemColumnName.RItemId, rightId);

            return _fixture
                .Build<CdcTableTypeModel>()
                .With(cdc => cdc.Entity, entity)
                .With(cdc => cdc.Action, operationType)
                .With(cdc => cdc.TransactionLsn, transactionLsn)
                .With(cdc => cdc.SequenceLsn, sequenceLsn)
                .Create();
        }
    }
}
