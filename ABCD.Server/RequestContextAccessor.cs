using ABCD.Application;
using ABCD.Domain.Exceptions;

namespace ABCD.Server {
    public class RequestContextAccessor {
        private RequestContext? _requestContext;
        public RequestContext RequestContext {
            get => _requestContext ?? throw new IllegalOperationException("ApplicationContext has not been set.");
            internal set
            {
                if (_requestContext is not null)
                    throw new IllegalOperationException("ApplicationContext can only be set once per scope.");

                _requestContext = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
    }
}
