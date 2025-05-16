using MediFlow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MediFlow.Implementations
{
    public class Mediflow : IMediFlow
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediflow(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            var handlerType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                await (Task)handlerType
                    .GetMethod("Handle")!
                    .Invoke(handler, new object[] { notification, cancellationToken })!;
            }
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var handlerTyp = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceProvider.GetService(handlerTyp);

            if (handler == null)
            {
                throw new InvalidOperationException($"Handler Not Found {request.GetType().Name}");
            }

            return await (Task<TResponse>)handlerTyp
                .GetMethod("Handler")
                .Invoke(handler, new object[] { request, cancellationToken });
        }
    }
}
