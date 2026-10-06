using MediatR;

namespace ThaiX.Application.Common.Interfaces.Messaging;

public interface IAppCommand<out TResponse> : IRequest<TResponse>;

public interface IAppQuery<out TResponse> : IRequest<TResponse>;

public interface IAppNonTransactionalCommand<out TResponse> : IRequest<TResponse>;