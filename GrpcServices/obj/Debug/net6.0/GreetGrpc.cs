// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: greet.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace GrpcGreeter.greet {
  /// <summary>
  /// The greeting service definition.
  /// </summary>
  public static partial class Greeter
  {
    static readonly string __ServiceName = "greet.Greeter";

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static void __Helper_SerializeMessage(global::Google.Protobuf.IMessage message, grpc::SerializationContext context)
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (message is global::Google.Protobuf.IBufferMessage)
      {
        context.SetPayloadLength(message.CalculateSize());
        global::Google.Protobuf.MessageExtensions.WriteTo(message, context.GetBufferWriter());
        context.Complete();
        return;
      }
      #endif
      context.Complete(global::Google.Protobuf.MessageExtensions.ToByteArray(message));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static class __Helper_MessageCache<T>
    {
      public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static T __Helper_DeserializeMessage<T>(grpc::DeserializationContext context, global::Google.Protobuf.MessageParser<T> parser) where T : global::Google.Protobuf.IMessage<T>
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (__Helper_MessageCache<T>.IsBufferMessage)
      {
        return parser.ParseFrom(context.PayloadAsReadOnlySequence());
      }
      #endif
      return parser.ParseFrom(context.PayloadAsNewBuffer());
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::GrpcGreeter.greet.HelloRequest> __Marshaller_greet_HelloRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::GrpcGreeter.greet.HelloRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::GrpcGreeter.greet.HelloReply> __Marshaller_greet_HelloReply = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::GrpcGreeter.greet.HelloReply.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::GrpcGreeter.greet.StreamingRequest> __Marshaller_greet_StreamingRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::GrpcGreeter.greet.StreamingRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::GrpcGreeter.greet.StreamingResponse> __Marshaller_greet_StreamingResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::GrpcGreeter.greet.StreamingResponse.Parser));

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::GrpcGreeter.greet.HelloRequest, global::GrpcGreeter.greet.HelloReply> __Method_SayHello = new grpc::Method<global::GrpcGreeter.greet.HelloRequest, global::GrpcGreeter.greet.HelloReply>(
        grpc::MethodType.Unary,
        __ServiceName,
        "SayHello",
        __Marshaller_greet_HelloRequest,
        __Marshaller_greet_HelloReply);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::GrpcGreeter.greet.StreamingRequest, global::GrpcGreeter.greet.StreamingResponse> __Method_StreamingFromServer = new grpc::Method<global::GrpcGreeter.greet.StreamingRequest, global::GrpcGreeter.greet.StreamingResponse>(
        grpc::MethodType.ServerStreaming,
        __ServiceName,
        "StreamingFromServer",
        __Marshaller_greet_StreamingRequest,
        __Marshaller_greet_StreamingResponse);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::GrpcGreeter.greet.StreamingRequest, global::GrpcGreeter.greet.StreamingResponse> __Method_StreamingFromClient = new grpc::Method<global::GrpcGreeter.greet.StreamingRequest, global::GrpcGreeter.greet.StreamingResponse>(
        grpc::MethodType.ClientStreaming,
        __ServiceName,
        "StreamingFromClient",
        __Marshaller_greet_StreamingRequest,
        __Marshaller_greet_StreamingResponse);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::GrpcGreeter.greet.StreamingRequest, global::GrpcGreeter.greet.StreamingResponse> __Method_streamingBothWays = new grpc::Method<global::GrpcGreeter.greet.StreamingRequest, global::GrpcGreeter.greet.StreamingResponse>(
        grpc::MethodType.DuplexStreaming,
        __ServiceName,
        "streamingBothWays",
        __Marshaller_greet_StreamingRequest,
        __Marshaller_greet_StreamingResponse);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::GrpcGreeter.greet.GreetReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of Greeter</summary>
    [grpc::BindServiceMethod(typeof(Greeter), "BindService")]
    public abstract partial class GreeterBase
    {
      /// <summary>
      /// Sends a greeting
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::GrpcGreeter.greet.HelloReply> SayHello(global::GrpcGreeter.greet.HelloRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task StreamingFromServer(global::GrpcGreeter.greet.StreamingRequest request, grpc::IServerStreamWriter<global::GrpcGreeter.greet.StreamingResponse> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::GrpcGreeter.greet.StreamingResponse> StreamingFromClient(grpc::IAsyncStreamReader<global::GrpcGreeter.greet.StreamingRequest> requestStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task streamingBothWays(grpc::IAsyncStreamReader<global::GrpcGreeter.greet.StreamingRequest> requestStream, grpc::IServerStreamWriter<global::GrpcGreeter.greet.StreamingResponse> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static grpc::ServerServiceDefinition BindService(GreeterBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_SayHello, serviceImpl.SayHello)
          .AddMethod(__Method_StreamingFromServer, serviceImpl.StreamingFromServer)
          .AddMethod(__Method_StreamingFromClient, serviceImpl.StreamingFromClient)
          .AddMethod(__Method_streamingBothWays, serviceImpl.streamingBothWays).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the  service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static void BindService(grpc::ServiceBinderBase serviceBinder, GreeterBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_SayHello, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::GrpcGreeter.greet.HelloRequest, global::GrpcGreeter.greet.HelloReply>(serviceImpl.SayHello));
      serviceBinder.AddMethod(__Method_StreamingFromServer, serviceImpl == null ? null : new grpc::ServerStreamingServerMethod<global::GrpcGreeter.greet.StreamingRequest, global::GrpcGreeter.greet.StreamingResponse>(serviceImpl.StreamingFromServer));
      serviceBinder.AddMethod(__Method_StreamingFromClient, serviceImpl == null ? null : new grpc::ClientStreamingServerMethod<global::GrpcGreeter.greet.StreamingRequest, global::GrpcGreeter.greet.StreamingResponse>(serviceImpl.StreamingFromClient));
      serviceBinder.AddMethod(__Method_streamingBothWays, serviceImpl == null ? null : new grpc::DuplexStreamingServerMethod<global::GrpcGreeter.greet.StreamingRequest, global::GrpcGreeter.greet.StreamingResponse>(serviceImpl.streamingBothWays));
    }

  }
}
#endregion
