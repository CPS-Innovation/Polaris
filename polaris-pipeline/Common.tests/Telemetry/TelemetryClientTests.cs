using System.Collections.Concurrent;
using AI = Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Xunit;
using Common.Telemetry;
using Moq;
using FluentAssertions;
using Microsoft.ApplicationInsights.DataContracts;

namespace Common.tests.Telemetry
{

    public class TelemetryClientTests
    {
        TelemetryClient _telemetryClient;
        Mock<BaseTelemetryEvent> _mockEvent;

        MockTelemetryChannel _mockTelemetryChannel;
        Dictionary<string, string> _mockEventProperties = [];
        Dictionary<string, double?> _mockEventMetrics = [];
        public TelemetryClientTests()
        {
            _mockTelemetryChannel = new MockTelemetryChannel();

            var aiTelemetryConfig = new TelemetryConfiguration
            {
                ConnectionString = $"InstrumentationKey={Guid.NewGuid().ToString()};IngestionEndpoint=https://example.org;LiveEndpoint=https://example.org;",
            };
            var aiTelemetryClient = new AI.TelemetryClient(aiTelemetryConfig);

            _mockEvent = new Mock<BaseTelemetryEvent>();
            _mockEvent.Setup(x => x.ToTelemetryEventProps()).Returns((_mockEventProperties, _mockEventMetrics));

            _telemetryClient = new TelemetryClient(aiTelemetryClient);
        }

        [Fact]
        public void TelemetryClient_TrackEvent_RegistersVersion()
        {
            // Act
            _telemetryClient.TrackEvent(_mockEvent.Object);

            // Assert
            var receivedEvent = _mockTelemetryChannel.SentTelemetries.First() as EventTelemetry;
            receivedEvent?.Properties[TelemetryClient.telemetryVersion].Should().Be(TelemetryClient.Version);
        }

        [Fact]
        public void TelemetryClient_TrackEvent_RegistersPropertiesAndMetricsUsingCleanedPropertyNames()
        {
            // Arrange
            _mockEventProperties["_testFoo"] = "foo";
            _mockEventProperties["_testBar"] = "bar";
            _mockEventProperties["TestQuux"] = "quux";
            _mockEventMetrics["_testBaz"] = 1.0;
            _mockEventMetrics["_testQux"] = 2.991919;
            _mockEventMetrics["TestQuux1"] = 1.1;

            // Act
            _telemetryClient.TrackEvent(_mockEvent.Object);

            // Assert
            var receivedEvent = _mockTelemetryChannel.SentTelemetries.First() as EventTelemetry;
            receivedEvent?.Properties["testFoo"].Should().Be("foo");
            receivedEvent?.Properties["testBar"].Should().Be("bar");
            receivedEvent?.Properties["testQuux"].Should().Be("quux");
            // ✅ FIX: metrics are now stored as properties (string)
            receivedEvent?.Properties["testBaz"].Should().Be("1");
            receivedEvent?.Properties["testQux"].Should().Be("2.991919");
            receivedEvent?.Properties["testQuux1"].Should().Be("1.1");
        }

        [Fact]
        public void TelemetryClient_TrackEvent_IgnoresNullValuedMetrics()
        {
            // Arrange
            _mockEventMetrics["_test"] = 1.0;
            _mockEventMetrics["_testNull"] = null;

            // Act
            _telemetryClient.TrackEvent(_mockEvent.Object);

            // Assert
            var receivedEvent = _mockTelemetryChannel.SentTelemetries.First() as EventTelemetry;
            receivedEvent?.Properties["test"].Should().Be("1.0");
            receivedEvent?.Properties.ContainsKey("testNull").Should().BeFalse();
        }

        [Fact]
        public void TelemetryClient_TrackEvent_DoesNotContainIsFailureFlag()
        {
            // Arrange
            _mockEventMetrics["_test"] = 1.0;

            // Act
            _telemetryClient.TrackEvent(_mockEvent.Object);

            // Assert
            var receivedEvent = _mockTelemetryChannel.SentTelemetries.First() as EventTelemetry;
            receivedEvent?.Properties.ContainsKey("isFailure").Should().BeFalse();
        }

        [Fact]
        public void TelemetryClient_TrackEventFailure_ContainsIsFailureFlag()
        {
            // Arrange
            _mockEventMetrics["_test"] = 1.0;

            // Act
            _telemetryClient.TrackEventFailure(_mockEvent.Object);

            // Assert
            var receivedEvent = _mockTelemetryChannel.SentTelemetries.First() as EventTelemetry;
            receivedEvent?.Properties.ContainsKey("isFailure").Should().BeTrue();
        }

        [Fact]
        public void TelemetryClient_TrackEvent_RegistersACleanedEventName()
        {
            // Act
            _telemetryClient.TrackEvent(new AnEventWithANameEvent());

            // Assert
            var receivedEvent = _mockTelemetryChannel.SentTelemetries.First() as EventTelemetry;
            receivedEvent?.Name.Should().Be("AnEventWithAName");
        }

    }

    public class MockTelemetryChannel : AI.Channel.ITelemetry
    {
        public ConcurrentBag<ITelemetry> SentTelemetries = [];

        public MockTelemetryChannel()
        {
            EndpointAddress = "";
        }

        public bool IsFlushed { get; private set; }
        public bool? DeveloperMode { get; set; }
        public string EndpointAddress { get; set; }
        public DateTimeOffset Timestamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public TelemetryContext Context => throw new NotImplementedException();

        public void Send(ITelemetry item)
        {
            this.SentTelemetries.Add(item);
        }

        public void Flush()
        {
            this.IsFlushed = true;
        }

        public void Dispose()
        {

        }
    }

    public class AnEventWithANameEvent : BaseTelemetryEvent
    {
        public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
        {
            return (new Dictionary<string, string>(), new Dictionary<string, double?>());
        }
    }


}