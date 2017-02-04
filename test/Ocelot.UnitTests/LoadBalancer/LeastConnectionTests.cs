using System.Collections.Generic;
using System.Threading.Tasks;
using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.Responses;
using Ocelot.Values;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.LoadBalancer
{
    public class LeastConnectionTests
    {
        private HostAndPort _hostAndPort;
        private Response<HostAndPort> _result;
        private LeastConnectionLoadBalancer _leastConnection;
        private List<Service> _services;

        [Fact]
        public void should_get_next_url()
        {
            var serviceName = "products";

            var hostAndPort = new HostAndPort("localhost", 80);

            var availableServices = new List<Service>
            {
                new Service(serviceName, hostAndPort, string.Empty, string.Empty, new string[0])
            };

            this.Given(x => x.GivenAHostAndPort(hostAndPort))
            .And(x => x.GivenTheLoadBalancerStarts(availableServices, serviceName))
            .When(x => x.WhenIGetTheNextHostAndPort())
            .Then(x => x.ThenTheNextHostAndPortIsReturned())
            .BDDfy();
        }

        [Fact]
        public void should_serve_from_service_with_least_connections()
        {
            var serviceName = "products";

            var availableServices = new List<Service>
            {
                new Service(serviceName, new HostAndPort("127.0.0.1", 80), string.Empty, string.Empty, new string[0]),
                new Service(serviceName, new HostAndPort("127.0.0.2", 80), string.Empty, string.Empty, new string[0]),
                new Service(serviceName, new HostAndPort("127.0.0.3", 80), string.Empty, string.Empty, new string[0])
            };

            _services = availableServices;
            _leastConnection = new LeastConnectionLoadBalancer(() => Task.FromResult(_services), serviceName);

            var response = _leastConnection.Lease().Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[0].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease().Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[1].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease().Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[2].HostAndPort.DownstreamHost);
        }

        [Fact]
        public void should_build_connections_per_service()
        {
             var serviceName = "products";

            var availableServices = new List<Service>
            {
                new Service(serviceName, new HostAndPort("127.0.0.1", 80), string.Empty, string.Empty, new string[0]),
                new Service(serviceName, new HostAndPort("127.0.0.2", 80), string.Empty, string.Empty, new string[0]),
            };

            _services = availableServices;
            _leastConnection = new LeastConnectionLoadBalancer(() => Task.FromResult(_services), serviceName);

            var response = _leastConnection.Lease().Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[0].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease().Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[1].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease().Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[0].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease().Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[1].HostAndPort.DownstreamHost);
        }

        [Fact]
        public void should_release_connection()
        {
             var serviceName = "products";

            var availableServices = new List<Service>
            {
                new Service(serviceName, new HostAndPort("127.0.0.1", 80), string.Empty, string.Empty, new string[0]),
                new Service(serviceName, new HostAndPort("127.0.0.2", 80), string.Empty, string.Empty, new string[0]),
            };

            _services = availableServices;
            _leastConnection = new LeastConnectionLoadBalancer(() => Task.FromResult(_services), serviceName);

            var response = _leastConnection.Lease().Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[0].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease().Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[1].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease().Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[0].HostAndPort.DownstreamHost);

            response = _leastConnection.Lease().Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[1].HostAndPort.DownstreamHost);

            //release this so 2 should have 1 connection and we should get 2 back as our next host and port
            _leastConnection.Release(availableServices[1].HostAndPort);

            response = _leastConnection.Lease().Result;

            response.Data.DownstreamHost.ShouldBe(availableServices[1].HostAndPort.DownstreamHost);
        }

        [Fact]
        public void should_return_error_if_services_are_null()
        {
            var serviceName = "products";

            var hostAndPort = new HostAndPort("localhost", 80);
               this.Given(x => x.GivenAHostAndPort(hostAndPort))
                .And(x => x.GivenTheLoadBalancerStarts(null, serviceName))
                .When(x => x.WhenIGetTheNextHostAndPort())
                .Then(x => x.ThenServiceAreNullErrorIsReturned())
                .BDDfy();
        }

        [Fact]
        public void should_return_error_if_services_are_empty()
        {
            var serviceName = "products";

            var hostAndPort = new HostAndPort("localhost", 80);
               this.Given(x => x.GivenAHostAndPort(hostAndPort))
                .And(x => x.GivenTheLoadBalancerStarts(new List<Service>(), serviceName))
                .When(x => x.WhenIGetTheNextHostAndPort())
                .Then(x => x.ThenServiceAreEmptyErrorIsReturned())
                .BDDfy();
        }

        private void ThenServiceAreNullErrorIsReturned()
        {
            _result.IsError.ShouldBeTrue();
            _result.Errors[0].ShouldBeOfType<ServicesAreNullError>();
        }

        private void ThenServiceAreEmptyErrorIsReturned()
        {
            _result.IsError.ShouldBeTrue();
            _result.Errors[0].ShouldBeOfType<ServicesAreEmptyError>();
        }

        private void GivenTheLoadBalancerStarts(List<Service> services, string serviceName)
        {
            _services = services;
            _leastConnection = new LeastConnectionLoadBalancer(() => Task.FromResult(_services), serviceName);
        }

        private void WhenTheLoadBalancerStarts(List<Service> services, string serviceName)
        {
            GivenTheLoadBalancerStarts(services, serviceName);
        }

        private void GivenAHostAndPort(HostAndPort hostAndPort)
        {
            _hostAndPort = hostAndPort;
        }

        private void WhenIGetTheNextHostAndPort()
        {
            _result = _leastConnection.Lease().Result;
        }

        private void ThenTheNextHostAndPortIsReturned()
        {
            _result.Data.DownstreamHost.ShouldBe(_hostAndPort.DownstreamHost);
            _result.Data.DownstreamPort.ShouldBe(_hostAndPort.DownstreamPort);
        }
    }
}