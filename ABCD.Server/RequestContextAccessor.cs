using ABCD.Application;

namespace ABCD.Server {
    public class RequestContextAccessor {
        private RequestContext? _requestContext;
        public RequestContext RequestContext {
            get => _requestContext ?? throw new InvalidOperationException("ApplicationContext has not been set.");
            internal set
            {
                if (_requestContext is not null)
                    throw new InvalidOperationException("ApplicationContext can only be set once per scope.");

                _requestContext = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
    }
}
