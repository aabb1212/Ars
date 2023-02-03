﻿using Ars.Common.Consul.GrpcHelper;
using GrpcGreeter.greet;
using Microsoft.Extensions.Hosting;

namespace GrpcClients
{
    public class ChannelService : BackgroundService
    {
        private readonly IChannelProvider _channelProvider;
        private readonly IGrpcClientProvider _grpcClientProvider;
        public ChannelService(
            IChannelProvider channelProvider,
            IGrpcClientProvider grpcClientProvider)
        {
            _channelProvider = channelProvider;
            _grpcClientProvider = grpcClientProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var client = await _grpcClientProvider.GetGrpcClient<Greeter.GreeterClient>("apigrpc");
            using var req = client.StreamingFromClient();

            var channel = _channelProvider.GetOrAddChannel<StreamingRequest>("grpc");
            while (await channel.Reader.WaitToReadAsync())
            {
                while (channel.Reader.TryRead(out var msg))
                {
                    await req.RequestStream.WriteAsync(msg);
                }
            }

            return;
        }
    }
}
