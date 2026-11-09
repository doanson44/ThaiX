namespace ThaiX.Application.Common.Interfaces.Messaging;

public interface IAppSerializableCommand<out TResponse> : IAppCommand<TResponse>;
