using System.Collections.Generic;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Definitions.Cardigann;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.CardigannTests
{
    [TestFixture]
    public class MergeheaderFixture : CoreTest<CardigannMergeheaderTestSubject>
    {
        private const string MergeSelector = "td.audio_header.edition_info";

        private const string SampleHtml = """
            <table>
              <tbody>
                <tr><td class="audio_header edition_info">Dual</td><td>Other column</td></tr>
                <tr class="torrent"><td>torrent 1</td></tr>
                <tr class="torrent"><td>torrent 2</td></tr>
                <tr><td class="audio_header edition_info">Legendado</td><td>Other column</td></tr>
                <tr class="torrent"><td>torrent 3</td></tr>
                <tr class="torrent"><td>torrent 4</td></tr>
              </tbody>
            </table>
            """;

        [SetUp]
        public void SetUp()
        {
            var definition = Builder<CardigannDefinition>.CreateNew()
                .With(x => x.Encoding = "UTF-8")
                .With(x => x.Links = new List<string> { "https://example.com/" })
                .With(x => x.Caps = new CapabilitiesBlock())
                .Build();

            Mocker.SetConstant(definition);
        }

        [Test]
        public void should_merge_closest_previous_header_for_each_torrent_row()
        {
            var torrentRows = GetTorrentRows(SampleHtml);

            Subject.ApplyMergeheader(torrentRows[0], MergeSelector);
            Subject.ApplyMergeheader(torrentRows[1], MergeSelector);
            Subject.ApplyMergeheader(torrentRows[2], MergeSelector);
            Subject.ApplyMergeheader(torrentRows[3], MergeSelector);

            torrentRows[0].TextContent.Should().Contain("torrent 1");
            torrentRows[0].TextContent.Should().Contain("Dual");
            torrentRows[0].TextContent.Should().NotContain("Legendado");

            torrentRows[1].TextContent.Should().Contain("torrent 2");
            torrentRows[1].TextContent.Should().Contain("Dual");
            torrentRows[1].TextContent.Should().NotContain("Legendado");

            torrentRows[2].TextContent.Should().Contain("torrent 3");
            torrentRows[2].TextContent.Should().Contain("Legendado");
            torrentRows[2].TextContent.Should().NotContain("Dual");

            torrentRows[3].TextContent.Should().Contain("torrent 4");
            torrentRows[3].TextContent.Should().Contain("Legendado");
            torrentRows[3].TextContent.Should().NotContain("Dual");
        }

        [Test]
        public void should_clone_header_so_original_row_remains_for_later_torrents()
        {
            var document = ParseHtml(SampleHtml);
            var headerRow = document.QuerySelector("tbody > tr:first-child");
            var torrentRows = GetTorrentRows(SampleHtml);

            Subject.ApplyMergeheader(torrentRows[0], MergeSelector);
            Subject.ApplyMergeheader(torrentRows[1], MergeSelector);

            headerRow.QuerySelector(MergeSelector).Should().NotBeNull();
            headerRow.QuerySelector(MergeSelector).TextContent.Should().Be("Dual");
            torrentRows[0].QuerySelectorAll(MergeSelector).Should().HaveCount(1);
            torrentRows[1].QuerySelectorAll(MergeSelector).Should().HaveCount(1);
        }

        [Test]
        public void should_leave_row_unchanged_when_no_previous_header_exists()
        {
            var html = """
                <table>
                  <tbody>
                    <tr class="torrent"><td>torrent 1</td></tr>
                  </tbody>
                </table>
                """;
            var torrentRow = GetTorrentRows(html)[0];
            var originalHtml = torrentRow.OuterHtml;

            Subject.ApplyMergeheader(torrentRow, MergeSelector);

            torrentRow.OuterHtml.Should().Be(originalHtml);
            torrentRow.QuerySelector(MergeSelector).Should().BeNull();
        }

        [Test]
        public void should_use_parent_fallback_when_previous_sibling_is_in_another_container()
        {
            var html = """
                <table>
                  <tbody>
                    <tr><td class="audio_header edition_info">Dual</td></tr>
                  </tbody>
                  <tbody>
                    <tr class="torrent"><td>torrent 1</td></tr>
                  </tbody>
                </table>
                """;
            var torrentRow = GetTorrentRows(html)[0];

            Subject.ApplyMergeheader(torrentRow, MergeSelector);

            torrentRow.TextContent.Should().Contain("torrent 1");
            torrentRow.TextContent.Should().Contain("Dual");
        }

        [Test]
        public void find_previous_sibling_match_should_return_null_when_no_match_exists()
        {
            var html = """
                <table>
                  <tbody>
                    <tr class="torrent"><td>torrent 1</td></tr>
                  </tbody>
                </table>
                """;
            var torrentRow = GetTorrentRows(html)[0];

            Subject.FindMatch(torrentRow, MergeSelector).Should().BeNull();
        }

        private static List<IElement> GetTorrentRows(string html)
        {
            var document = ParseHtml(html);
            var rows = new List<IElement>();

            foreach (var row in document.QuerySelectorAll("tr.torrent"))
            {
                rows.Add(row);
            }

            return rows;
        }

        private static IHtmlDocument ParseHtml(string html)
        {
            return new HtmlParser().ParseDocument(html);
        }
    }

    public class CardigannMergeheaderTestSubject : CardigannBase
    {
        public CardigannMergeheaderTestSubject(IConfigService configService, CardigannDefinition definition, NLog.Logger logger)
            : base(configService, definition, logger)
        {
        }

        public IElement FindMatch(IElement row, string selector) => FindPreviousSiblingMatch(row, selector);

        public void ApplyMergeheader(IElement row, string selector)
        {
            var match = FindPreviousSiblingMatch(row, selector);
            if (match != null)
            {
                row.Append(match.Clone(true));
            }
        }
    }
}
